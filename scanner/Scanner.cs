using System;
using System.IO;
using System.Collections.Generic;

namespace CompilersProject
{
	public class Scanner
	{
		private const int END_OF_STREAM = -1;

		private static Dictionary<char, Category> simpleLexemes = new Dictionary<char, Category>()
		{
			{'(', Category.Left_Bracket},
			{')', Category.Rigth_Bracket},
			{'<', Category.Operator_Less},
			{'=', Category.Operator_Equality},
			{'&', Category.Operator_And},
			{'!', Category.Operator_Not},
			{'+', Category.Operator_Addition},
			{'*', Category.Operator_Multiplication},
			{'/', Category.Operator_Division},
			{';', Category.Semicolon}
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
			{"bool", Category.Type_Boolean},
			{"string", Category.Type_String},
			{"int", Category.Type_Integer}
		};

		//todo: maybe use linked list as the buffer instead of queue since C# queue sucks
		private Queue<Token> tokenBuffer = new Queue<Token>();
		private StreamReader charStream;
		private int line = 1;
		private int column = 1;
 
		private String lexeme;
		private Category category;
		private int lexeme_begin_line = -1;
		private int lexeme_begin_column = -1;

		/*
		 * Public methods
		 */

		public Scanner (StreamReader charStream)
		{
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
			token.lexeme = lexeme;
			token.category = category;
			token.line = lexeme_begin_line;
			token.column = lexeme_begin_column;
			return token;
		}

		private void readLexeme ()
		{
			if (charStream.EndOfStream) {
				throw new EndOfStreamException("Character stream ended unexpectedly");
			}

			char current = nextChar ();
			lexeme  += current;
			lexeme_begin_column = column -1;
			lexeme_begin_line = line;
		
			if (simpleLexemes.ContainsKey (current)) {
				simpleLexemes.TryGetValue(current, out category);
			} 
			else if (current == ':') {
				if(peekChar () == '=') {
					category = Category.Operator_Assignment;
					lexeme += nextChar ();
				}
				else {
					category = Category.Colon;
				}
			} else if (Char.IsLetter (current)) { //indentifier, keyword, function, etc.
				readWhile ( x => Char.IsLetterOrDigit(x));
			} 
			else if (Char.IsNumber (current)) { //integer literal
				category = Category.Literal_Integer;
				readWhile (x => Char.IsDigit(x));
			} 
			else if(current == '"') { //string literal
				category = Category.Literal_String;
				readWhile (x => x != '"');
				lexeme += nextChar ();
			}
			else if ( current == '.') {
				if(peekChar() == '.') {
					category = Category.Loop_Range;
					lexeme += nextChar();
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
				category = Category.NONE; //marks invalid identifier
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

	}
}

