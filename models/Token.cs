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

		public static Token errorToken ()
		{
			return new Token(Category.NONE, "", -1, -1);
		}

		public bool isBinaryOperator ()
		{
			return  category == Category.Operator_Addition ||
					category == Category.Operator_Substraction ||
					category == Category.Operator_Multiplication ||
					category == Category.Operator_Division ||
					category == Category.Operator_And ||
					category == Category.Operator_Equality ||
					category == Category.Operator_Less;
		}

		public bool isUnaryOperator ()
		{
			return category == Category.Operator_Not;
		}

		public bool isOperand ()
		{
			return  category == Category.Identifier ||
					category == Category.Literal_String ||
					category == Category.Literal_Integer ||
					category == Category.Literal_Boolean;
		}


		public override string ToString ()
		{
			return lexeme;
		}

	}
}

