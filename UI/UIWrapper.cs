using System;
using System.IO;

namespace CompilersProject
{
	public class UIWrapper
	{

		StreamReader charStream;
		ErrorContainer errors = new ErrorContainer();
		Scanner scanner;
		Parser parser;
		AbstractSyntaxTree ast;
		SemanticAnalyser analyser;
		Interpreter interpreter = new Interpreter();

		public UIWrapper (StreamReader reader)
		{
			this.charStream = reader;
		}

		public void Run() {
			if(!DoLexicalAnalysis())
				return;
			if(!DoParsing())
				return;
			if(!DoSemanticAnylysis())
				return;
			interpreter.Interprete(ast);
		}

		bool DoLexicalAnalysis() {
			this.scanner = new Scanner (charStream, errors);
			string errMsg = "Your program contained some lexical errors and could not be interpreted:";
			return DoErrorCheckingAndWriteErrorMessages(errMsg, ErrorType.LexicalError);
		}

		bool DoParsing() {
			this.parser = new Parser(scanner, errors);
			this.ast = parser.Parse();
			string errMsg = "Your program contained some syntax errors and could not be interpreted:";
			return DoErrorCheckingAndWriteErrorMessages(errMsg, ErrorType.SyntaxError);
		}

		bool DoSemanticAnylysis() {
			this.analyser = new SemanticAnalyser(ast, errors);
			analyser.DoTypeChecking();
			string errMsg = "Your program contained some semantic errors and could not be interpreted:";
			return DoErrorCheckingAndWriteErrorMessages(errMsg, ErrorType.SemanticError);
		}

		bool DoErrorCheckingAndWriteErrorMessages(string msg, ErrorType errType) {
			var errList = errors.GetErrorsByType(errType);
			if (errList.Count > 0) {
				Console.WriteLine(msg);
				foreach(var e in errList) {
					Console.WriteLine(e.ToString());
				}
				return false;
			}
			return true;
		}

	}
}

