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

		static Dictionary<string, int> precedence = new Dictionary<string, int>() {
			{Operators.NOT, 1},
			{Operators.EQUALITY, 2},
			{Operators.LESS, 2},
			{Operators.AND, 3},
			{Operators.ADDITION, 4},
			{Operators.SUBSTRACTION, 4},
			{Operators.MULTIPLICATION, 5},
			{Operators.DIVISION, 5}
		};

		Stack<Token> operStack = new Stack<Token>();
		Stack<Token> inputStack = new Stack<Token>();
		Stack<Token> outputStack = new Stack<Token>();

		ErrorContainer errors;

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
			ShuntingYard();
			return BuildExpression ();
		}

		//builds the expression subtree recursively
		Expression BuildExpression ()
		{
			Expression ret = null;

			if (outputStack.Count < 1) {
				return ret;
			}

			Token t = outputStack.Pop ();

			if (t.Category == Category.Binary_Operator) {
				ret = new BinaryOperator ();
				((BinaryOperator)ret).Oper = t;
				((BinaryOperator)ret).LeftOperand = BuildExpression ();
				((BinaryOperator)ret).RightOperand = BuildExpression ();
			} else if (t.Category == Category.Unary_Operator) {
				ret = new UnaryOperator ();
				((UnaryOperator)ret).Oper = t;
				((UnaryOperator)ret).Operand = BuildExpression ();
			} else {
				ret = new ExpressionLeaf();
				((ExpressionLeaf)ret).Token = t;
			}
			return ret;
		}


		void ShuntingYard ()
		{
			while (inputStack.Count > 0) {
				Token t = inputStack.Pop ();

				switch (t.Category) {
				//operand
				case Category.Identifier:
				case Category.Literal_Integer:
				case Category.Literal_String:
					outputStack.Push (t);
					break;
				
				//right bracket
				case Category.Rigth_Bracket:
					operStack.Push (t);
					break;
				
				//left bracket
				case Category.Left_Bracket:
					while (operStack.Count > 0) {
						if (operStack.Peek ().Category == Category.Rigth_Bracket) {
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
						precedence.TryGetValue (top.Lexeme, out precTop);
						precedence.TryGetValue (t.Lexeme, out precCur);
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
				if(t.Category != Category.Rigth_Bracket) {
					outputStack.Push(t);
				} else {
					//the input contains errors. parser should have handled this!
					throw new ArgumentException("Parethesis mismatch");
				}
			}

		}


	}
}

