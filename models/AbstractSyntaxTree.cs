using System;
using System.Collections.Generic;

namespace CompilersProject
{
	public class AbstractSyntaxTree
	{
		public  Statements  statements = new Statements();
		public AbstractSyntaxTree ()
		{	 
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
	}

	public abstract class Statement : ASTNode 
	{
	}

	public class Declaration : Statement 
	{
		public Token identifier;
		public Token type;
		public Expression expression;
	}

	public class Assignment : Statement
	{
		public Token identifier;
		public Expression expression;
	}

	public class ForLoop : Statement
	{
		public Token variable;
		public Expression from;
		public Expression to;
		public List<Statement> statements;
	}

	public class Print : Statement
	{
		public Expression expression;
	}

	public class Read : Statement
	{
		public Token identifier;
	}

	public class assert : Statement
	{
		public Expression assertion;
	}

	public abstract class Expression : ASTNode 
	{
		public Expression head;
	}

	public class BinaryOperator : Expression 
	{
		public Token oper;
		public Expression leftOperand;
		public Expression rigtOperand;
	}

	public class UnaryOperator : Expression 
	{
		public Token oper;
		public Expression operand;
	}

	//identifier or literal
	public class ExpressionLeaf : Expression 
	{
		public Token literal;
	}



}

