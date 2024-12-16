using System.Collections.Concurrent;
using System.Linq.Expressions;
using FluentValidation.Internal;
using FluentValidation.Results;

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


		/// <summary>
		/// Validates a single property of an object using the rules defined in the validator.
		/// </summary>
		/// <typeparam name="TProperty">The type of the property to validate.</typeparam>
		/// <param name="instance">The object containing the property to validate.</param>
		/// <param name="propertyExpression">An expression pointing to the property to validate, e.g., <c>x => x.PropertyName</c>.</param>
		/// <returns>
		/// A <see cref="ValidationResult"/> containing the validation results for the specified property.
		/// If the property is valid, the <see cref="ValidationResult.Errors"/> collection will be empty.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyExpression"/> is null.</exception>
		public ValidationResult ValidateProperty<TProperty>(T instance, Expression<Func<T, TProperty>> propertyExpression) {
			if (propertyExpression == null) {
				throw new ArgumentNullException(nameof(propertyExpression));
			}

			// Extract the name of the property from the lambda expression
			var propertyName = ((MemberExpression)propertyExpression.Body).Member.Name;

			// Create a ValidationContext and specify the property to validate using a selector
			var context = new ValidationContext<T>(instance, new PropertyChain(), new MemberNameValidatorSelector(new[] { propertyName }));

			// Execute validation and return the result
			return Validate(context);
		}

		/// <summary>
		/// Validates a single property of an object using the rules defined in the validator.
		/// </summary>
		/// <param name="instance">The object containing the property to validate.</param>
		/// <param name="propertyName">The name of the property to validate.</param>
		/// <returns>
		/// A <see cref="ValidationResult"/> containing the validation results for the specified property.
		/// If the property is valid, the <see cref="ValidationResult.Errors"/> collection will be empty.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyName"/> is null or empty.</exception>
		public ValidationResult ValidateProperty(T instance, string propertyName) {
			if (string.IsNullOrEmpty(propertyName)) {
				throw new ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");
			}

			// Create a ValidationContext and specify the property to validate using a selector
			var context = new ValidationContext<T>(instance, new PropertyChain(), new MemberNameValidatorSelector(new[] { propertyName }));

			// Execute validation and return the result
			return Validate(context);
		}





		public IRuleBuilderInitialCollection<T, TElement> RuleForEachTrack<TElement>(Expression<Func<T, IEnumerable<TElement>>> expression) {
			if (expression == null) {
				throw new ArgumentNullException(nameof(expression));
			}

			// Track dependencies for the collection expression
			TrackExpression(expression);

			// Call the base RuleForEach method to define validation rules for each element in the collection
			var ruleBuilder = base.RuleForEach(expression);

			return ruleBuilder;
		}

		


		internal IRuleBuilderOptions<T, TProperty> Must<TProperty>(
			IRuleBuilder<T, TProperty> ruleBuilder,
			Expression<Func<TProperty, bool>> expression) {
			// Automatic dependency tracking
			TrackExpression(expression.Body);

			// Call the original Must method from FluentValidation
			return ruleBuilder.Must(expression.Compile());
		}

		internal IRuleBuilderOptions<T, TProperty> Must<TProperty>(
			IRuleBuilder<T, TProperty> ruleBuilder,
			Expression<Func<T, TProperty, bool>> expression) {
			// Automatic dependency tracking
			TrackExpression(expression.Body);

			// Call the original Must method from FluentValidation
			return ruleBuilder.Must(expression.Compile());
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

				case NewExpression newExpression:
					//eg x=>new {x.Field1,x.Field2}
					 foreach (var argument in newExpression.Arguments)
            {
                fieldNames.AddRange(GetFieldNames(argument));
            }
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


		public IConditionBuilder WhenTrack(Expression<Func<T, bool>> expression, Action action) {
			TrackExpression(expression);
			var predicate = expression.Compile();
			return When((x, _) => predicate(x), action);
		}

		public IConditionBuilder WhenTrack(Expression<Func<T, ValidationContext<T>, bool>> expression, Action action) {
			TrackExpression(expression);
			var predicate = expression.Compile();			
			return new ConditionBuilder<T>(Rules).When(predicate, action);
		}
	}
}
