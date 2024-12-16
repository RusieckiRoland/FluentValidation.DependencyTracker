using FluentValidation.DependencyTracker.Tracker;
using FluentValidation.DependencyTracker.Extentions;
using FluentValidation.Tests.FluentValidation.DependencyTracker.TestHelpers.Models;

namespace FluentValidation.Tests.FluentValidation.DependencyTracker.Validators {
	public class TestPersonValidatorWithChildren : TrackingValidator<TestPerson> {
		public TestPersonValidatorWithChildren() {
			// Validate first name
			RuleForTrack(testPerson => testPerson.FirstName).NotEmpty()
					.WithMessage("First name must not be empty.");

		//	Validate age or parent consent
			RuleForEach(testPerson => testPerson.Children)
					.MustTrack(test => test.Age > 0)
					.WithMessage("Age must be greater than 18 or parent consent must be given.");

			//Validate each child in the Children collection
			RuleForEachTrack(testPerson => testPerson.Children)
					.MustTrack((testPerson,child) => child.Name != "Josef")
					.WithMessage("Child age must be positive.");
			.When()
		}
	}
}
