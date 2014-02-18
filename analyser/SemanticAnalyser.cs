using System;
using System.Collections.Generic;

namespace CompilersProject
{
	public class SemanticAnalyser
	{
		private AbstractSyntaxTree ast;
		private ErrorContainer errors;
		private SymbolTable symbolTable = new SymbolTable();

		public SemanticAnalyser (AbstractSyntaxTree ast, ErrorContainer errors)
		{
			this.ast = ast;
			this.errors = errors;
		}

		/*
		 * performs a pass on the ast checking:
		 * 		- declarations before assignments
		 * 		- no violations to loop variable locking
		 * 		- types produced by expression match declarations
		 */
		public void DoTypeChecking ()
		{
			DoTypeChecking(ast.Statements);
		}

		private void DoTypeChecking (Statements statements)
		{
			foreach (Statement stmt in statements.StatementList) {
				DoTypeChecking (stmt);
			}
		}

		private void DoTypeChecking (Statement stmt)
		{
			if (stmt is Declaration) {
				var declaration = (Declaration)stmt;
				if (symbolTable.IsDeclared (declaration.Identifier)) {
					errors.AddError (declaration.Identifier, ErrorType.Semantic_Error, "Identifier " + declaration.Identifier.Lexeme + " already declared");
				} else {
					symbolTable.Declare (declaration.Identifier, declaration.Type );
					if(declaration.Expression != null) {
						DoTypeChecking (declaration.Expression, declaration.Type.Lexeme);
					}
				}
			} else if (stmt is Assignment) {
				var assignment = ((Assignment)stmt);
				validateAssignmentAndDoAction(assignment.Identifier, 
				    () => DoTypeChecking (assignment.Expression, symbolTable.GetVariableType (assignment.Identifier)));
			} else if (stmt is ForLoop) {
				ForLoop loop = (ForLoop)stmt;
				validateAssignmentAndDoAction(loop.Variable, () => {
					//note: as it stands now, invalid loop variable declaration halts type checking
					//      for the entire loop body
					//      might be good idea to refactor this later and remove such behaviour
					symbolTable.Lock(loop.Variable);
					DoTypeChecking (loop.From, TypeBindings.PRIMITIVE_INTEGER_NAME);
					DoTypeChecking (loop.To, TypeBindings.PRIMITIVE_INTEGER_NAME);
					DoTypeChecking (loop.Statements);
					symbolTable.Unlock();
				});
			} else if (stmt is Print) {
				doConsistencyChecking(((Print)stmt).Expression);			
			} else if (stmt is Assert) {
				Assert assert = (Assert)stmt;
				DoTypeChecking(assert.Assertion, TypeBindings.PRIMITIVE_BOOLEAN_NAME);
			} else if (stmt is Read) {
				//just check if variable is not declared or locked with no-action
				validateAssignmentAndDoAction(((Read)stmt).Identifier, () => {} );
			}
		}

		private void validateAssignmentAndDoAction (Token identifier, Action action) {
			if (symbolTable.IsDeclared (identifier)) {
				if (symbolTable.IsLocked (identifier)) {
					errors.AddError (identifier, ErrorType.Semantic_Error, "Variable is being used by a for loop and cannot be assigned to");
				} else {
					//do action if assignment to identifier is valid
					action();
				}
			} else {
				errors.AddError (identifier, ErrorType.Semantic_Error, "Cannot assign to a variable that is undeclared");
			}
		}


		private void DoTypeChecking (Expression expression, string type)
		{
			/*if (TypeSystem.GetCategoryFromType (decideType (expression)) != type) {
				errors.addError(expression.head(), ErrorType.Semantic_Error, "Expression does not match the required type");
			}*/
			var binding = TypeBindings.DecideType (expression, symbolTable, errors);
			if (binding == null) {
				//expression was inconsistent
				return;
			} else if (binding.Name != type) {
				errors.AddError(expression.Head(), ErrorType.Semantic_Error, "Expression does not match the required type");
			}
		}

		//checks that given expression is consistent, namely that types of each left and right operand match
		private void doConsistencyChecking (Expression expression) {
			TypeBindings.DecideType(expression, symbolTable, errors);
		}

	}
}

