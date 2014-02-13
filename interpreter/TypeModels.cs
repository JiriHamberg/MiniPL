using System;
using System.Collections.Generic;

namespace CompilersProject
{
	public static class TypeModels
	{

		private static Dictionary<TypeBinding, TypeModel> models = new Dictionary<TypeBinding, TypeModel>();

		static TypeModels ()
		{
			TypeModel intModel = new TypeModel(0);
			intModel.AddBinaryOperators(new Dictionary<Category, Func<object, object, object>>() {
				{Category.Operator_Addition, (left, right) =>
					(int)left + (int)right
				},
				{Category.Operator_Substraction, (left, right) =>
					(int)left - (int)right
				},
				{Category.Operator_Multiplication, (left, right) =>
					(int)left * (int)right
				},
				{Category.Operator_Division, (left, right) =>
					(int)left / (int)right
				},
				{Category.Operator_Less, (left, right) =>
					(int)left < (int)right
				},
				{Category.Operator_Equality, (left, right) =>
					(int)left == (int)right
				}

			});

			TypeModel boolModel = new TypeModel(false);
			boolModel.AddBinaryOperators (new Dictionary<Category, Func<object, object, object>>() {
				{Category.Operator_Equality, (left, right) =>
					(bool)left == (bool)right
				},
				{Category.Operator_And, (left, right) =>
					(bool)left && (bool)right
				}
			});

			boolModel.AddUnaryOperators(new Dictionary<Category, Func<object, object>>{
				{Category.Operator_Not, (operand) =>
					!(bool)operand
				}
			});

			TypeModel stringModel = new TypeModel("");
			stringModel.AddBinaryOperators(new Dictionary<Category, Func<object, object, object>>() {
				{Category.Operator_Equality, (left, right) =>
					(string)left == (string)right
				},
				{Category.Operator_Addition, (left, right) =>
					(string)left + (string)right
				}
				
			});

			models.Add(TypeBindings.integer, intModel);
			models.Add(TypeBindings.boolean, boolModel);
			models.Add (TypeBindings.str, stringModel);

		}

		public static object EvaluateBinaryOperator (TypeBinding type, Category oper, object leftOperand, object rightOperand)
		{
			TypeModel model;
			Func<object, object, object> binOp;
			if (!models.TryGetValue (type, out model)) {
				//errors.addError (ErrorType.Runtime_Error, "No implementation for typebinding " + type.ToString ());
				throw new InvalidOperationException("No implementation for typebinding " + type.ToString ());
			}
			if (!model.binaryOperatorImplementations.TryGetValue (oper, out binOp)) {
				throw new InvalidOperationException("Type model for type binding " + type.ToString() + " does not implement operator " + oper);
			}
			return binOp(leftOperand, rightOperand);
		}

		public static object EvaluateUnaryOperator (TypeBinding type, Category oper, object operand)
		{
			TypeModel model;
			Func<object,object> unOp;
			if (!models.TryGetValue (type, out model)) {
				throw new InvalidOperationException("No implementation for typebinding " + type.ToString ());
			}
			if (!model.unaryOperatorImplementations.TryGetValue (oper, out unOp)) {
				throw new InvalidOperationException("Type model for type binding " + type.ToString() + " does not implement operator " + oper);
			}
			return unOp(operand);
		}

		public static object GetDefaultValue(TypeBinding binding) {
			TypeModel model;
			if (!models.TryGetValue (binding, out model)) {
				throw new InvalidOperationException("No implementation for typebinding " + binding.ToString ());
			}
			return model.defaultValue;
		}
	}


	public class TypeModel{
		public object defaultValue;
		public Dictionary<Category, Func<object, object, object>> binaryOperatorImplementations;
		public Dictionary<Category, Func<object, object>> unaryOperatorImplementations;

		public TypeModel (object defaultValue)
		{
			this.defaultValue = defaultValue;
		}

		public void AddBinaryOperators (Dictionary<Category, Func<object, object, object>> binOps)
		{
			this.binaryOperatorImplementations = binOps;
		}

		public void AddUnaryOperators (Dictionary<Category, Func<object, object>> unOps)
		{
			this.unaryOperatorImplementations = unOps;
		}
	}

}

