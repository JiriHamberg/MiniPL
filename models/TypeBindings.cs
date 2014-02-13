using System;
using System.Collections.Generic;

namespace CompilersProject
{

	/*
	 * Datastructure 
	 */ 

	public static class TypeBindings
	{

		public static Dictionary<string, TypeBinding> types = new Dictionary<string, TypeBinding>();

		public static readonly TypeBinding boolean = new TypeBinding("bool");
		public static readonly TypeBinding str = new TypeBinding("string");
		public static readonly TypeBinding integer = new TypeBinding("int");


		static TypeBindings ()
		{
			boolean.AddOperators(new Dictionary<Category, TypeBinding> () {
				{Category.Operator_Equality, boolean},
				{Category.Operator_Not, boolean},
				{Category.Operator_And, boolean}
			});

			str.AddOperators(new Dictionary<Category, TypeBinding> () {
				{Category.Operator_Addition, str},
				{Category.Operator_Equality, boolean}
			});

			integer.AddOperators(new Dictionary<Category, TypeBinding> () {
				{Category.Operator_Addition, integer},
				{Category.Operator_Substraction, integer},
				{Category.Operator_Multiplication, integer},
				{Category.Operator_Division, integer},
				{Category.Operator_Less, boolean},
				{Category.Operator_Equality, boolean}
			});
		}

		public static Category GetCategoryFromType (TypeBinding type)
		{
			if (type == boolean) {
				return Category.Type_Boolean;
			} else if (type == str) {
				return Category.Type_String;
			} else if (type == integer) {
				return Category.Type_Integer;
			}
			return Category.NONE;
		}

		public static TypeBinding GetTypeFromCategory (Category category)
		{
			if (category == Category.Type_Integer) {
				return integer;
			} else if (category == Category.Type_Boolean) {
				return boolean;
			} else if (category == Category.Type_String) {
				return str;
			}
			return null;
		}

		public static TypeBinding DecideType (Expression expression, SymbolTable symbolTable, ErrorContainer errors)
		{
			if (expression is BinaryOperator) {
				var binOp = (BinaryOperator)expression;
				TypeBinding leftType = DecideType (binOp.leftOperand, symbolTable, errors);
				TypeBinding rightType = DecideType (binOp.rigtOperand, symbolTable, errors);
				if(leftType == null || rightType == null) {
					return null;
				}
				if (leftType != rightType) {
					errors.addError (binOp.oper, ErrorType.Semantic_Error, "Types of left and right operand do not match");
					return null;
				}
				var ret = rightType.Operate (binOp.oper.category);
				if(ret == null) {
					errors.addError (binOp.oper, ErrorType.Semantic_Error, "Could not apply operator to given types");
				}
				return ret; 
			} else if (expression is UnaryOperator) {
				var unOp = (UnaryOperator)expression;
				TypeBinding operandType = DecideType (unOp.operand, symbolTable, errors);
				if (operandType == null) {
					return null;
				}
				var ret = operandType.Operate (unOp.oper.category);
				if(ret == null) {
					errors.addError(unOp.oper, ErrorType.Semantic_Error, "Could not apply operator to given type");
				}
				return ret;
			} else if (expression is ExpressionLeaf) {
				var leaf = (ExpressionLeaf)expression;
				switch(leaf.token.category) {
				case Category.Literal_Integer:
					return TypeBindings.integer;
				case Category.Literal_Boolean:
					return TypeBindings.boolean;
				case Category.Literal_String:
					return TypeBindings.str;
				case Category.Identifier:
					if (symbolTable.isDeclared (leaf.token)) {
						return TypeBindings.GetTypeFromCategory(symbolTable.GetVariableType(leaf.token));
					} else {
						errors.addError (leaf.token, ErrorType.Semantic_Error, "Undeclared variable");
						return null;
					}
				}
				return null;
			}

			return null;
		}


	}

	public class TypeBinding {

		public readonly string name;

		private Dictionary<Category, TypeBinding> transitions;
			
		public TypeBinding (string name, Dictionary<Category, TypeBinding> transitions)
		{
			this.name = name;
			this.transitions = transitions;
		}

		public TypeBinding (string name) : this(name, new Dictionary<Category, TypeBinding>())
		{
		}

		public void AddOperator(Category oper, TypeBinding targetType)
		{
			this.transitions.Add(oper, targetType);
		}

		public void AddOperators (Dictionary<Category, TypeBinding> transitions)
		{
			this.transitions = transitions;
		}

		public TypeBinding Operate(Category oper)
		{
			TypeBinding res;
			transitions.TryGetValue(oper, out res);
			return res;
		}

	}
}

