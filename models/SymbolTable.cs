using System;
using System.Collections.Generic;

namespace CompilersProject
{
	public class SymbolTable
	{

		private Dictionary<string, SymbolTableEntry> symbols = new Dictionary<string, SymbolTableEntry>();
		private Stack<string> symbolLock = new Stack<string>();



		public SymbolTable ()
		{		
		}

		public void Lock (Token identifier)
		{
			symbolLock.Push(identifier.Lexeme);
		}

		public void Unlock ()
		{
			symbolLock.Pop();
		}

		public void Declare(Token identifier, Token type, object value) 
		{
			symbols.Add(identifier.Lexeme, new SymbolTableEntry( type.Lexeme, value));
		}

		public void Declare (Token identifier, Token type)
		{
			Declare (identifier, type, null);
		}

		public void Assign (Token token, object value)
		{
			SymbolTableEntry entry;
			symbols.TryGetValue(token.Lexeme, out entry);
			entry.Value = value;
		}

		public bool IsDeclared (Token identifier)
		{
			return symbols.ContainsKey(identifier.Lexeme);
		}

		public bool IsLocked (Token identifier)
		{
			return symbolLock.Contains(identifier.Lexeme);
		}

		public object GetValue (Token identifier)
		{
			SymbolTableEntry entry;
			symbols.TryGetValue(identifier.Lexeme, out entry);
			return entry.Value;
		}

		public string GetVariableType (Token identifier)
		{
			SymbolTableEntry entry;
			symbols.TryGetValue(identifier.Lexeme, out entry);
			return entry.VariableType;
		}

	}

	public class SymbolTableEntry {

		public readonly string VariableType;
		public object Value {get; set;}

		public SymbolTableEntry (string variableType, object value)
		{
			this.VariableType = variableType;
			this.Value = value;
		}

		public SymbolTableEntry (string variableType) : this(variableType, null)
		{
		}

	}
}

