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

		public void addError (ErrorType type, string message)
		{
			var err = new ErrorEntry(type, message);
			addError (err);
		}

	}

	public enum ErrorType {
		Lexical_Error,
		Syntax_Error,
		Semantic_Error,
		Runtime_Error
	}

	public class ErrorEntry
	{
		public Token? token = null;
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
			Token t = Token.errorToken();
			t.line = line;
			t.column = column;
			this.token = t;
		}

		public ErrorEntry (ErrorType type, string message)
		{
			this.type = type;
			this.message = message;
		}

		public override string ToString ()
		{
			string s = "";
			if (token.HasValue) {
				s = s + "Error near ";
				if (token.Value.category != Category.NONE) {
					s = s + "\"" + token.Value.lexeme + "\" on ";
				}
				s = s + string.Format ("line {0} column {1}: {2}",
				                     token.Value.line, token.Value.column, message);
			} else {
				s = s + message;
			}
			return s;
		}
	}

}

