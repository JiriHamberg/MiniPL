using System;
using System.Collections.Generic;

namespace CompilersProject
{

	/*
	 * Defines the abstract syntax tree structure
	 */

	public class AbstractSyntaxTree
	{
		public  Statements  statements = new Statements();
		public AbstractSyntaxTree ()
		{	 
		}

		public override string ToString ()
		{
			return statements.ToString();
		}
	}


	public abstract class ASTNode
	{
	}

	public class Statements : ASTNode 
	{
		public List<Statement> statements = new List<Statement>();

		public void addStatement (Statement statement)
		{
			this.statements.Add(statement);
		}

		//we might need multiple statements builders to get all statements
		//if there are errors, hence this method
		public void mergeStatements (Statements stmts)
		{
			foreach (Statement s in stmts.statements) {
				this.statements.Add(s);
			}
		}

		public override string ToString ()
		{
			string s = "";

			foreach (Statement stmt in statements) {
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
		public Token identifier;
		public Token type;
		public Expression expression;


		public override string ToString ()
		{
			return "declare: " +  identifier + " = " + expression;
		}
	}

	public class Assignment : Statement
	{
		public Token identifier;
		public Expression expression;

		public override string ToString ()
		{
			return identifier + " = " + expression;
		}
	}

	public class ForLoop : Statement
	{
		public Token variable;
		public Expression from;
		public Expression to;
		public Statements statements;

		public override string ToString ()
		{
			string s = "for: " + variable + " from " + from + " to " + to + " \n" + statements + " end for";
			return s;
		}
	}

	public class Print : Statement
	{
		public Expression expression;

		public override string ToString ()
		{
			return "print: " + expression;
		}
	}

	public class Read : Statement
	{
		public Token identifier;

		public override string ToString ()
		{
			return identifier.ToString();
		}
	}

	public class Assert : Statement
	{
		public Expression assertion;
	}

	public abstract class Expression : ASTNode 
	{
		//public Expression head;
	}

	public class BinaryOperator : Expression 
	{
		public Token oper;
		public Expression leftOperand;
		public Expression rigtOperand;
	
		public override string ToString ()
		{
			return oper.ToString() + " " + leftOperand.ToString() + " " + rigtOperand.ToString();
		}
	}

	public class UnaryOperator : Expression 
	{
		public Token oper;
		public Expression operand;

		public override string ToString ()
		{
			return oper.ToString() + operand.ToString();
		}
	}

	//identifier or literal
	public class ExpressionLeaf : Expression 
	{
		public Token token;

		public override string ToString ()
		{
			return token.ToString();
		}
	}

}

