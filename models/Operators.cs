using System;
using System.Collections.Generic;

namespace CompilersProject
{
	public static class Operators
	{
		public const string ADDITION = "+";
		public const string SUBSTRACTION = "-";
		public const string MULTIPLICATION = "*";
		public const string DIVISION = "/";

		public const string AND = "&";
		public const string EQUALITY = "=";
		public const string LESS = "<";
		public const string NOT = "!";
	

		static List<string> binaryOperators = new List<string>() {
			ADDITION, SUBSTRACTION, MULTIPLICATION, DIVISION, AND, EQUALITY, LESS
		};

		static List<string> unaryOperators = new List<string>() {
			NOT
		};

		public static bool IsBinaryOperator(string oper) {
			return binaryOperators.Contains(oper);
		}

		public static bool IsUnaryOperator(string oper) {
			return unaryOperators.Contains(oper);
		}

	}
}

