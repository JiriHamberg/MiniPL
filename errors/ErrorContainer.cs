using System;
using System.Collections.Generic;

namespace CompilersProject
{
	/*
	 * Container for all errors encountered during scanning, parsing and semantic analysis
	 */ 

	public class ErrorContainer
	{

		Dictionary<ErrorType, List<ErrorEntry>> errors = new Dictionary<ErrorType, List<ErrorEntry>>();

		public ErrorContainer ()
		{
			foreach (var e in Enum.GetValues(typeof(ErrorType))) {
				errors.Add((ErrorType)e, new List<ErrorEntry>());
			}
		}

		public IList<ErrorEntry> GetErrorsByType (ErrorType errType)
		{
			List<ErrorEntry> errList = null;
			if (!errors.TryGetValue (errType, out errList)) {
				throw new InvalidOperationException("Could not find error list for error type " + errType);
			}
			return errList.AsReadOnly();
		}

		public IList<ErrorEntry> GetAllErrors ()
		{
			var errList = new List<ErrorEntry>();
			foreach (var list in errors.Values) {
				errList.AddRange(list);
			}
			return errList.AsReadOnly();
		}

		public void AddError (ErrorEntry err)
		{
			List<ErrorEntry> errList = null;
			if (!errors.TryGetValue (err.Type, out errList)) {
				throw new InvalidOperationException("Could not find error list for error type " + err.Type);
			}
			errList.Add(err);
		}

		public void AddError (Token token, ErrorType type, string message)
		{
			var err = new ErrorEntry(token, type, message);
			AddError (err);
		}

		public void AddError (int line, int column, ErrorType type, string message)
		{
			var err = new ErrorEntry(line, column, type, message);
			AddError (err);
		}

		public void AddError (ErrorType type, string message)
		{
			var err = new ErrorEntry(type, message);
			AddError (err);
		}

	}

	public enum ErrorType {
		LexicalError,
		SyntaxError,
		SemanticError,
		RuntimeError
	}

	public class ErrorEntry
	{
		public Token? NearToken = null;
		public string Message;
		public ErrorType Type;

		public ErrorEntry (Token token, ErrorType type, string message)
		{
			this.NearToken = token;
			this.Type = type;
			this.Message = message;
		}

		public ErrorEntry (int line, int column, ErrorType type, string message) : this(Token.ErrorToken(), type, message)
		{
			Token t = Token.ErrorToken();
			t.Line = line;
			t.Column = column;
			this.NearToken = t;
		}

		public ErrorEntry (ErrorType type, string message)
		{
			this.Type = type;
			this.Message = message;
		}

		public override string ToString ()
		{
			string s = "";
			if (NearToken.HasValue) {
				s = s + "Error near ";
				if (NearToken.Value.Category != Category.NONE) {
					s = s + "\"" + NearToken.Value.Lexeme + "\" on ";
				}
				s = s + string.Format ("line {0} column {1}: {2}",
				                     NearToken.Value.Line, NearToken.Value.Column, Message);
			} else {
				s = s + Message;
			}
			return s;
		}
	}

}

