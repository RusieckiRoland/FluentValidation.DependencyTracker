using FluentValidation.Tests.FluentValidation.DependencyTracker.TestHelpers.Models;
using System.Collections.Generic;
using System.Linq;
using static FluentValidation.Tests.PropertyChainTests;

namespace FluentValidation.Tests.FluentValidation.DependencyTracker.TestHelpers.Factories {
	public static class TestPersonFactory {
		public static TestPerson CreateAdult(string firstName = "John", string lastName = "Doe", int age = 25) {
			return new TestPerson {
				FirstName = firstName,
				LastName = lastName,
				Age = age,
				ParentConsentGiven = false,
				ParentFirstName = null,
				ParentLastName = null
			};
		}

		public static TestPerson CreateMinorWithoutConsent(string firstName = "Jane", string lastName = "Doe", int age = 15) {
			return new TestPerson {
				FirstName = firstName,
				LastName = lastName,
				Age = age,
				ParentConsentGiven = false,
				ParentFirstName = null,
				ParentLastName = null
			};
		}

		public static TestPerson CreateMinorWithConsent(string firstName = "Alex", string lastName = "Smith", int age = 16,
																										 string parentFirstName = "Sarah", string parentLastName = "Smith") {
			return new TestPerson {
				FirstName = firstName,
				LastName = lastName,
				Age = age,
				ParentConsentGiven = true, // Zgoda rodziców została udzielona
				ParentFirstName = parentFirstName,
				ParentLastName = parentLastName
			};
		}

		public static TestPerson CreateEmptyPerson() {
			return new TestPerson {
				FirstName = null,
				LastName = null,
				Age = 0,
				ParentConsentGiven = false,
				ParentFirstName = null,
				ParentLastName = null
			};
		}

		public static TestPerson CreateWithChildren(string firstName = "John", string lastName = "Doe", int age = 35, params TestChild[] children) {
			return new TestPerson {
				FirstName = firstName,
				LastName = lastName,
				Age = age,
				ParentConsentGiven = false,
				ParentFirstName = null,
				ParentLastName = null,
				Children = children.ToList()
			};
		}
	}
}
