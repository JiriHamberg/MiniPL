using System;
using System.Collections.Generic;

namespace CompilersProject
{

	/*
	 * Datastructure 
	 */ 

	public static class TypeBindings
	{

		private static Dictionary<string, TypeBinding> types = new Dictionary<string, TypeBinding>();

		public const string PRIMITIVE_INTEGER_NAME = "int";
		public const string PRIMITIVE_STRING_NAME = "string";
		public const string PRIMITIVE_BOOLEAN_NAME = "bool";


		static TypeBindings ()
		{
			TypeBinding boolean = new TypeBinding(PRIMITIVE_BOOLEAN_NAME);
			TypeBinding str = new TypeBinding(PRIMITIVE_STRING_NAME);
			TypeBinding integer = new TypeBinding(PRIMITIVE_INTEGER_NAME);

			boolean.AddOperators(new Dictionary<string, TypeBinding> () {
				{Operators.EQUALITY, boolean},
				{Operators.NOT, boolean},
				{Operators.AND, boolean}
			});

			str.AddOperators(new Dictionary<string, TypeBinding> () {
				{Operators.ADDITION, str},
				{Operators.EQUALITY, boolean}
			});

			integer.AddOperators(new Dictionary<string, TypeBinding> () {
				{Operators.ADDITION, integer},
				{Operators.SUBSTRACTION, integer},
				{Operators.MULTIPLICATION, integer},
				{Operators.DIVISION, integer},
				{Operators.LESS, boolean},
				{Operators.EQUALITY, boolean}
			});
			AddTypeBinding(boolean);
			AddTypeBinding(str);
			AddTypeBinding(integer);
		}

		public static void AddTypeBinding (TypeBinding binding)
		{
			types.Add(binding.Name, binding);
		}

		public static TypeBinding GetTypeByName (string type)
		{
			TypeBinding binding; 
			if(!types.TryGetValue(type, out binding)) {
				//at the moment types are hard coded, and invalid types cause
				//lexical error so this is indication of faulty program logic
				throw new InvalidOperationException("Invalid type: " + type);
			}
			return binding;
		}


		public static TypeBinding DecideType (Expression expression, SymbolTable symbolTable, ErrorContainer errors)
		{
			if (expression is BinaryOperator) {
				var binOp = (BinaryOperator)expression;
				TypeBinding leftType = DecideType (binOp.LeftOperand, symbolTable, errors);
				TypeBinding rightType = DecideType (binOp.RightOperand, symbolTable, errors);
				if(leftType == null || rightType == null) {
					return null;
				}
				if (leftType != rightType) {
					errors.AddError (binOp.Oper, ErrorType.Semantic_Error, "Types of left and right operand do not match");
					return null;
				}
				var ret = rightType.Operate (binOp.Oper.Lexeme);
				if(ret == null) {
					errors.AddError (binOp.Oper, ErrorType.Semantic_Error, "Could not apply operator to given types");
				}
				return ret; 
			} else if (expression is UnaryOperator) {
				var unOp = (UnaryOperator)expression;
				TypeBinding operandType = DecideType (unOp.Operand, symbolTable, errors);
				if (operandType == null) {
					return null;
				}
				var ret = operandType.Operate (unOp.Oper.Lexeme);
				if(ret == null) {
					errors.AddError(unOp.Oper, ErrorType.Semantic_Error, "Could not apply operator to given type");
				}
				return ret;
			} else if (expression is ExpressionLeaf) {
				var leaf = (ExpressionLeaf)expression;
				switch(leaf.Token.Category) {
				case Category.Literal_Integer:
					return GetTypeByName(PRIMITIVE_INTEGER_NAME);
				case Category.Literal_String:
					return GetTypeByName(PRIMITIVE_STRING_NAME);
				case Category.Identifier:
					if (symbolTable.IsDeclared (leaf.Token)) {
						return TypeBindings.GetTypeByName(symbolTable.GetVariableType(leaf.Token));
					} else {
						errors.AddError (leaf.Token, ErrorType.Semantic_Error, "Undeclared variable");
						return null;
					}
				}
				return null;
			}

			return null;
		}


	}

	public class TypeBinding {

		public readonly string Name;

		private Dictionary<string, TypeBinding> transitions;
			
		public TypeBinding (string name, Dictionary<string, TypeBinding> transitions)
		{
			this.Name = name;
			this.transitions = transitions;
		}

		public TypeBinding (string name) : this(name, new Dictionary<string, TypeBinding>())
		{
		}

		public void AddOperator(string oper, TypeBinding targetType)
		{
			this.transitions.Add(oper, targetType);
		}

		public void AddOperators (Dictionary<string, TypeBinding> transitions)
		{
			this.transitions = transitions;
		}

		public TypeBinding Operate(string oper)
		{
			TypeBinding res;
			transitions.TryGetValue(oper, out res);
			return res;
		}

	}
}

