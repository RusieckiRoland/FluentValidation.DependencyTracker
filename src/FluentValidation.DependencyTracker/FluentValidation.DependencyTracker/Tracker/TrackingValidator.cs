using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace FluentValidation.DependencyTracker.Tracker {
	public class TrackingValidator<T> : AbstractValidator<T> {
		private ConcurrentDictionary<string, HashSet<string>> _dependencies = new();
		public IRuleBuilderInitial<T, TProperty> RuleForTrack<TProperty>(Expression<Func<T, TProperty>> expression) {
			if (expression == null) {
				throw new ArgumentNullException(nameof(expression));
			}

			TrackExpression(expression);

			var ruleBuilder = base.RuleFor(expression);


			return ruleBuilder;
		}

		internal IRuleBuilderOptions<T, TProperty> Must<TProperty>(
			IRuleBuilder<T, TProperty> ruleBuilder,
			Expression<Func<T, TProperty, bool>> predicate) {
			// Automatic dependency tracking
			TrackExpression(predicate.Body);

			// Call the original Must method from FluentValidation
			return ruleBuilder.Must(predicate.Compile());
		}



		private void TrackExpression(Expression expression) {
			AddDependencies(GetFieldNames(expression));
		}

		private List<string> GetFieldNames(Expression expression) {
			var fieldNames = new List<string>();

			switch (expression) {
				case MemberExpression memberExpression:
					// Add the field name
					fieldNames.Add(memberExpression.Member.Name);
					break;

				case BinaryExpression binaryExpression:
					// Analyze the left side
					fieldNames.AddRange(GetFieldNames(binaryExpression.Left));
					// Analyze the right side
					fieldNames.AddRange(GetFieldNames(binaryExpression.Right));
					break;

				case UnaryExpression unaryExpression:
					// Analyze operands (e.g., for negation: !isDev)
					fieldNames.AddRange(GetFieldNames(unaryExpression.Operand));
					break;

				case MethodCallExpression methodCallExpression:
					// Analyze arguments in method call
					foreach (var argument in methodCallExpression.Arguments) {
						fieldNames.AddRange(GetFieldNames(argument));
					}
					break;

				case LambdaExpression lambdaExpression:
					// Analyze the body of the lambda
					fieldNames.AddRange(GetFieldNames(lambdaExpression.Body));
					break;

				case ParameterExpression parameterExpression:
					// Add the parameter name if it's a field
					fieldNames.Add(parameterExpression.Name);
					break;

				case ConstantExpression constantExpression:
					// Ignore constant values
					break;

				default:
					throw new ArgumentException($"Unsupported expression type: {expression.NodeType}");
			}

			return fieldNames;
		}

		private void AddDependencies(List<string> fields) {
			if (fields.Count <= 1) {
				// Ignore lists with a single field
				return;
			}

			// For each field in the list, add dependencies
			foreach (var field in fields) {
				// Add or update the key `field`
				_dependencies.AddOrUpdate(
						field,
						_ => CreateHashSetWithoutSelf(fields, field), // New key
						(_, existingDependencies) => {
							lock (existingDependencies) // Synchronize `HashSet`
							{
								foreach (var relatedField in fields) {
									if (field != relatedField) {
										existingDependencies.Add(relatedField); // Add dependency
									}
								}
							}
							return existingDependencies;
						});

				// Ensure dependencies are mutual
				foreach (var relatedField in fields) {
					if (field != relatedField) {
						_dependencies.AddOrUpdate(
								relatedField,
								_ => new HashSet<string> { field }, // New key
								(_, existingDependencies) => {
									lock (existingDependencies) {
										existingDependencies.Add(field);
									}
									return existingDependencies;
								});
					}
				}
			}
		}

		private HashSet<string> CreateHashSetWithoutSelf(List<string> fields, string self) {
			var result = new HashSet<string>();
			foreach (var field in fields) {
				if (field != self) {
					result.Add(field);
				}
			}
			return result;
		}


	}
}
