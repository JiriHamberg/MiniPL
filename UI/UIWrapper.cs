using System;
using System.IO;

namespace CompilersProject
{
	public class UIWrapper
	{

		private StreamReader charStream;
		private ErrorContainer errors = new ErrorContainer();
		private Scanner scanner;
		private Parser parser;
		private AbstractSyntaxTree ast;
		private SemanticAnalyser analyser;
		private Interpreter interpreter = new Interpreter();

		public UIWrapper (StreamReader reader)
		{
			this.charStream = reader;
		}

		public void Run() {
			if(!doLexicalAnalysis())
				return;
			if(!doParsing())
				return;
			if(!doSemanticAnylysis())
				return;
			interpreter.Interprete(ast);
		}

		private bool doLexicalAnalysis() {
			this.scanner = new Scanner (charStream, errors);
			string errMsg = "Your program contained some lexical errors and could not be interpreted:";
			return doErrorCheckingAndWriteErrorMessages(errMsg, ErrorType.Lexical_Error);
		}

		private bool doParsing() {
			this.parser = new Parser(scanner, errors);
			this.ast = parser.Parse();
			string errMsg = "Your program contained some syntax errors and could not be interpreted:";
			return doErrorCheckingAndWriteErrorMessages(errMsg, ErrorType.Syntax_Error);
		}

		private bool doSemanticAnylysis() {
			this.analyser = new SemanticAnalyser(ast, errors);
			analyser.DoTypeChecking();
			string errMsg = "Your program contained some semantic errors and could not be interpreted:";
			return doErrorCheckingAndWriteErrorMessages(errMsg, ErrorType.Semantic_Error);
		}

		private bool doErrorCheckingAndWriteErrorMessages(string msg, ErrorType errType) {
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

