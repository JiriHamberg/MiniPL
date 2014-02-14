using System;

namespace CompilersProject
{
	public struct Token
	{
		public Category category;
		public String lexeme;
		public int line;
		public int column;


		public Token (Category category, String lexeme, int line, int column)
		{
			this.category = category;
			this.lexeme = lexeme;
			this.line = line;
			this.column = column;
		}

		public static Token ErrorToken ()
		{
			return new Token(Category.NONE, "", -1, -1);
		}


		public bool IsOperand ()
		{
			return  category == Category.Identifier ||
					category == Category.Literal_String ||
					category == Category.Literal_Integer;
		}


		public override string ToString ()
		{
			return lexeme;
		}

	}
}

