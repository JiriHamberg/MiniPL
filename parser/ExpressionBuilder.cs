using System;
using System.Collections.Generic;

namespace CompilersProject
{

	/*
	 * Implements the classic shunting yard algorithm
	 * (with Polish notation output [not reverse Polish notation!])
	 * and builds an expression subtree for the abstract syntax tree
	 */

	public class ExpressionBuilder
	{

		private static Dictionary<string, int> precedence = new Dictionary<string, int>() {
			{Operators.NOT, 1},
			{Operators.EQUALITY, 2},
			{Operators.LESS, 2},
			{Operators.AND, 3},
			{Operators.ADDITION, 4},
			{Operators.SUBSTRACTION, 4},
			{Operators.MULTIPLICATION, 5},
			{Operators.DIVISION, 5}
		};

		private Stack<Token> operStack = new Stack<Token>();
		private Stack<Token> inputStack = new Stack<Token>();
		private Stack<Token> outputStack = new Stack<Token>();

		private ErrorContainer errors;

		public ExpressionBuilder (ErrorContainer errors)
		{
			this.errors = errors;
		}

		//give tokens to the builder
		public void Offer(Token next)
		{
			inputStack.Push (next);
		}

		public Expression Build ()
		{
			shuntingYard();
			return buildExpression ();
		}

		//builds the expression subtree recursively
		private Expression buildExpression ()
		{
			Expression ret = null;

			if (outputStack.Count < 1) {
				//ExpressionLeaf e = new ExpressionLeaf();
				//e.token = Token.errorToken();
				//return e;
				return ret;
			}

			Token t = outputStack.Pop ();

			if (t.category == Category.Binary_Operator) {
				ret = new BinaryOperator ();
				((BinaryOperator)ret).oper = t;
				((BinaryOperator)ret).leftOperand = buildExpression ();
				((BinaryOperator)ret).rigtOperand = buildExpression ();
			} else if (t.category == Category.Unary_Operator) {
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
						precedence.TryGetValue (top.lexeme, out precTop);
						precedence.TryGetValue (t.lexeme, out precCur);
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

