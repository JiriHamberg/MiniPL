using System.Collections.Generic;

namespace CompilersProject
{

	/*
	 * Defines the abstract syntax tree structure
	 */

	public class AbstractSyntaxTree
	{
		public  Statements  Statements = new Statements();
		public AbstractSyntaxTree ()
		{	 
		}

		public override string ToString ()
		{
			return Statements.ToString();
		}
	}


	public abstract class ASTNode
	{
	}

	public class Statements : ASTNode 
	{
		public List<Statement> StatementList = new List<Statement>();

		public void AddStatement (Statement statement)
		{
			this.StatementList.Add(statement);
		}

		//we might need multiple statements builders to get all statements
		//if there are errors, hence this method
		public void MergeStatements (Statements stmts)
		{
			foreach (Statement s in stmts.StatementList) {
				this.StatementList.Add(s);
			}
		}

		public override string ToString ()
		{
			string s = "";

			foreach (Statement stmt in StatementList) {
				s = s + "\n" + stmt;
			}
			return s;
		}
	}

	public abstract class Statement : ASTNode 
	{
	}

	public class Declaration : Statement 
	{
		public Token Identifier;
		public Token Type;
		public Expression Expression;


		public override string ToString ()
		{
			string s = "declare: " + Identifier;
			if (Expression != null) {
				s = s + " = " + Expression;
			}
			return s;
		}
	}

	public class Assignment : Statement
	{
		public Token Identifier;
		public Expression Expression;

		public override string ToString ()
		{
			return Identifier + " = " + Expression;
		}
	}

	public class ForLoop : Statement
	{
		public Token Variable;
		public Expression From;
		public Expression To;
		public Statements Statements;

		public override string ToString ()
		{
			string s = "for: " + Variable + " from " + From + " to " + To + " \n" + Statements + " end for";
			return s;
		}
	}

	public class Print : Statement
	{
		public Expression Expression;

		public override string ToString ()
		{
			return "print: " + Expression;
		}
	}

	public class Read : Statement
	{
		public Token Identifier;

		public override string ToString ()
		{
			return Identifier.ToString();
		}
	}

	public class Assert : Statement
	{
		public Token Location;
		public Expression Assertion;
	}

	public abstract class Expression : ASTNode 
	{
		//public Expression head;
		public abstract Token Head();
	}

	public class BinaryOperator : Expression 
	{
		public Token Oper;
		public Expression LeftOperand;
		public Expression RightOperand;

		public override Token Head ()
		{
			return Oper;
		}

		public override string ToString ()
		{
			string s = "";
			if (LeftOperand is ExpressionLeaf) {
				s += LeftOperand.ToString ();
			} else {
				s += "(" + LeftOperand.ToString() + ")";
			}

			s += " " + Oper.ToString() + " ";

			if (RightOperand is ExpressionLeaf) {
				s += RightOperand.ToString ();
			} else {
				s += "(" + RightOperand.ToString() + ")";
			}

			return s;
		}
	}

	public class UnaryOperator : Expression 
	{
		public Token Oper;
		public Expression Operand;

		public override Token Head ()
		{
			return Oper;
		}

		public override string ToString ()
		{
			return Oper.ToString() + Operand.ToString();
		}
	}

	//identifier or literal
	public class ExpressionLeaf : Expression 
	{
		public Token Token;

		public override Token Head ()
		{
			return Token;
		}

		public override string ToString ()
		{
			return Token.ToString();
		}
	}

}

