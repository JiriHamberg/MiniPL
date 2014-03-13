
namespace CompilersProject
{
	public enum Category
	{
		Identifier,
		//literals
		Literal_Integer,
		Literal_String,
		//Literal_Boolean, boolean literal not included according to language spec

		//operators
		Binary_Operator,
		Unary_Operator,

		Assignment,

		Type,

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

		//end of file
		End_Of_File,

		//error
		NONE
	}
}

