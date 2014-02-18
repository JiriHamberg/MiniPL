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
			intModel.AddBinaryOperators(new Dictionary<string, Func<object, object, object>>() {
				{Operators.ADDITION, (left, right) =>
					(int)left + (int)right
				},
				{Operators.SUBSTRACTION, (left, right) =>
					(int)left - (int)right
				},
				{Operators.MULTIPLICATION, (left, right) =>
					(int)left * (int)right
				},
				{Operators.DIVISION, (left, right) =>
					(int)left / (int)right
				},
				{Operators.LESS, (left, right) =>
					(int)left < (int)right
				},
				{Operators.EQUALITY, (left, right) =>
					(int)left == (int)right
				}

			});

			TypeModel boolModel = new TypeModel(false);
			boolModel.AddBinaryOperators (new Dictionary<string, Func<object, object, object>>() {
				{Operators.EQUALITY, (left, right) =>
					(bool)left == (bool)right
				},
				{Operators.AND, (left, right) =>
					(bool)left && (bool)right
				}
			});

			boolModel.AddUnaryOperators(new Dictionary<string, Func<object, object>>{
				{Operators.NOT, (operand) =>
					!(bool)operand
				}
			});

			TypeModel stringModel = new TypeModel("");
			stringModel.AddBinaryOperators(new Dictionary<string, Func<object, object, object>>() {
				{Operators.EQUALITY, (left, right) =>
					(string)left == (string)right
				},
				{Operators.ADDITION, (left, right) =>
					(string)left + (string)right
				}
				
			});

			models.Add(TypeBindings.GetTypeByName(TypeBindings.PRIMITIVE_INTEGER_NAME), intModel);
			models.Add(TypeBindings.GetTypeByName(TypeBindings.PRIMITIVE_BOOLEAN_NAME), boolModel);
			models.Add (TypeBindings.GetTypeByName(TypeBindings.PRIMITIVE_STRING_NAME), stringModel);

		}

		public static object EvaluateBinaryOperator (TypeBinding type, string oper, object leftOperand, object rightOperand)
		{
			TypeModel model;
			Func<object, object, object> binOp;
			if (!models.TryGetValue (type, out model)) {
				//errors.addError (ErrorType.Runtime_Error, "No implementation for typebinding " + type.ToString ());
				throw new InvalidOperationException("No implementation for typebinding " + type.ToString ());
			}
			if (!model.BinaryOperatorImplementations.TryGetValue (oper, out binOp)) {
				throw new InvalidOperationException("Type model for type binding " + type.ToString() + " does not implement operator " + oper);
			}
			return binOp(leftOperand, rightOperand);
		}

		public static object EvaluateUnaryOperator (TypeBinding type, string oper, object operand)
		{
			TypeModel model;
			Func<object,object> unOp;
			if (!models.TryGetValue (type, out model)) {
				throw new InvalidOperationException("No implementation for typebinding " + type.ToString ());
			}
			if (!model.UnaryOperatorImplementations.TryGetValue (oper, out unOp)) {
				throw new InvalidOperationException("Type model for type binding " + type.ToString() + " does not implement operator " + oper);
			}
			return unOp(operand);
		}

		public static object GetDefaultValue(TypeBinding binding) {
			TypeModel model;
			if (!models.TryGetValue (binding, out model)) {
				throw new InvalidOperationException("No implementation for typebinding " + binding.ToString ());
			}
			return model.DefaultValue;
		}
	}


	public class TypeModel {
		public object DefaultValue;
		public Dictionary<string, Func<object, object, object>> BinaryOperatorImplementations;
		public Dictionary<string, Func<object, object>> UnaryOperatorImplementations;

		public TypeModel (object defaultValue)
		{
			this.DefaultValue = defaultValue;
		}

		public void AddBinaryOperators (Dictionary<string, Func<object, object, object>> binOps)
		{
			this.BinaryOperatorImplementations = binOps;
		}

		public void AddUnaryOperators (Dictionary<string, Func<object, object>> unOps)
		{
			this.UnaryOperatorImplementations = unOps;
		}
	}

}

