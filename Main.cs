using System;
using System.IO;

namespace CompilersProject
{
	class MainClass
	{
		public static void Main (string[] args)
		{

			String path = "examples/test.txt";
			StreamReader file = File.OpenText(path);
			ErrorContainer errors = new ErrorContainer();
			System.Console.WriteLine("Scanning file " + path + " ...");
			Scanner scanner = new Scanner(file, errors);
		
			//print lexical errors
			foreach(ErrorEntry e in errors.getErrorsByType(ErrorType.Lexical_Error)) {
				System.Console.WriteLine("Lexical error");
				System.Console.WriteLine(e.ToString());
			}

			Parser parser = new Parser(scanner, errors);

			AbstractSyntaxTree ast  = parser.Parse();

			//print syntax errors
			foreach(ErrorEntry e in errors.getErrorsByType(ErrorType.Syntax_Error)) {
				System.Console.WriteLine("Syntax error");
				System.Console.WriteLine(e.ToString());
			}

			System.Console.WriteLine(ast);

			SemanticAnalyser semanticAnalyser = new SemanticAnalyser(ast, errors);

			semanticAnalyser.doTypeChecking();

			foreach(ErrorEntry e in errors.getErrorsByType(ErrorType.Semantic_Error)) {
				System.Console.WriteLine("Semantic error");
				System.Console.WriteLine(e.ToString());
			}


			/*while (scanner.HasNext ()) {
				Token next = scanner.Next ();
				String t = "Next Token: " + next.lexeme + "  " + next.category;
				t = t + " at line " + next.line.ToString() + " column " + next.column.ToString();

				System.Console.WriteLine(t);
			}*/

		}
	}
}
