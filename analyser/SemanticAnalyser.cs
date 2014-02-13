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

		//performs a pass on the ast checking typebindings and declarations of identifiers
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
						doTypeChecking (declaration.expression, declaration.type.category);
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
					doTypeChecking (loop.from, Category.Type_Integer);
					doTypeChecking (loop.to, Category.Type_Integer);
					doTypeChecking (loop.statements);
					symbolTable.Unlock();
				});
			} else if (stmt is Print) {
				doConsistencyChecking(((Print)stmt).expression);			
			} else if (stmt is Assert) {
				Assert assert = (Assert)stmt;
				doTypeChecking(assert.assertion, Category.Type_Boolean);
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


		private void doTypeChecking (Expression expression, Category type)
		{
			/*if (TypeSystem.GetCategoryFromType (decideType (expression)) != type) {
				errors.addError(expression.head(), ErrorType.Semantic_Error, "Expression does not match the required type");
			}*/

			if (TypeBindings.GetCategoryFromType (TypeBindings.DecideType(expression, symbolTable, errors)) != type) {
				errors.addError(expression.head(), ErrorType.Semantic_Error, "Expression does not match the required type");
			}
		}

		/*private TypeModel decideType (Expression expression)
		{
			if (expression is BinaryOperator) {
				var binOp = (BinaryOperator)expression;
				TypeModel leftType = decideType (binOp.leftOperand);
				TypeModel rightType = decideType (binOp.rigtOperand);
				if(leftType == null || rightType == null) {
					return null;
				}
				if (leftType != rightType) {
					errors.addError (binOp.oper, ErrorType.Semantic_Error, "Types of left and right operand do not match");
					return null;
				}
				var ret = rightType.Operate (binOp.oper.category);
				if(ret == null) {
					errors.addError (binOp.oper, ErrorType.Semantic_Error, "Could not apply operator to given types");
				}
				return ret; 
			} else if (expression is UnaryOperator) {
				var unOp = (UnaryOperator)expression;
				TypeModel operandType = decideType (unOp.operand);
				if (operandType == null) {
					return null;
				}
				var ret = operandType.Operate (unOp.oper.category);
				if(ret == null) {
					errors.addError(unOp.oper, ErrorType.Semantic_Error, "Could not apply operator to given type");
				}
				return ret;
			} else if (expression is ExpressionLeaf) {
				var leaf = (ExpressionLeaf)expression;
				switch(leaf.token.category) {
				case Category.Literal_Integer:
					return TypeSystem.integer;
				case Category.Literal_Boolean:
					return TypeSystem.boolean;
				case Category.Literal_String:
					return TypeSystem.str;
				case Category.Identifier:
					if (symbolTable.isDeclared (leaf.token)) {
						return TypeSystem.GetTypeFromCategory(symbolTable.GetVariableType(leaf.token));
					} else {
						errors.addError (leaf.token, ErrorType.Semantic_Error, "Undeclared variable");
						return null;
					}
				}
				return null;
			}

			return null;
		}*/


		//checks that given expression is consistent for exmaple bool + integer is inconsistent
		private void doConsistencyChecking (Expression expression) {
			//decideType (expression);
			TypeBindings.DecideType(expression, symbolTable, errors);
		}

	}
}

