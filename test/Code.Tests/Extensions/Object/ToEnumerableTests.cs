using System;
using System.Collections.Generic;
using Code.Extensions.Object;
using FluentAssertions;

namespace Code.Tests.Extensions.Object
{
	public abstract class ObjectToEnumerable : ScenarioFor<object>
	{
		protected IEnumerable<object> Result;

		protected void WhenTransformingAnObjectToEnumerable() => Result = SUT.ToEnumerable();

		public class ForObject : ObjectToEnumerable
		{
			void GivenAnObjectWithProperties() => SUT = new { Date = DateTime.MinValue, Value = 1 };
			void ThenItReturnsACollectionContainingThePropertyValuesOrderedByPropertyName() => Result.Should().BeEquivalentTo(DateTime.MinValue, 1);
		}

		public class ForCollection : ObjectToEnumerable
		{
			void GivenACollection() => SUT = new[] { 1, 2, 3 };
			void ThenItReturnsTheGivenCollection() => Result.Should().BeEquivalentTo(1, 2, 3);
		}
	}
}
