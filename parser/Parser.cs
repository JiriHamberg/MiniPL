using System;
using System.Collections.Generic;

namespace CompilersProject
{
	public class Parser
	{
		class SyntaxErrorException : Exception {}

		static HashSet<Category> Set(params Category[] categories) 
		{
			return new HashSet<Category>(categories);
		}

		static HashSet<Category> statementFirstSet;
		static HashSet<Category> declarationFirstSet;
		static HashSet<Category> assignmentFirstSet;
		static HashSet<Category> forLoopFirstSet;
		static HashSet<Category> readFirstSet;
		static HashSet<Category> printFirstSet;
		static HashSet<Category> assertFirstSet;
		static HashSet<Category> operandFirstSet;
		static HashSet<Category> expressionFirstSet;

		static Parser()
		{
			statementFirstSet = Set (
				Category.Keyword_Var, Category.Identifier, 
				Category.Keyword_For, Category.Function_Read,
				Category.Function_Print, Category.Function_Assert
			);

			declarationFirstSet = Set (Category.Keyword_Var);
			assignmentFirstSet = Set (Category.Identifier);
			forLoopFirstSet = Set (Category.Keyword_For);
			printFirstSet = Set (Category.Function_Print);
			readFirstSet = Set (Category.Function_Read);
			assertFirstSet = Set (Category.Function_Assert);

			operandFirstSet = Set (
				Category.Literal_Integer, 
				Category.Literal_String,
				Category.Identifier
			);
			expressionFirstSet = operandFirstSet;
			expressionFirstSet.Add(Category.Unary_Operator);
		}


		Scanner scanner;
		ErrorContainer errors;

		//current token
		Token token;
		//last token
		Token accepted;

		public Parser (Scanner scanner, ErrorContainer errors)
		{
			this.scanner = scanner;
			this.errors = errors;
		}


		public AbstractSyntaxTree Parse ()
		{
			AbstractSyntaxTree ast = new AbstractSyntaxTree();
			GetNextToken ();
			/*while (token.Category != Category.End_Of_File) { //scanner.HasNext()
				var follow = Set (Category.End_Of_File);
				var starters = Set ();
				try {
					ast.Statements.MergeStatements(ParseStatements (follow, starters ));
				} catch(SyntaxErrorException) {
					//there is no upper level parser method so this should not happen
					Console.WriteLine("File ended unexpectedly");
					//throw new InvalidProgramException("Parser failed to merge statements");

				}
			}*/
			ast.Statements = ParseStatements(Set (Category.End_Of_File), Set ());
			return ast;
		}


		void GetNextToken ()
		{
			//todo: handle end of stream !!!
			if (scanner.HasNext ()) {
				token = scanner.Next ();
			} else {
				//token = Token.ErrorToken();
			}
		}

		bool Accept (params Category[] categories)
		{
			foreach (var category in categories) {
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
			foreach (var category in categories) {
				if (Accept(category)) return true;
			}
			AddError (token, categories);
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
			throw new SyntaxErrorException();
		}

		ASTNode Recover (ISet<Category> first, ISet<Category> follow, ISet<Category> starters, Func<ASTNode> firstAction )
		{
			//GetNextToken(); 
			while(true) {
				if(first.Contains(token.Category)) {
					return firstAction();
				} else if(follow.Contains(token.Category)) {
					return null;
				} else if(starters.Contains(token.Category) || token.Category == Category.End_Of_File) {
					throw new SyntaxErrorException();
				} 
				GetNextToken();
			}
		}

	
		/*
		 *  Actual parsing and builging the ast
		 */

		Statements ParseStatements (ISet<Category> follow, ISet<Category> starters)
		{
			var ret = new Statements ();
			try {
				ret.AddStatement (ParseStatement (Set (Category.Semicolon), starters));
			} catch (SyntaxErrorException) {
				ret.AddStatement((Statement)Recover (statementFirstSet, follow, starters, () => ParseStatement (Set (Category.Semicolon), starters) ));
			}

			try {
				Expect (Category.Semicolon);
			} catch(SyntaxErrorException) {
				//missing semicolon, no big deal
			}

			//maximum munch
			while (!follow.Contains(token.Category)) { //statementFirstSet.Contains(token.Category)
				try {
					ret.AddStatement (ParseStatement (Set (Category.Semicolon), starters));
				} catch (SyntaxErrorException) {
					ret.AddStatement ((Statement)Recover (statementFirstSet, follow, starters, () => ParseStatement (Set (Category.Semicolon), starters)));
				}
				try {
					Expect (Category.Semicolon);
				} catch(SyntaxErrorException) {
					//missing semicolon, no big deal
				}
			}
			return ret;
		}

		Statement ParseStatement (ISet<Category> follow, ISet<Category> starters)
		{
			var stmtFollow = Set (Category.Semicolon);
			starters = Set (Category.Keyword_For, Category.Keyword_Do, Category.Keyword_End);
			//variable declaration
			if (Accept (Category.Keyword_Var)) {
				try {
					return ParseDeclaration(stmtFollow, starters);
				} catch(SyntaxErrorException) {
					return (Declaration) Recover (declarationFirstSet, follow, starters, () => ParseDeclaration(stmtFollow, starters));
				}
				//assignment
			} else if (Accept (Category.Identifier)) {
				try {
					return ParseAssignment(stmtFollow, starters);
				} catch(SyntaxErrorException) {
					return (Assignment)Recover(assignmentFirstSet, follow, starters, () => ParseAssignment(stmtFollow, starters));
				}
				//for loop
			} else if (Accept (Category.Keyword_For)) {
				try {
					return ParseForLoop(stmtFollow, starters);
				} catch(SyntaxErrorException) {
					return (ForLoop)Recover(forLoopFirstSet, follow, starters, () => ParseForLoop(stmtFollow, starters));
				}
				//read
			} else if (Accept (Category.Function_Read)) {
				try {
					return ParseRead (stmtFollow, starters);
				} catch(SyntaxErrorException) {
					return (Read)Recover (readFirstSet, follow, starters, () => ParseRead (stmtFollow, starters));
				}
				//print
			} else if (Accept (Category.Function_Print)) {
				try {
					return ParsePrint(stmtFollow, starters);
				} catch(SyntaxErrorException) {
					return (Print)Recover(printFirstSet, follow, starters, () => ParsePrint(stmtFollow, starters));
				}
				//assert
			} else if (Accept (Category.Function_Assert)) {
				try {
					return ParseAssert(stmtFollow, starters);
				} catch(SyntaxErrorException) {
					return (Assert)Recover(assertFirstSet, follow, starters, () => ParseAssert(stmtFollow, starters));
				}
			} else {
				errors.AddError(token, ErrorType.SyntaxError, "Expecting a statement");
				throw new SyntaxErrorException();
			}
			//return null;
		}

		Declaration ParseDeclaration (ISet<Category> follow, ISet<Category> starters)
		{
			var ret = new Declaration();
			Expect (Category.Identifier);
			ret.Identifier = accepted;
			Expect (Category.Colon);
			Expect (Category.Type);
			ret.Type = accepted;
			if (Accept (Category.Assignment)) {
				try {
					ret.Expression = ParseExpression (Set (Category.Semicolon), starters);
				} catch(SyntaxErrorException) {
					ret.Expression = (Expression)Recover (expressionFirstSet, follow, starters, () => ParseExpression (Set (Category.Semicolon), starters));
				}
			}
			return ret;
		}

		Assignment ParseAssignment (ISet<Category> follow, ISet<Category> starters)
		{
			var ret = new Assignment();
			ret.Identifier = accepted;
			if (Expect (Category.Assignment)) {
				try {
					ret.Expression = ParseExpression (Set (Category.Semicolon), starters);
				} catch (SyntaxErrorException) {
					ret.Expression = (Expression)Recover(expressionFirstSet, follow, starters, 
						() => ParseExpression (Set (Category.Semicolon), starters));
				}
			} else {
				//System.Console.WriteLine("INVALID ASSIGNMENT!!!");
				errors.AddError(token, ErrorType.SyntaxError, "Invalid assignment; expression expected");
				throw new SyntaxErrorException();
			}
			return ret;
		}

		ForLoop ParseForLoop (ISet<Category> follow, ISet<Category> starters)
		{
			var ret = new ForLoop ();
			Expect (Category.Identifier);
			ret.Variable = accepted;
			Expect (Category.Keyword_In);
			try {
				ret.From = ParseExpression (Set (Category.Loop_Range), starters);
			} catch (SyntaxErrorException) {
				ret.From = (Expression)Recover (forLoopFirstSet, follow, starters,
				                                () => ParseExpression (Set (Category.Loop_Range), starters)); 
			}
			Expect (Category.Loop_Range);
			try {
				ret.To = ParseExpression (Set (Category.Keyword_Do), starters);
			} catch (SyntaxErrorException) {
				ret.To = (Expression)Recover (forLoopFirstSet, follow, starters,
				                              () => ParseExpression (Set (Category.Keyword_Do), starters));
			}
			Expect (Category.Keyword_Do);
			try {
				ret.Statements = ParseStatements (Set (Category.Keyword_End), starters);
			} catch (SyntaxErrorException) {
				ret.Statements = (Statements)Recover (statementFirstSet, follow, starters, 
				                                      () => ParseStatements (Set (Category.Keyword_End), starters));
			}
			Expect (Category.Keyword_End);
			Expect (Category.Keyword_For);
			return ret;
		}

		Read ParseRead (ISet<Category> follow, ISet<Category> starters)
		{
			var ret = new Read();
			Expect (Category.Identifier);
			ret.Identifier = accepted;
			return ret;
		}

		Print ParsePrint (ISet<Category> follow, ISet<Category> starters)
		{
			var ret = new Print();
			try {
				ret.Expression = ParseExpression (Set (Category.Semicolon), starters);
			} catch(SyntaxErrorException) {
				ret.Expression = (Expression)Recover (printFirstSet, follow, starters,
				                                      () => ParseExpression (Set (Category.Semicolon), starters));
			}
			return ret;
		}

		Assert ParseAssert (ISet<Category> follow, ISet<Category> starters)
		{
			var ret = new Assert ();
			ret.Location = accepted;
			Expect (Category.Left_Bracket);
			try {			
				ret.Assertion = ParseExpression (Set (Category.Semicolon), starters);
			} catch (SyntaxErrorException) {
				ret.Assertion = (Expression)Recover (assertFirstSet, follow, starters, 
				                                     () => ParseExpression (Set (Category.Semicolon), starters)); 
			}
			Expect (Category.Rigth_Bracket);
			return ret;
		}


		Expression ParseExpression (ISet<Category> follow, ISet<Category> starters)
		{
			ExpressionBuilder builder = new ExpressionBuilder(errors);
			BuildExpression(builder);
			return builder.Build();
		}

		void BuildExpression(ExpressionBuilder builder) 
		{
			if (Accept (Category.Unary_Operator)) {
				builder.Offer(accepted);
				BuildOperand (builder);
			} else {
				BuildOperand (builder);
				if (Accept (Category.Binary_Operator)) {
					builder.Offer(accepted);
					BuildOperand (builder);
				}
			}
		}

		void BuildOperand (ExpressionBuilder builder)
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

