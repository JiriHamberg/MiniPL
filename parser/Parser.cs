using System;

namespace CompilersProject
{
	public class Parser
	{

		private Scanner scanner;

		//current token
		private Token token;
		//last token
		private Token accepted;

		//ast that we are building
		private AbstractSyntaxTree ast = new AbstractSyntaxTree();

		public Parser (Scanner scanner)
		{
			this.scanner = scanner;
		}

		public void Parse ()
		{
			getNextToken ();
			while (scanner.HasNext()) {
				statements ();
			}
		}


		private void getNextToken ()
		{
			//todo: handle end of stream !!!

			token = scanner.Next ();
		}

		private bool accept (params TokenCategory[] categories)
		{
			foreach (TokenCategory category in categories) {
				if(category == token.category) {
					accepted = token;
					getNextToken ();
					return true;
				}
			}
			accepted = Token.errorToken();
			return false;
		}

		private bool expect (params TokenCategory [] categories)
		{
			foreach (TokenCategory category in categories) {
				if (accept(category)) return true;
			}
			addError (token, categories);
			//getNextToken ();
			recover ();
			return false;
		}

		private void addError (Token found, params TokenCategory[] expected)
		{
			//todo: implement... maybe a container class for errors?
			string error = "Error: line " + found.line + " column " + found.column;
			error = error + " expecting " + expected [0];
			for (int i=1; i <expected.Length; i++) {
				error = error + " or " + expected[i];
			}
			error = error +	", but found " + found.category;
			System.Console.WriteLine (error);
			System.Console.WriteLine (token.lexeme);
			//System.Console.ReadLine(); 
		}

		//naive recovery strategy: continue parsing from the next semicolon
		private void recover ()
		{
			while (scanner.HasNext()) {
				token = scanner.Next();
				if(token.category == TokenCategory.Semicolon) {
					token = scanner.Next ();
				}
			}
		}


		/*
		 *  Start parser spesification
		 */

		private Statements statements ()
		{
			Statements ret = new Statements();
			ret.addStatement(statement ());
			expect (TokenCategory.Semicolon);

			//maximum munch
			TokenCategory next = token.category;
			while (next == TokenCategory.Keyword_Var || 
				next == TokenCategory.Identifier ||
				next == TokenCategory.Identifier ||
				next == TokenCategory.Keyword_For ||
				next == TokenCategory.Function_Read ||
				next == TokenCategory.Function_Print ||
				next == TokenCategory.Function_Assert) {
				//statements ();
				ret.addStatement(statement ());
				expect (TokenCategory.Semicolon);
			}
			return ret;
		}

		private Statement statement ()
		{
			//variable declaration
			if (accept (TokenCategory.Keyword_Var)) {
				expect (TokenCategory.Identifier);
				expect (TokenCategory.Colon);
				expect (TokenCategory.Type_Boolean, 
				        TokenCategory.Type_Integer,
				        TokenCategory.Type_String);
				if (accept (TokenCategory.Operator_Assignment)) {
					expression ();
				}
				//assignment
			} else if (accept (TokenCategory.Identifier)) {
				expect (TokenCategory.Operator_Assignment);
				expression ();
				//for loop
			} else if (accept (TokenCategory.Keyword_For)) {
				expect (TokenCategory.Identifier);
				expect (TokenCategory.Keyword_In);
				expression ();
				expect (TokenCategory.Loop_Range);
				expression ();
				expect (TokenCategory.Keyword_Do);
				statements ();
				expect (TokenCategory.Keyword_End);
				expect (TokenCategory.Keyword_For);
				//read
			} else if (accept (TokenCategory.Function_Read)) {
				expect (TokenCategory.Identifier);
				//print
			} else if (accept (TokenCategory.Function_Print)) {
				expression ();
				//assert
			} else if (accept (TokenCategory.Function_Assert)) {
				expect (TokenCategory.Left_Bracket);
				expression ();
				expect (TokenCategory.Rigth_Bracket);
			} else {
				//error? expecting a statement
			}
		}

		private void expression ()
		{
			if (accept (TokenCategory.Operator_Not)) {
				operand ();
			}else {
				operand ();
				if (accept (TokenCategory.Operator_Addition,
				        TokenCategory.Operator_Division,
				        TokenCategory.Operator_Multiplication,
				        TokenCategory.Operator_Substraction,
				        TokenCategory.Operator_Equality,
				        TokenCategory.Operator_Less,
					    TokenCategory.Operator_And)) {
					operand ();
				}
			}
		}

		private void operand ()
		{
			//literal or identifier
			if (accept (TokenCategory.Literal_Integer, 
						TokenCategory.Literal_String,
						TokenCategory.Identifier)) {
			//nested expression
			} else if (accept (TokenCategory.Left_Bracket)) {
				expression ();
				expect (TokenCategory.Rigth_Bracket);
			} else {
				addError(token, TokenCategory.Literal_Integer, 
						 TokenCategory.Literal_String,
						 TokenCategory.Identifier,
				         TokenCategory.Left_Bracket);
			}
		}


	}
}

