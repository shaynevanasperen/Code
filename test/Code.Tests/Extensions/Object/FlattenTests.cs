using System;
using System.Collections.Generic;
using FluentAssertions;
using Code.Extensions.Object;

namespace Code.Tests.Extensions.Object
{
	public abstract class ObjectFlatten : ScenarioFor<object>
	{
		protected IEnumerable<object> Result;

		protected void WhenFlattening() => Result = SUT.Flatten();

		public class ForAtomic : ObjectFlatten
		{
			void GivenAnAtomicObject() => SUT = 1;
			void ThenItReturnsACollectionContainingJustThatObject() => Result.Should().BeEquivalentTo(1);
		}

		public class ForObjectGraph : ObjectFlatten
		{
			void GivenAnObjectGraph() => SUT = new
			{
				d = UriKind.Absolute,
				e = new[] { "one", "two", "three" },
				f = new[] { new[] { "x", "y", "z" }, new[] { "i", "ii", "iii" }, new[] { "£", "$", "#" } },
				a = "string",
				b = 1,
				c = (object)null,
				g = new { b = "second", a = "first" },
				h = new
				{
					b = 1,
					c = new[] { "alpha", "beta", "charlie" },
					a = new
					{
						b = "delta",
						a = "echo",
						c = "foxtrot"
					}
				}
			};
			void ThenItReturnsACollectionContainingAllTheAtomicValuesInTheGraphAlphabetically() =>
				Result.Should().BeEquivalentTo("string", 1, null, UriKind.Absolute, "one", "two", "three", "x", "y", "z", "i", "ii", "iii", "£", "$", "#", "first", "second", "echo", "delta", "foxtrot", 1, "alpha", "beta", "charlie");
		}
	}
}
