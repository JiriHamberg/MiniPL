using System;
using System.Collections.Generic;

namespace CompilersProject
{

	/*
	 * Implements the classic shunting yard algorithm
	 * (with polish notation output [not reverse polish notation!])
	 * and builds an expression subtree of the abstract syntax tree
	 */

	public class ExpressionBuilder
	{

		private static Dictionary<Category, int> precedence = new Dictionary<Category, int>(){
			{Category.Operator_Not, 1},
			{Category.Operator_Equality, 2},
			{Category.Operator_Less, 2},
			{Category.Operator_And, 3},
			{Category.Operator_Addition, 4},
			{Category.Operator_Substraction, 4},
			{Category.Operator_Multiplication, 5},
			{Category.Operator_Division, 5}
		};

		private Stack<Token> operStack = new Stack<Token>();
		private Stack<Token> inputStack = new Stack<Token>();
		private Stack<Token> outputStack = new Stack<Token>();

		private ErrorContainer errors;

		public ExpressionBuilder (ErrorContainer errors)
		{
			this.errors = errors;
		}

		public void offer(Token next)
		{
			inputStack.Push (next);
		}


		public Expression build ()
		{
			shuntingYard();
			return buildExpression ();
		}

		//builds the expression subtree recursively
		private Expression buildExpression ()
		{
			Expression ret = null;

			if (outputStack.Count < 1) {
				ExpressionLeaf e = new ExpressionLeaf();
				e.token = Token.errorToken();
				return e;
			}

			Token t = outputStack.Pop ();


			if (t.isBinaryOperator()) {
				ret = new BinaryOperator ();
				((BinaryOperator)ret).oper = t;
				((BinaryOperator)ret).leftOperand = buildExpression ();
				((BinaryOperator)ret).rigtOperand = buildExpression ();
			} else if (t.isUnaryOperator()) {
				ret = new UnaryOperator ();
				((UnaryOperator)ret).oper = t;
				((UnaryOperator)ret).operand = buildExpression ();
			} else {
				ret = new ExpressionLeaf();
				((ExpressionLeaf)ret).token = t;
			}
			return ret;
		}


		private void shuntingYard ()
		{
			while (inputStack.Count > 0) {
				Token t = inputStack.Pop ();

				switch (t.category) {
				//operand
				case Category.Identifier:
				case Category.Literal_Integer:
				case Category.Literal_String:
				case Category.Literal_Boolean:
					outputStack.Push (t);
					break;
				
				//right bracket
				case Category.Rigth_Bracket:
					operStack.Push (t);
					break;
				
				//left bracket
				case Category.Left_Bracket:
					while (operStack.Count > 0) {
						if (operStack.Peek ().category == Category.Rigth_Bracket) {
							operStack.Pop ();
							break; //end while
						} else {
							outputStack.Push (operStack.Pop ());
						}
					}
					break; //end switch

				//operator
				//all our operators are left assosiative
				default:
					while (operStack.Count > 0) {
						Token top = operStack.Peek ();
						int precTop = -1;
						int precCur = -1;
						precedence.TryGetValue (top.category, out precTop);
						precedence.TryGetValue (t.category, out precCur);
						if (precTop >= precCur) {
							outputStack.Push (operStack.Pop ());
						} else {
							break; //end while
						}
					}
					operStack.Push (t);
					break; //end switch
				}
			}

			while (operStack.Count > 0) {
				Token t = operStack.Pop();
				if(t.category != Category.Rigth_Bracket) {
					outputStack.Push(t);
				} else {
					//the input contains errors. parser should have handled this!
					throw new ArgumentException("Parethesis mismatch");
				}
			}

		}


	}
}

