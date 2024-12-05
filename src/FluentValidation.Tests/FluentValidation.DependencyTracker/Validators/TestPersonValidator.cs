using FluentValidation.DependencyTracker.Tracker;
using FluentValidation.Tests.FluentValidation.DependencyTracker.TestHelpers.Models;
using FluentValidation.DependencyTracker.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentValidation.Tests.FluentValidation.DependencyTracker.Validators {
		public class TestPersonValidator:TrackingValidator<TestPerson> {

				public TestPersonValidator() {
				 RuleForTrack(testPerson =>testPerson.FirstName).NotEmpty();
			   RuleForTrack(testPerson => testPerson.Age)
				.MustTrack((testPerson,age) => testPerson.Age > 18 || testPerson.ParentConsentGiven);

				}
		}
}
