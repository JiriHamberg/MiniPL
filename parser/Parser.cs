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
				ast.statements.mergeStatements(statements ());
			}
			return ast;
		}


		private void getNextToken ()
		{
			//todo: handle end of stream !!!
			if (scanner.HasNext ()) {
				token = scanner.Next ();
			} else {
				token = Token.errorToken();
			}
		}

		private bool accept (params Category[] categories)
		{
			foreach (Category category in categories) {
				if(category == token.category) {
					accepted = token;
					getNextToken ();
					return true;
				}
			}
			accepted = Token.errorToken();
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
			errorMsg = errorMsg +	", but found " + found.category;
			errors.addError(token, ErrorType.Syntax_Error, errorMsg); 
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
			ret.addStatement(statement ());
			expect (Category.Semicolon);

			//maximum munch
			while (token.category == Category.Keyword_Var || 
				token.category == Category.Identifier ||
				token.category == Category.Keyword_For ||
				token.category == Category.Function_Read ||
				token.category == Category.Function_Print ||
				token.category == Category.Function_Assert) {
				ret.addStatement(statement ());
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
				((Declaration)ret).identifier = accepted;
				expect (Category.Colon);
				expect (Category.Type_Boolean, 
				        Category.Type_Integer,
				        Category.Type_String);
				((Declaration)ret).type = accepted;
				if (accept (Category.Operator_Assignment)) {
					((Declaration)ret).expression = expression ();
				}
				//assignment
			} else if (accept (Category.Identifier)) {
				ret = new Assignment();
				((Assignment)ret).identifier = accepted;
				if (expect (Category.Operator_Assignment)) {
					((Assignment)ret).expression = expression ();
				} else {
					System.Console.WriteLine("INVALID ASSIGNMENT!!!");
					errors.addError(token, ErrorType.Syntax_Error, "Invalid assignment; expression expected");
				}
				//for loop
			} else if (accept (Category.Keyword_For)) {
				ret = new ForLoop();
				expect (Category.Identifier);
				((ForLoop)ret).variable = accepted;
				expect (Category.Keyword_In);
				((ForLoop)ret).from = expression ();
				expect (Category.Loop_Range);
				((ForLoop)ret).to = expression ();
				expect (Category.Keyword_Do);
				((ForLoop)ret).statements = statements ();
				expect (Category.Keyword_End);
				expect (Category.Keyword_For);
				//read
			} else if (accept (Category.Function_Read)) {
				ret = new Read();
				expect (Category.Identifier);
				((Read)ret).identifier = accepted;
				//print
			} else if (accept (Category.Function_Print)) {
				ret = new Print();
				((Print)ret).expression = expression ();
				//assert
			} else if (accept (Category.Function_Assert)) {
				ret = new Assert();
				((Assert)ret).location = accepted;
				expect (Category.Left_Bracket);
				((Assert)ret).assertion = expression ();
				expect (Category.Rigth_Bracket);
			} else {
				errors.addError(token, ErrorType.Syntax_Error, "Expecting a statement");
			}
			return ret;
		}

		private Expression expression ()
		{
			ExpressionBuilder builder = new ExpressionBuilder(errors);
			buildExpression(builder);
			return builder.build();
		}

		private void buildExpression(ExpressionBuilder builder) 
		{
			if (accept (Category.Operator_Not)) {
				builder.offer(accepted);
				operand (builder);
			} else {
				operand (builder);
				if (accept (Category.Operator_Addition,
				        Category.Operator_Division,
				        Category.Operator_Multiplication,
				        Category.Operator_Substraction,
				        Category.Operator_Equality,
				        Category.Operator_Less,
					    Category.Operator_And)) {
					builder.offer(accepted);
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
				builder.offer(accepted);

			//nested expression
			} else if (accept (Category.Left_Bracket)) {
				builder.offer(accepted);
				buildExpression (builder);
				expect (Category.Rigth_Bracket);
				builder.offer (accepted);
			} else {
				/*addError(token, Category.Literal_Integer, 
						 Category.Literal_String,
						 Category.Identifier,
				         Category.Left_Bracket);*/
				errors.addError(token, ErrorType.Syntax_Error, "Expecting an operand");
			}
		}


	}
}

