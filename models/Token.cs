using System;

namespace CompilersProject
{
	public struct Token
	{
		public Category Category;
		public String Lexeme;
		public int Line;
		public int Column;


		public Token (Category category, String lexeme, int line, int column)
		{
			this.Category = category;
			this.Lexeme = lexeme;
			this.Line = line;
			this.Column = column;
		}

		public static Token ErrorToken ()
		{
			return new Token(Category.NONE, "", -1, -1);
		}


		public bool IsOperand ()
		{
			return  Category == Category.Identifier ||
					Category == Category.Literal_String ||
					Category == Category.Literal_Integer;
		}


		public override string ToString ()
		{
			return Lexeme;
		}

	}
}

