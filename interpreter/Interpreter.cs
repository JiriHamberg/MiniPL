using System;
using System.IO;

namespace CompilersProject
{
	public class Interpreter
	{
		private SymbolTable symbolTable = new SymbolTable();
		private ErrorContainer errors = new ErrorContainer();

		public Interpreter ()
		{
		}

		public void Interprete(AbstractSyntaxTree ast)
		{
			Interprete(ast.Statements);
		}

		void Interprete (Statements stmts)
		{
			foreach (var stmt in stmts.StatementList) {
				Interprete(stmt);
			}
		}

		void Interprete(Statement stmt)
		{
			if (stmt is Declaration) {
				var declaration = (Declaration)stmt;
				if(declaration.Expression == null) {
					//use default value
					TypeBinding binding = TypeBindings.GetTypeByName(declaration.Type.Lexeme);
					object defaultValue = TypeModels.GetDefaultValue(binding);
					symbolTable.Declare(declaration.Identifier, declaration.Type, defaultValue);
				} else {
					symbolTable.Declare(declaration.Identifier, declaration.Type, Evaluate(declaration.Expression));
				}				
			} else if (stmt is Assignment) {
				var assignment = ((Assignment)stmt);
				symbolTable.Assign(assignment.Identifier, Evaluate(assignment.Expression));
			} else if (stmt is ForLoop) {
				var loop = (ForLoop)stmt;
				int low = (int)Evaluate (loop.From);
				int high = (int)Evaluate(loop.To);				
				for(int i = low; i<=high; i++) {
					symbolTable.Assign(loop.Variable, i);
					Interprete(loop.Statements);
				}
			} else if (stmt is Print) {
				var print = (Print)stmt;
				object evaluation = Evaluate(print.Expression);
				Console.Write (evaluation.ToString());
			} else if (stmt is Assert) {
				var assert = (Assert)stmt;
				if(!(bool)Evaluate(assert.Assertion)) {
					AssertionMessage(assert.Location.Line, assert.Location.Column, assert.Assertion);
				}
			} else if (stmt is Read) {
				var read = (Read)stmt;
				//todo: convert to correct type
				string type = symbolTable.GetVariableType(read.Identifier);
				object inputValue = ReadNextWord();
				if(type == TypeBindings.PRIMITIVE_INTEGER_NAME) {
					inputValue = int.Parse((string)inputValue);
				} else if(type == TypeBindings.PRIMITIVE_BOOLEAN_NAME) {
					inputValue = bool.Parse((string)inputValue);
				}			
				symbolTable.Assign(read.Identifier, inputValue);
			}
		}

		object Evaluate(Expression expression) 
		{
			if (expression is BinaryOperator) {
				var binOp = (BinaryOperator)expression;
				TypeBinding binding = TypeBindings.DecideType(binOp.LeftOperand, symbolTable, errors);

				return TypeModels.EvaluateBinaryOperator(binding, binOp.Oper.Lexeme,
				                                  Evaluate (binOp.LeftOperand), Evaluate(binOp.RightOperand));
			} else if (expression is UnaryOperator) {
				var unOp = (UnaryOperator)expression;
				TypeBinding binding = TypeBindings.DecideType(unOp.Operand, symbolTable, errors);

				return TypeModels.EvaluateUnaryOperator(binding, unOp.Oper.Lexeme,
				                                        Evaluate (unOp.Operand));
			} else if (expression is ExpressionLeaf) {
				var leaf = (ExpressionLeaf)expression;
				switch(leaf.Token.Category) 
				{
					case Category.Literal_Integer:
						return int.Parse(leaf.Token.Lexeme);
						break;
					case Category.Literal_String:
						return leaf.Token.Lexeme;
						break;
					case Category.Identifier:
						if (symbolTable.IsDeclared (leaf.Token)) {
							return symbolTable.GetValue(leaf.Token);
						} else {
							//should be handled by semantic analyser
							throw new InvalidOperationException("Undeclared variable");
						}
						break;
					default:
						return null;
				}
			}
			Console.WriteLine(expression.GetType());
			Console.WriteLine("Category: " + expression.Head().Category);
			Console.WriteLine ("Line " + expression.Head().Line);
			throw new InvalidOperationException("Unrecognized expression class");
		}


		string ReadNextWord ()
		{
			string s = "";
			while (true) {
				int c = Console.Read();
				if(c >= 0 && !Char.IsWhiteSpace((char)c)) {
					s += (char)c;
				} else {
					return s;
				}
			}
			//throw new EndOfStreamException("The input closed unexpectedly");
		}

		void AssertionMessage (int line, int column, Expression e)
		{
			string s = "Assertion near line " + line;
			s = s + " column " + column + " failed. Assertion \"" + e.ToString() + "\" was false.";
			Console.WriteLine(s);
		}
	}
}

