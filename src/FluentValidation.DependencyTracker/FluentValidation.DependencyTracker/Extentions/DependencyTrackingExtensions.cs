using FluentValidation.DependencyTracker.Tracker;
using System.Linq.Expressions;
using FluentValidation.Internal;

namespace FluentValidation.DependencyTracker.Extentions {
	public static class DependencyTrackingExtensions {
		public static IRuleBuilderOptions<T, TProperty> MustTrack<T, TProperty>(
				this IRuleBuilder<T, TProperty> ruleBuilder,
				Expression<Func<T, TProperty, bool>> predicate) {
			if (ruleBuilder == null)
				throw new ArgumentNullException(nameof(ruleBuilder));

			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));

	
			if (ruleBuilder is not RuleBuilder<T, TProperty> ruleBuilderImpl)
				throw new InvalidOperationException("RuleBuilder implementation not recognized.");

			if (ruleBuilderImpl.ParentValidator is not TrackingValidator<T> trackingValidator)
				throw new InvalidOperationException("Validator must be of type TrackingValidator.");

		
			return trackingValidator.Must(ruleBuilder, predicate);
		}
	}
}
