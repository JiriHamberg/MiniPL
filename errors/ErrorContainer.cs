using System;
using System.Collections.Generic;

namespace CompilersProject
{
	/*
	 * Container for all errors encountered during scanning, parsing and semantic analysis
	 */ 

	public class ErrorContainer
	{

		private Dictionary<ErrorType, List<ErrorEntry>> errors = new Dictionary<ErrorType, List<ErrorEntry>>();

		public ErrorContainer ()
		{
			foreach (var e in Enum.GetValues(typeof(ErrorType))) {
				errors.Add((ErrorType)e, new List<ErrorEntry>());
			}
		}

		public IList<ErrorEntry> getErrorsByType (ErrorType errType)
		{
			List<ErrorEntry> errList = null;
			if (!errors.TryGetValue (errType, out errList)) {
				throw new InvalidOperationException("Could not find error list for error type " + errType);
			}
			return errList.AsReadOnly();
		}

		public IList<ErrorEntry> getAllErrors ()
		{
			var errList = new List<ErrorEntry>();
			foreach (var list in errors.Values) {
				errList.AddRange(list);
			}
			return errList.AsReadOnly();
		}

		public void addError (ErrorEntry err)
		{
			List<ErrorEntry> errList = null;
			if (!errors.TryGetValue (err.type, out errList)) {
				throw new InvalidOperationException("Could not find error list for error type " + err.type);
			}
			errList.Add(err);
		}

		public void addError (Token token, ErrorType type, string message)
		{
			var err = new ErrorEntry(token, type, message);
			addError (err);
		}

		public void addError (int line, int column, ErrorType type, string message)
		{
			var err = new ErrorEntry(line, column, type, message);
			addError (err);
		}

	}

	public enum ErrorType {
		Lexical_Error,
		Syntax_Error,
		Semantic_Error
	}

	public class ErrorEntry
	{
		public Token token;
		public string message;
		public ErrorType type;

		public ErrorEntry (Token token, ErrorType type, string message)
		{
			this.token = token;
			this.type = type;
			this.message = message;
		}

		public ErrorEntry (int line, int column, ErrorType type, string message) : this(Token.errorToken(), type, message)
		{
			this.token.line = line;
			this.token.column = column;
		}

		public override string ToString ()
		{
			string s = "Error near ";
			if (token.category != Category.NONE) {
				s = s + "\"" + token.lexeme +"\" on ";
			}
			s = s + string.Format("line {0} column {1}: {2}",
			                     token.line, token.column, message);
			return s;
		}
	}

}

