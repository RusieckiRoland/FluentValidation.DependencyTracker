using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentValidation.Tests.FluentValidation.DependencyTracker.TestHelpers.Models {
	public class TestPerson {
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int Age { get; set; }
		public bool ParentConsentGiven { get; set; } 
		public string ParentFirstName { get; set; } 
		public string ParentLastName { get; set; }  
	}
}
