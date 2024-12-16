using FluentValidation.Tests.FluentValidation.DependencyTracker.TestHelpers.Factories;
using FluentValidation.Tests.FluentValidation.DependencyTracker.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FluentValidation.Tests.PropertyChainTests;
using Xunit;
using FluentValidation.Tests.FluentValidation.DependencyTracker.TestHelpers.Models;

namespace FluentValidation.Tests.FluentValidation.DependencyTracker.Tracker {
		public  class TrackingValidatorTests {

		[Fact]
		public void Should_FailValidation_When_ChildAge_IsInvalid() {
			// Arrange
			var validator = new TestPersonValidatorWithChildren();

			var testPerson = TestPersonFactory.CreateWithChildren(
					firstName: "Anna",
					lastName: "Smith",
					age: 40,
					new TestChild { Name = "Tom", Age = 10 },
					new TestChild { Name = "Lucy", Age = -1 } // Invalid child age
			);

			// Act
			var result = validator.Validate(testPerson);

			// Assert
			Assert.False(result.IsValid, "Validation should fail when any child has an invalid age.");
	
		}

	}
}
