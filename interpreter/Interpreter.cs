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
			Interprete(ast.statements);
		}

		private void Interprete (Statements stmts)
		{
			foreach (var stmt in stmts.statements) {
				Interprete(stmt);
			}
		}

		private void Interprete(Statement stmt)
		{
			if (stmt is Declaration) {
				var declaration = (Declaration)stmt;
				if(declaration.expression == null) {
					//use default value
					TypeBinding binding = TypeBindings.GetTypeByName(declaration.type.lexeme);
					object defaultValue = TypeModels.GetDefaultValue(binding);
					symbolTable.Declare(declaration.identifier, declaration.type, defaultValue);
				} else {
					symbolTable.Declare(declaration.identifier, declaration.type, Evaluate(declaration.expression));
				}				
			} else if (stmt is Assignment) {
				var assignment = ((Assignment)stmt);
				symbolTable.Assign(assignment.identifier, Evaluate(assignment.expression));
			} else if (stmt is ForLoop) {
				var loop = (ForLoop)stmt;
				int low = (int)Evaluate (loop.from);
				int high = (int)Evaluate(loop.to);				
				for(int i = low; i<=high; i++) {
					symbolTable.Assign(loop.variable, i);
					Interprete(loop.statements);
				}
			} else if (stmt is Print) {
				var print = (Print)stmt;
				object evaluation = Evaluate(print.expression);
				Console.Write (evaluation.ToString());
			} else if (stmt is Assert) {
				var assert = (Assert)stmt;
				if(!(bool)Evaluate(assert.assertion)) {
					assertionMessage(assert.location.line, assert.location.column, assert.assertion);
				}
			} else if (stmt is Read) {
				var read = (Read)stmt;
				//todo: convert to correct type
				string type = symbolTable.GetVariableType(read.identifier);
				object inputValue = readNextWord();
				if(type == TypeBindings.PRIMITIVE_INTEGER_NAME) {
					inputValue = int.Parse((string)inputValue);
				} else if(type == TypeBindings.PRIMITIVE_BOOLEAN_NAME) {
					inputValue = bool.Parse((string)inputValue);
				}			
				symbolTable.Assign(read.identifier, inputValue);
			}
		}

		private object Evaluate(Expression expression) 
		{
			if (expression is BinaryOperator) {
				var binOp = (BinaryOperator)expression;
				TypeBinding binding = TypeBindings.DecideType(binOp.leftOperand, symbolTable, errors);

				return TypeModels.EvaluateBinaryOperator(binding, binOp.oper.lexeme,
				                                  Evaluate (binOp.leftOperand), Evaluate(binOp.rightOperand));
			} else if (expression is UnaryOperator) {
				var unOp = (UnaryOperator)expression;
				TypeBinding binding = TypeBindings.DecideType(unOp.operand, symbolTable, errors);

				return TypeModels.EvaluateUnaryOperator(binding, unOp.oper.lexeme,
				                                        Evaluate (unOp.operand));
			} else if (expression is ExpressionLeaf) {
				var leaf = (ExpressionLeaf)expression;
				switch(leaf.token.category) 
				{
					case Category.Literal_Integer:
						return int.Parse(leaf.token.lexeme);
						break;
					case Category.Literal_String:
						return leaf.token.lexeme;
						break;
					case Category.Identifier:
						if (symbolTable.isDeclared (leaf.token)) {
							return symbolTable.GetValue(leaf.token);
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
			Console.WriteLine("Category: " + expression.head().category);
			Console.WriteLine ("Line " + expression.head().line);
			throw new InvalidOperationException("Unrecognized expression class");
		}


		private string readNextWord ()
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
			throw new EndOfStreamException("The input closed unexpectedly");
		}

		private void assertionMessage (int line, int column, Expression e)
		{
			string s = "Assertion near line " + line;
			s = s + " column " + column + " failed. Assertion \"" + e.ToString() + "\" was false.";
			Console.WriteLine(s);
		}
	}
}

