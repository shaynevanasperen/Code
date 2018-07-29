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

		public class ForAnonymousObject : ObjectToEnumerable
		{
			void GivenAnAnonymousObjectWithProperties() => SUT = new { Date = DateTime.MinValue, Value = 1 };
			void ThenItReturnsACollectionContainingThePropertyValuesOrderedByPropertyName() => Result.Should().BeEquivalentTo(DateTime.MinValue, 1);
		}

		public class ForDeclaredObject : ObjectToEnumerable
		{
			void GivenADeclaredObjectWithProperties()
			{
				var sut = new DeclaredObject(DateTime.MinValue, 1)
				{
					Base = "base",
					[TestEnum.A] = "A",
					[TestEnum.B] = "B"
				};
				SUT = sut;
			}

			void ThenItReturnsACollectionContainingThePropertyValuesOrderedByPropertyName() => Result.Should().BeEquivalentTo("base", DateTime.MinValue, 1);

			class DeclaredObject : BaseObject
			{
				readonly IDictionary<TestEnum, object> _items = new Dictionary<TestEnum, object>();

				public DeclaredObject(DateTime date, int value)
				{
					Date = date;
					Value = value;
				}

				public DateTime Date { get; private set; }

				public int Value { get; private set; }

				public static string Static { get; set; } = "static";

				string Private { get; set; } = "private";

				public object this[TestEnum index]
				{
					get => _items[index];
					set => _items[index] = value;
				}
			}

			enum TestEnum
			{
				A,
				B
			}

			class BaseObject
			{
				public string Base { get; set; }
			}
		}

		public class ForCollection : ObjectToEnumerable
		{
			void GivenACollection() => SUT = new[] { 1, 2, 3 };
			void ThenItReturnsTheGivenCollection() => Result.Should().BeEquivalentTo(1, 2, 3);
		}
	}
}
