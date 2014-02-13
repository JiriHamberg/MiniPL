using System;
using System.Collections.Generic;

namespace CompilersProject
{
	public class SymbolTable
	{

		private Dictionary<string, SymbolTableEntry> symbols = new Dictionary<string, SymbolTableEntry>();

		public SymbolTable ()
		{		
		}

		public void Declare(Token identifier, Token type, object value) 
		{
			symbols.Add(identifier.lexeme, new SymbolTableEntry( type.category, value));
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

		public object GetValue (Token identifier)
		{
			SymbolTableEntry entry;
			symbols.TryGetValue(identifier.lexeme, out entry);
			return entry.value;
		}

		public Category GetVariableType (Token identifier)
		{
			SymbolTableEntry entry;
			symbols.TryGetValue(identifier.lexeme, out entry);
			return entry.variableType;
		}

	}

	public class SymbolTableEntry {

		public readonly Category variableType;
		public object value {get; set;}

		public SymbolTableEntry (Category variableType, object value)
		{
			this.variableType = variableType;
			this.value = value;
		}

		public SymbolTableEntry (Category variableType) : this(variableType, null)
		{
		}

	}
}

