using Xunit;
using FluentValidation.Tests.FluentValidation.DependencyTracker.Validators;
using FluentValidation.Tests.FluentValidation.DependencyTracker.TestHelpers.Factories;

namespace FluentValidation.Tests.FluentValidation.DependencyTracker.Extensions {
		public class DependencyTrackingExtensionsTests {

		[Fact]
		public void Should_ThrowArgumentNullException_When_RuleBuilder_Is_Null() {
			// Arrange


			var validator = new TestPersonValidator();

			var testPerson = TestPersonFactory.CreateMinorWithoutConsent();

			var result = validator.Validate(testPerson);

			Assert.False(result.IsValid, "Validation should fail for child without the parent consent");

		}


		

		
	}
}
