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
		public void doTypeChecking ()
		{
			doTypeChecking(ast.statements);
		}

		private void doTypeChecking (Statements statements)
		{
			foreach (Statement stmt in statements.statements) {
				doTypeChecking (stmt);
			}
		}

		private void doTypeChecking (Statement stmt)
		{
			if (stmt is Declaration) {
				var declaration = (Declaration)stmt;
				if (symbolTable.isDeclared (declaration.identifier)) {
					errors.addError (declaration.identifier, ErrorType.Semantic_Error, "Identifier " + declaration.identifier.lexeme + " already declared");
				} else {
					symbolTable.Declare (declaration.identifier, declaration.type );
					if(declaration.expression != null) {
						doTypeChecking (declaration.expression, declaration.type.lexeme);
					}
				}
			} else if (stmt is Assignment) {
				var assignment = ((Assignment)stmt);
				validateAssignmentAndDoAction(assignment.identifier, 
				    () => doTypeChecking (assignment.expression, symbolTable.GetVariableType (assignment.identifier)));
			} else if (stmt is ForLoop) {
				ForLoop loop = (ForLoop)stmt;
				validateAssignmentAndDoAction(loop.variable, () => {
					//note: as it stands now, invalid loop variable declaration halts type checking
					//      for the entire loop body
					//      might be good idea to refactor this later and remove such behaviour
					symbolTable.Lock(loop.variable);
					doTypeChecking (loop.from, TypeBindings.PRIMITIVE_INTEGER_NAME);
					doTypeChecking (loop.to, TypeBindings.PRIMITIVE_INTEGER_NAME);
					doTypeChecking (loop.statements);
					symbolTable.Unlock();
				});
			} else if (stmt is Print) {
				doConsistencyChecking(((Print)stmt).expression);			
			} else if (stmt is Assert) {
				Assert assert = (Assert)stmt;
				doTypeChecking(assert.assertion, TypeBindings.PRIMITIVE_BOOLEAN_NAME);
			} else if (stmt is Read) {
				//just check if variable is not declared or locked with no-action
				validateAssignmentAndDoAction(((Read)stmt).identifier, () => {} );
			}
		}

		void validateAssignmentAndDoAction (Token identifier, Action action) {
			if (symbolTable.isDeclared (identifier)) {
				if (symbolTable.isLocked (identifier)) {
					errors.addError (identifier, ErrorType.Semantic_Error, "Variable is being used by a for loop and cannot be assigned to");
				} else {
					//do action if assignment to identifier is valid
					action();
				}
			} else {
				errors.addError (identifier, ErrorType.Semantic_Error, "Cannot assign to a variable that is undeclared");
			}
		}


		private void doTypeChecking (Expression expression, string type)
		{
			/*if (TypeSystem.GetCategoryFromType (decideType (expression)) != type) {
				errors.addError(expression.head(), ErrorType.Semantic_Error, "Expression does not match the required type");
			}*/

			if (TypeBindings.DecideType(expression, symbolTable, errors).name != type) {
				errors.addError(expression.head(), ErrorType.Semantic_Error, "Expression does not match the required type");
			}
		}

		//checks that given expression is consistent, namely that types of each left and right operand match
		private void doConsistencyChecking (Expression expression) {
			TypeBindings.DecideType(expression, symbolTable, errors);
		}

	}
}

