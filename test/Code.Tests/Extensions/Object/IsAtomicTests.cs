using System;
using FluentAssertions;
using Code.Extensions.Object;
using TestStack.BDDfy;

namespace Code.Tests.Extensions.Object
{
	public abstract class ObjectIsAtomic : ScenarioFor<object>
	{
		protected bool Result;

		protected void WhenCheckingIfItIsAtomic() => Result = SUT.IsAtomic();

		public class ForNull : ObjectIsAtomic
		{
			void GivenANullObject() => SUT = null;
			void ThenItReturnsTrue() => Result.Should().BeTrue();
		}

		public class ForString : ObjectIsAtomic
		{
			void GivenAString() => SUT = Guid.NewGuid().ToString();
			void ThenItReturnsTrue() => Result.Should().BeTrue();
		}

		public class ForEnum : ObjectIsAtomic
		{
			void GivenAnEnum() => SUT = UriKind.Absolute;
			void ThenItReturnsTrue() => Result.Should().BeTrue();
		}

		public class ForDateTime : ObjectIsAtomic
		{
			void GivenAnEnum() => SUT = DateTime.Now;
			void ThenItReturnsTrue() => Result.Should().BeTrue();
		}

		public class ForDecimal : ObjectIsAtomic
		{
			void GivenADecimal() => SUT = decimal.MinValue;
			void ThenItReturnsTrue() => Result.Should().BeTrue();
		}

		public class ForPrimitive : ObjectIsAtomic
		{
			public ForPrimitive()
			{
				Examples = new ExampleTable("Primitive")
				{
					true,
					byte.MinValue,
					sbyte.MinValue,
					short.MinValue,
					ushort.MinValue,
					int.MinValue,
					uint.MinValue,
					long.MinValue,
					ulong.MinValue,
					new IntPtr(int.MaxValue),
					new UIntPtr(uint.MaxValue),
					char.MinValue,
					double.MinValue,
					float.MinValue
				};
			}

			void GivenAPrimitive(object primitive) => SUT = primitive;
			void ThenItReturnsTrue() => Result.Should().BeTrue();
		}

		public class ForClassWithoutProperties : ObjectIsAtomic
		{
			void GivenAClassWithoutProperties() => SUT = new Class();
			void ThenItReturnsTrue() => Result.Should().BeTrue();
		}

		public class ForClassWithProperties : ObjectIsAtomic
		{
			void GivenAClassWithProperties() => SUT = new ClassWithProperty();
			void ThenItReturnsFalse() => Result.Should().BeFalse();
		}

		public class ForDerivedClassWithProperties : ObjectIsAtomic
		{
			void GivenAClassWithOnlyInheritedProperties() => SUT = new ClassWithInheritedProperty();
			void ThenItReturnsFalse() => Result.Should().BeFalse();
		}

		public class ForStructWithoutProperties : ObjectIsAtomic
		{
			void GivenAStructWithoutProperties() => SUT = new Struct();
			void ThenItReturnsTrue() => Result.Should().BeTrue();
		}

		public class ForStructWithProperties : ObjectIsAtomic
		{
			void GivenAStructWithProperties() => SUT = new StructWithProperty();
			void ThenItReturnsFalse() => Result.Should().BeFalse();
		}
	}

	public class Class { }

	public class ClassWithProperty
	{
		public int Property { get; set; }
	}

	public class ClassWithInheritedProperty : ClassWithProperty { }

	public struct Struct { }

	public struct StructWithProperty
	{
		public int Property { get; set; }
	}
}
