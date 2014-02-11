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
			Scanner scanner = new Scanner(file);
		
			Parser parser = new Parser(scanner);

			System.Console.WriteLine("Scanning file " + path + " ...");

			AbstractSyntaxTree ast  = parser.Parse();

			System.Console.WriteLine(ast);
			

			/*while (scanner.HasNext ()) {
				Token next = scanner.Next ();
				String t = "Next Token: " + next.lexeme + "  " + next.category;
				t = t + " at line " + next.line.ToString() + " column " + next.column.ToString();

				System.Console.WriteLine(t);
			}*/

		}
	}
}
