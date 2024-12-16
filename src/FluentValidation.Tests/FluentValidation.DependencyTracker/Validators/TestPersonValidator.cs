using FluentValidation.DependencyTracker.Tracker;
using FluentValidation.Tests.FluentValidation.DependencyTracker.TestHelpers.Models;
using FluentValidation.DependencyTracker.Extentions;

namespace FluentValidation.Tests.FluentValidation.DependencyTracker.Validators {
		public class TestPersonValidator:TrackingValidator<TestPerson> {

				public TestPersonValidator() {
				 RuleForTrack(testPerson =>testPerson.FirstName).NotEmpty();
			   RuleForTrack(testPerson => testPerson)
				.MustTrack((testPerson) => testPerson.Age > 18 || testPerson.ParentConsentGiven);

				}
		}
}
