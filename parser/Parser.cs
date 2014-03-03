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
			GetNextToken ();
			while (scanner.HasNext()) {
				ast.Statements.MergeStatements(Statements ());
			}
			return ast;
		}


		void GetNextToken ()
		{
			//todo: handle end of stream !!!
			if (scanner.HasNext ()) {
				token = scanner.Next ();
			} else {
				token = Token.ErrorToken();
			}
		}

		bool Accept (params Category[] categories)
		{
			foreach (Category category in categories) {
				if(category == token.Category) {
					accepted = token;
					GetNextToken ();
					return true;
				}
			}
			accepted = Token.ErrorToken();
			return false;
		}

		bool Expect (params Category [] categories)
		{
			foreach (Category category in categories) {
				if (Accept(category)) return true;
			}
			AddError (token, categories);
			Recover ();
			return false;
		}

		void AddError (Token found, params Category[] expected)
		{
			string errorMsg = "Expecting " + expected [0];
			for (int i=1; i <expected.Length; i++) {
				errorMsg = errorMsg + " or " + expected[i];
			}
			errorMsg = errorMsg +	", but found " + found.Category;
			errors.AddError(token, ErrorType.SyntaxError, errorMsg); 
		}

		//naive recovery strategy: continue parsing from the next token
		//todo: implement properly
		void Recover ()
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
			GetNextToken ();
		}


		/*
		 *  Actual parsing and builging ast
		 */

		Statements Statements ()
		{
			Statements ret = new Statements();
			ret.AddStatement(Statement ());
			Expect (Category.Semicolon);

			//maximum munch
			while (token.Category == Category.Keyword_Var || 
				token.Category == Category.Identifier ||
				token.Category == Category.Keyword_For ||
				token.Category == Category.Function_Read ||
				token.Category == Category.Function_Print ||
				token.Category == Category.Function_Assert) {
				ret.AddStatement(Statement ());
				Expect (Category.Semicolon);
			}
			return ret;
		}

		Statement Statement ()
		{
			Statement ret = null;
			//variable declaration
			if (Accept (Category.Keyword_Var)) {
				ret = new Declaration();
				Expect (Category.Identifier);
				((Declaration)ret).Identifier = accepted;
				Expect (Category.Colon);
				Expect (Category.Type);
				((Declaration)ret).Type = accepted;
				if (Accept (Category.Assignment)) {
					((Declaration)ret).Expression = Expression ();
				}
				//assignment
			} else if (Accept (Category.Identifier)) {
				ret = new Assignment();
				((Assignment)ret).Identifier = accepted;
				if (Expect (Category.Assignment)) {
					((Assignment)ret).Expression = Expression ();
				} else {
					//System.Console.WriteLine("INVALID ASSIGNMENT!!!");
					errors.AddError(token, ErrorType.SyntaxError, "Invalid assignment; expression expected");
				}
				//for loop
			} else if (Accept (Category.Keyword_For)) {
				ret = new ForLoop();
				Expect (Category.Identifier);
				((ForLoop)ret).Variable = accepted;
				Expect (Category.Keyword_In);
				((ForLoop)ret).From = Expression ();
				Expect (Category.Loop_Range);
				((ForLoop)ret).To = Expression ();
				Expect (Category.Keyword_Do);
				((ForLoop)ret).Statements = Statements ();
				Expect (Category.Keyword_End);
				Expect (Category.Keyword_For);
				//read
			} else if (Accept (Category.Function_Read)) {
				ret = new Read();
				Expect (Category.Identifier);
				((Read)ret).Identifier = accepted;
				//print
			} else if (Accept (Category.Function_Print)) {
				ret = new Print();
				((Print)ret).Expression = Expression ();
				//assert
			} else if (Accept (Category.Function_Assert)) {
				ret = new Assert();
				((Assert)ret).Location = accepted;
				Expect (Category.Left_Bracket);
				((Assert)ret).Assertion = Expression ();
				Expect (Category.Rigth_Bracket);
			} else {
				errors.AddError(token, ErrorType.SyntaxError, "Expecting a statement");
			}
			return ret;
		}

		Expression Expression ()
		{
			ExpressionBuilder builder = new ExpressionBuilder(errors);
			BuildExpression(builder);
			return builder.Build();
		}

		void BuildExpression(ExpressionBuilder builder) 
		{
			if (Accept (Category.Unary_Operator)) {
				builder.Offer(accepted);
				Operand (builder);
			} else {
				Operand (builder);
				if (Accept (Category.Binary_Operator)) {
					builder.Offer(accepted);
					Operand (builder);
				}
			}
		}

		void Operand (ExpressionBuilder builder)
		{
			//literal or identifier
			if (Accept (Category.Literal_Integer, 
						Category.Literal_String,
						Category.Identifier)) {
				builder.Offer(accepted);

			//nested expression
			} else if (Accept (Category.Left_Bracket)) {
				builder.Offer(accepted);
				BuildExpression (builder);
				Expect (Category.Rigth_Bracket);
				builder.Offer (accepted);
			} else {
				/*addError(token, Category.Literal_Integer, 
						 Category.Literal_String,
						 Category.Identifier,
				         Category.Left_Bracket);*/
				errors.AddError(token, ErrorType.SyntaxError, "Expecting an operand");
			}
		}


	}
}

