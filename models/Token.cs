using System;

namespace CompilersProject
{
	public struct Token
	{
		public TokenCategory category;
		public String lexeme;
		public int line;
		public int column;


		public Token (TokenCategory category, String lexeme, int line, int column)
		{
			this.category = category;
			this.lexeme = lexeme;
			this.line = line;
			this.column = column;
		}

		public static Token errorToken ()
		{
			return new Token(TokenCategory.NONE, "", -1, -1);
		}
	}
}

