using System;
using System.Collections.Generic;

namespace CompilersProject
{

	public static class TypeSystem
	{

		public static Dictionary<string, TypeModel> types = new Dictionary<string, TypeModel>();

		public static readonly TypeModel boolean = new TypeModel("bool");
		public static readonly TypeModel str = new TypeModel("string");
		public static readonly TypeModel integer = new TypeModel("int");


		static TypeSystem ()
		{
			boolean.AddOperators(new Dictionary<Category, TypeModel> () {
				{Category.Operator_Equality, boolean},
				{Category.Operator_Not, boolean},
				{Category.Operator_And, boolean}
			});

			str.AddOperators(new Dictionary<Category, TypeModel> () {
				{Category.Operator_Addition, str},
				{Category.Operator_Equality, boolean}
			});

			integer.AddOperators(new Dictionary<Category, TypeModel> () {
				{Category.Operator_Addition, integer},
				{Category.Operator_Substraction, integer},
				{Category.Operator_Multiplication, integer},
				{Category.Operator_Division, integer},
				{Category.Operator_Less, boolean},
				{Category.Operator_Equality, boolean}
			});

		}

		public static Category GetCategoryFromType (TypeModel type)
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

		public static TypeModel GetTypeFromCategory (Category category)
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

	}

	public class TypeModel {

		public readonly string name;

		private Dictionary<Category, TypeModel> transitions;
			
		public TypeModel (string name, Dictionary<Category, TypeModel> transitions)
		{
			this.name = name;
			this.transitions = transitions;
		}

		public TypeModel (string name) : this(name, new Dictionary<Category, TypeModel>())
		{
		}

		public void AddOperator(Category oper, TypeModel targetType)
		{
			this.transitions.Add(oper, targetType);
		}

		public void AddOperators (Dictionary<Category, TypeModel> transitions)
		{
			this.transitions = transitions;
		}

		public TypeModel Operate(Category oper)
		{
			TypeModel res;
			transitions.TryGetValue(oper, out res);
			return res;
		}

	}
}

