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
			symbolLock.Push(identifier.lexeme);
		}

		public void Unlock ()
		{
			symbolLock.Pop();
		}

		public void Declare(Token identifier, Token type, object value) 
		{
			symbols.Add(identifier.lexeme, new SymbolTableEntry( type.lexeme, value));
		}

		public void Declare (Token identifier, Token type)
		{
			Declare (identifier, type, null);
		}

		public void Assign (Token token, object value)
		{
			SymbolTableEntry entry;
			symbols.TryGetValue(token.lexeme, out entry);
			entry.value = value;
		}

		public bool isDeclared (Token identifier)
		{
			return symbols.ContainsKey(identifier.lexeme);
		}

		public bool isLocked (Token identifier)
		{
			return symbolLock.Contains(identifier.lexeme);
		}

		public object GetValue (Token identifier)
		{
			SymbolTableEntry entry;
			symbols.TryGetValue(identifier.lexeme, out entry);
			return entry.value;
		}

		public string GetVariableType (Token identifier)
		{
			SymbolTableEntry entry;
			symbols.TryGetValue(identifier.lexeme, out entry);
			return entry.variableType;
		}

	}

	public class SymbolTableEntry {

		public readonly string variableType;
		public object value {get; set;}

		public SymbolTableEntry (string variableType, object value)
		{
			this.variableType = variableType;
			this.value = value;
		}

		public SymbolTableEntry (string variableType) : this(variableType, null)
		{
		}

	}
}

