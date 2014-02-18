using System;
using System.IO;
using System.Collections.Generic;

namespace CompilersProject
{
	public class Scanner
	{
		private const int END_OF_STREAM = -1;

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
		private List<KeyValuePair<Func<char, bool>, Action>> transitionTable;
		private ErrorContainer errors;
		//todo: maybe use linked list as the buffer instead of queue since C# queue sucks
		private Queue<Token> tokenBuffer = new Queue<Token>();
		private StreamReader charStream;

		private char current;
		private int line = 1;
		private int column = 1;
		private String lexeme;
		private Category category;
		private int lexeme_begin_line = -1;
		private int lexeme_begin_column = -1;

		/*
		 * Public methods
		 */

		public Scanner (StreamReader charStream, ErrorContainer errContainer)
		{
			this.transitionTable = new List<KeyValuePair<Func<char, bool>, Action>> ()
			{
				//simple one char lexemes
				transition (
					c => simpleLexemes.ContainsKey(c.ToString()),
					() => simpleLexemes.TryGetValue(current.ToString(), out category) ),

				//identifier or reserved keyword
				transition (
					c => Char.IsLetter(c),
					() => readWhile ( x => !simpleLexemes.ContainsKey(x.ToString()) && x != '.' && !Char.IsWhiteSpace(x)) ),

				//integer literal
				transition (
					c => Char.IsNumber(c),
					() => 
						{
							category = Category.Literal_Integer;
							readWhile (x => Char.IsDigit(x));
						}),

				//string literal
				transition (
					c => c == '"', 
					() => scanString () ),

				//colon or assignment
				transition (
					c => c == ':', 
					() => 
						{
							if(peekChar () == '=') {
									category = Category.Assignment;
									lexeme += nextChar ();
							} else {
								category = Category.Colon;
							}
						}),

				//for loop range
				transition (
					c => c == '.', 
					() =>
						{
							if(peekChar() == '.') {
								category = Category.Loop_Range;
								lexeme += nextChar();
							}
						})
			};

			this.errors = errContainer;
			this.charStream = charStream;
			skipBlank ();
			while (!charStream.EndOfStream) {
				tokenBuffer.Enqueue(scanNextToken());
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

		private Token scanNextToken ()
		{
			lexeme = "";
			category = Category.NONE;
			lexeme_begin_line = -1;
			lexeme_begin_column = -1;

			if (charStream.EndOfStream) {
				throw new EndOfStreamException();
			}

			readLexeme ();
			skipBlank();
			decideCategory(); 

			Token token = new Token ();
			token.Lexeme = lexeme;
			token.Category = category;
			token.Line = lexeme_begin_line;
			token.Column = lexeme_begin_column;
			return token;
		}

		private void readLexeme ()
		{
			if (charStream.EndOfStream) {
				throw new EndOfStreamException ("Character stream ended unexpectedly");
			}

			current = nextChar ();
			lexeme += current;
			lexeme_begin_column = column - 1;
			lexeme_begin_line = line;
			//find the action corresponding to the current character and invoke it
			var match = transitionTable.Find (kvp => kvp.Key (current));
			try {
				match.Value ();
			} catch (NullReferenceException ex) {
				errors.AddError(lexeme_begin_line, lexeme_begin_column, ErrorType.Lexical_Error, "Invalid input character");
			}
			//match.Value();
		}


		private void scanString ()
		{
			category = Category.Literal_String;
			lexeme = "";
			while(!charStream.EndOfStream) {
				readWhile (x => x != '\\' && x != '"');
				int lookup = peekChar ();
				if(lookup == '\\') {
					nextChar ();
					if (!charStream.EndOfStream) {
						switch(peekChar ()) {
							case 'n':
								nextChar ();
								lexeme += '\n';
								break;
							case 't':
								nextChar ();
								lexeme += '\t';
								break;
							default:
								lexeme += nextChar ();
								break;
						}

					} else {
						errors.AddError (lexeme_begin_line, lexeme_begin_column, ErrorType.Lexical_Error, "Unclosed string literal");
					}
				} else if (lookup == '"') { 
					nextChar ();
					break;
				} else {
					errors.AddError (lexeme_begin_line, lexeme_begin_column, ErrorType.Lexical_Error, "Unclosed string literal");
				}
			}
		}


		private void decideCategory () 
		{
			if (category != Category.NONE) { //already decided during readLexeme
				return;
			} else if (reservedWords.ContainsKey (lexeme)) {
				reservedWords.TryGetValue (lexeme, out category);
			} else if (isValidIdentifier (lexeme)){ 
				category = Category.Identifier;
			} else{
				category = Category.NONE; //marks invalid token
				errors.AddError(line, column, ErrorType.Lexical_Error, "Invalid identifier: " + lexeme);
			}
		}

		private void skipBlank ()
		{
			while (!charStream.EndOfStream) {
				if (isBlank (peekChar ())){
					nextChar ();
					continue;
				} else {
					break;
				}
			}
		}

		private int peekChar ()
		{
			if (charStream.EndOfStream) {
				return END_OF_STREAM;
			}
			return charStream.Peek();
		}

		private char nextChar ()
		{
			char c = (char)charStream.Read();
			updateCursor (c);
			return c;
		}

		private bool isBlank (int c)
		{
			return c == ' ' || c == '\t'  || c == '\n';
		}

		private bool isValidIdentifier (string s)
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

		private void readWhile (Func<char, bool> condition)
		{
			while(peekChar() != END_OF_STREAM && condition((char)peekChar ())) {
				lexeme += nextChar ();
			}
		}

		private void updateCursor (char c)
		{
			if (c == '\n') {
				line++;
				column = 1;
			} else {
				column++;
			}
		}

		//helper to hide nasty type-typing
		private KeyValuePair<Func<char, bool>, Action> transition (Func<char, bool> condition, Action effect)
		{
			return new KeyValuePair<Func<char, bool>, Action>(condition, effect);
		}

	}
}

