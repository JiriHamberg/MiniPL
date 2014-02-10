using System;

namespace CompilersProject
{
	public enum TokenCategory
	{
		Identifier,

		//literals
		Literal_Integer,
		Literal_String,
		Literal_Boolean,

		//arithmetic operations
		Operator_Assignment,
		Operator_Addition,
		Operator_Substraction,
		Operator_Multiplication,
		Operator_Division,

		//boolean operators
		Operator_And,
		Operator_Not,
		Operator_Equality,
		Operator_Less,

		//(primitive) types
		Type_Integer,
		Type_String,
		Type_Boolean,

		//keywords
		Keyword_For,
		Keyword_In,
		Keyword_Do,
		Keyword_Var,
		Keyword_End,

		//(primitive functions)
		Function_Print,
		Function_Read,
		Function_Assert,

		//misc
		Semicolon,
		Colon,
		Left_Bracket,
		Rigth_Bracket,
		Loop_Range,

		//error
		NONE
	}
}

