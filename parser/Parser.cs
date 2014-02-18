using System;

namespace CompilersProject
{
	public class Parser
	{

		private Scanner scanner;
		private ErrorContainer errors;

		//current token
		private Token token;
		//last token
		private Token accepted;

		//ast that we are building
		//private AbstractSyntaxTree ast = new AbstractSyntaxTree();

		public Parser (Scanner scanner, ErrorContainer errors)
		{
			this.scanner = scanner;
			this.errors = errors;
		}

		public AbstractSyntaxTree Parse ()
		{
			AbstractSyntaxTree ast = new AbstractSyntaxTree();
			getNextToken ();
			while (scanner.HasNext()) {
				ast.Statements.MergeStatements(statements ());
			}
			return ast;
		}


		private void getNextToken ()
		{
			//todo: handle end of stream !!!
			if (scanner.HasNext ()) {
				token = scanner.Next ();
			} else {
				token = Token.ErrorToken();
			}
		}

		private bool accept (params Category[] categories)
		{
			foreach (Category category in categories) {
				if(category == token.Category) {
					accepted = token;
					getNextToken ();
					return true;
				}
			}
			accepted = Token.ErrorToken();
			return false;
		}

		private bool expect (params Category [] categories)
		{
			foreach (Category category in categories) {
				if (accept(category)) return true;
			}
			addError (token, categories);
			recover ();
			return false;
		}

		private void addError (Token found, params Category[] expected)
		{
			string errorMsg = "Expecting " + expected [0];
			for (int i=1; i <expected.Length; i++) {
				errorMsg = errorMsg + " or " + expected[i];
			}
			errorMsg = errorMsg +	", but found " + found.Category;
			errors.AddError(token, ErrorType.Syntax_Error, errorMsg); 
		}

		//naive recovery strategy: continue parsing from the next token
		//todo: implement properly
		private void recover ()
		{
			/*while (scanner.HasNext()) {
				token = scanner.Next();
				if(token.category == Category.Semicolon) {
					if(scanner.HasNext()) {
						token = scanner.Next ();
					} else {
						token = Token.errorToken();
					}
				}
			}*/
			getNextToken ();
		}


		/*
		 *  Actual parsing and builging ast
		 */

		private Statements statements ()
		{
			Statements ret = new Statements();
			ret.AddStatement(statement ());
			expect (Category.Semicolon);

			//maximum munch
			while (token.Category == Category.Keyword_Var || 
				token.Category == Category.Identifier ||
				token.Category == Category.Keyword_For ||
				token.Category == Category.Function_Read ||
				token.Category == Category.Function_Print ||
				token.Category == Category.Function_Assert) {
				ret.AddStatement(statement ());
				expect (Category.Semicolon);
			}
			return ret;
		}

		private Statement statement ()
		{
			Statement ret = null;
			//variable declaration
			if (accept (Category.Keyword_Var)) {
				ret = new Declaration();
				expect (Category.Identifier);
				((Declaration)ret).Identifier = accepted;
				expect (Category.Colon);
				expect (Category.Type);
				((Declaration)ret).Type = accepted;
				if (accept (Category.Assignment)) {
					((Declaration)ret).Expression = expression ();
				}
				//assignment
			} else if (accept (Category.Identifier)) {
				ret = new Assignment();
				((Assignment)ret).Identifier = accepted;
				if (expect (Category.Assignment)) {
					((Assignment)ret).Expression = expression ();
				} else {
					//System.Console.WriteLine("INVALID ASSIGNMENT!!!");
					errors.AddError(token, ErrorType.Syntax_Error, "Invalid assignment; expression expected");
				}
				//for loop
			} else if (accept (Category.Keyword_For)) {
				ret = new ForLoop();
				expect (Category.Identifier);
				((ForLoop)ret).Variable = accepted;
				expect (Category.Keyword_In);
				((ForLoop)ret).From = expression ();
				expect (Category.Loop_Range);
				((ForLoop)ret).To = expression ();
				expect (Category.Keyword_Do);
				((ForLoop)ret).Statements = statements ();
				expect (Category.Keyword_End);
				expect (Category.Keyword_For);
				//read
			} else if (accept (Category.Function_Read)) {
				ret = new Read();
				expect (Category.Identifier);
				((Read)ret).Identifier = accepted;
				//print
			} else if (accept (Category.Function_Print)) {
				ret = new Print();
				((Print)ret).Expression = expression ();
				//assert
			} else if (accept (Category.Function_Assert)) {
				ret = new Assert();
				((Assert)ret).Location = accepted;
				expect (Category.Left_Bracket);
				((Assert)ret).Assertion = expression ();
				expect (Category.Rigth_Bracket);
			} else {
				errors.AddError(token, ErrorType.Syntax_Error, "Expecting a statement");
			}
			return ret;
		}

		private Expression expression ()
		{
			ExpressionBuilder builder = new ExpressionBuilder(errors);
			buildExpression(builder);
			return builder.Build();
		}

		private void buildExpression(ExpressionBuilder builder) 
		{
			if (accept (Category.Unary_Operator)) {
				builder.Offer(accepted);
				operand (builder);
			} else {
				operand (builder);
				if (accept (Category.Binary_Operator)) {
					builder.Offer(accepted);
					operand (builder);
				}
			}
		}

		private void operand (ExpressionBuilder builder)
		{
			//literal or identifier
			if (accept (Category.Literal_Integer, 
						Category.Literal_String,
						Category.Identifier)) {
				builder.Offer(accepted);

			//nested expression
			} else if (accept (Category.Left_Bracket)) {
				builder.Offer(accepted);
				buildExpression (builder);
				expect (Category.Rigth_Bracket);
				builder.Offer (accepted);
			} else {
				/*addError(token, Category.Literal_Integer, 
						 Category.Literal_String,
						 Category.Identifier,
				         Category.Left_Bracket);*/
				errors.AddError(token, ErrorType.Syntax_Error, "Expecting an operand");
			}
		}


	}
}

