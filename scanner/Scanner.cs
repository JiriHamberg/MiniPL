using System;
using System.IO;
using System.Collections.Generic;

namespace CompilersProject
{
	public class Scanner
	{
		private const int EndOfStream = -1;

		private static Dictionary<string, Category> simpleLexemes = new Dictionary<string, Category>()
		{
			{"(", Category.Left_Bracket},
			{")", Category.Rigth_Bracket},
			{Operators.LESS, Category.Binary_Operator},
			{"=", Category.Binary_Operator},
			{Operators.AND, Category.Binary_Operator},
			{Operators.ADDITION, Category.Binary_Operator},
			{Operators.SUBSTRACTION, Category.Binary_Operator},
			{Operators.MULTIPLICATION, Category.Binary_Operator},
			{Operators.DIVISION, Category.Binary_Operator},
			{Operators.NOT, Category.Unary_Operator},
			{";", Category.Semicolon}
		};

		private static Dictionary<String, Category> reservedWords = new Dictionary<string, Category>()
		{
			{"for", Category.Keyword_For},
			{"do", Category.Keyword_Do},
			{"end", Category.Keyword_End},
			{"in", Category.Keyword_In},
			{"var", Category.Keyword_Var},
			{"assert", Category.Function_Assert},
			{"print", Category.Function_Print},
			{"read", Category.Function_Read},
			{"bool", Category.Type},
			{"string", Category.Type},
			{"int", Category.Type}
		};

		//list of transitions in order of presedence
		List<KeyValuePair<Func<char, bool>, Action>> transitionTable;
		ErrorContainer errors;
		//todo: maybe use linked list as the buffer instead of queue since C# queue sucks
		Queue<Token> tokenBuffer = new Queue<Token>();
		StreamReader charStream;

		char current;
		int line = 1;
		int column = 1;
		String lexeme;
		Category category;
		int lexemeBeginLine = -1;
		int lexemeBeginColumn = -1;

		/*
		 * Public methods
		 */

		public Scanner (StreamReader charStream, ErrorContainer errContainer)
		{
			this.transitionTable = new List<KeyValuePair<Func<char, bool>, Action>> ()
			{
				//simple one char lexemes
				Transition (
					c => simpleLexemes.ContainsKey(c.ToString()),
					() => simpleLexemes.TryGetValue(current.ToString(), out category) ),

				//identifier or reserved keyword
				Transition (
					c => Char.IsLetter(c),
					() => ReadWhile ( x => !simpleLexemes.ContainsKey(x.ToString()) && x != '.' && !Char.IsWhiteSpace(x)) ),

				//integer literal
				Transition (
					c => Char.IsNumber(c),
					() => 
						{
							category = Category.Literal_Integer;
							ReadWhile (x => Char.IsDigit(x));
						}),

				//string literal
				Transition (
					c => c == '"', 
					() => ScanString () ),

				//colon or assignment
				Transition (
					c => c == ':', 
					() => 
						{
							if(PeekChar () == '=') {
									category = Category.Assignment;
									lexeme += NextChar ();
							} else {
								category = Category.Colon;
							}
						}),

				//for loop range
				Transition (
					c => c == '.', 
					() =>
						{
							if(PeekChar() == '.') {
								category = Category.Loop_Range;
								lexeme += NextChar();
							}
						})
			};

			this.errors = errContainer;
			this.charStream = charStream;
			SkipBlank ();
			while (!charStream.EndOfStream) {
				tokenBuffer.Enqueue(ScanNextToken());
			}
		}

		public Token Next ()
		{
			if (!HasNext ()) {
				throw new EndOfStreamException("Scanner has no more tokens");
			}
			return tokenBuffer.Dequeue();
		}

		public Token Peek ()
		{
			if (!HasNext ()) {
				throw new EndOfStreamException("Scanner has no more tokens");
			}
			return tokenBuffer.Peek ();
		}

		public Boolean HasNext ()
		{
			return tokenBuffer.Count > 0;
		}

		/*
		 * Private methods
		 */ 

		Token ScanNextToken ()
		{
			lexeme = "";
			category = Category.NONE;
			lexemeBeginLine = -1;
			lexemeBeginColumn = -1;

			if (charStream.EndOfStream) {
				throw new EndOfStreamException();
			}

			ReadLexeme ();
			SkipBlank();
			DecideCategory(); 

			Token token = new Token ();
			token.Lexeme = lexeme;
			token.Category = category;
			token.Line = lexemeBeginLine;
			token.Column = lexemeBeginColumn;
			return token;
		}

		void ReadLexeme ()
		{
			if (charStream.EndOfStream) {
				throw new EndOfStreamException ("Character stream ended unexpectedly");
			}

			current = NextChar ();
			lexeme += current;
			lexemeBeginColumn = column - 1;
			lexemeBeginLine = line;
			//find the action corresponding to the current character and invoke it
			var match = transitionTable.Find (kvp => kvp.Key (current));
			try {
				match.Value ();
			} catch (NullReferenceException ex) {
				errors.AddError(lexemeBeginLine, lexemeBeginColumn, ErrorType.LexicalError, "Invalid input character");
			}
			//match.Value();
		}


		void ScanString ()
		{
			category = Category.Literal_String;
			lexeme = "";
			while(!charStream.EndOfStream) {
				ReadWhile (x => x != '\\' && x != '"');
				int lookup = PeekChar ();
				if(lookup == '\\') {
					NextChar ();
					if (!charStream.EndOfStream) {
						switch(PeekChar ()) {
							case 'n':
								NextChar ();
								lexeme += '\n';
								break;
							case 't':
								NextChar ();
								lexeme += '\t';
								break;
							default:
								lexeme += NextChar ();
								break;
						}

					} else {
						errors.AddError (lexemeBeginLine, lexemeBeginColumn, ErrorType.LexicalError, "Unclosed string literal");
					}
				} else if (lookup == '"') { 
					NextChar ();
					break;
				} else {
					errors.AddError (lexemeBeginLine, lexemeBeginColumn, ErrorType.LexicalError, "Unclosed string literal");
				}
			}
		}


		void DecideCategory () 
		{
			if (category != Category.NONE) { //already decided during readLexeme
				return;
			} else if (reservedWords.ContainsKey (lexeme)) {
				reservedWords.TryGetValue (lexeme, out category);
			} else if (IsValidIdentifier (lexeme)){ 
				category = Category.Identifier;
			} else{
				category = Category.NONE; //marks invalid token
				errors.AddError(line, column, ErrorType.LexicalError, "Invalid identifier: " + lexeme);
			}
		}

		void SkipBlank ()
		{
			while (!charStream.EndOfStream) {
				if (IsBlank (PeekChar ())){
					NextChar ();
					continue;
				} else {
					break;
				}
			}
		}

		int PeekChar ()
		{
			if (charStream.EndOfStream) {
				return EndOfStream;
			}
			return charStream.Peek();
		}

		char NextChar ()
		{
			char c = (char)charStream.Read();
			UpdateCursor (c);
			return c;
		}

		bool IsBlank (int c)
		{
			return c == ' ' || c == '\t'  || c == '\n';
		}

		bool IsValidIdentifier (string s)
		{
			char [] chars = s.ToCharArray ();
			if (chars.Length < 1 || !Char.IsLetter (chars [0])) {
				return false;
			}
			for (int i=1; i< chars.Length; i++) {
				char c = chars[i];
				if(!(Char.IsLetterOrDigit(c) || c == '_')) {
					return false;
				}
			}
			return true;
		}

		void ReadWhile (Func<char, bool> condition)
		{
			while(PeekChar() != EndOfStream && condition((char)PeekChar ())) {
				lexeme += NextChar ();
			}
		}

		void UpdateCursor (char c)
		{
			if (c == '\n') {
				line++;
				column = 1;
			} else {
				column++;
			}
		}

		//helper to hide nasty type-typing
		KeyValuePair<Func<char, bool>, Action> Transition (Func<char, bool> condition, Action effect)
		{
			return new KeyValuePair<Func<char, bool>, Action>(condition, effect);
		}

	}
}

