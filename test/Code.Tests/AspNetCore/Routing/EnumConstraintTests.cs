using System;
using Code.AspNetCore.Routing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Specify;

namespace Code.Tests.AspNetCore.Routing
{
	public abstract class EnumConstraintMatching : ScenarioFor<EnumConstraintWrapper>
	{
		protected string RouteKey = "RouteKey";
		protected RouteValueDictionary RouteValues = new RouteValueDictionary();
		protected bool Result;

		public class ForInvalidEnumType : EnumConstraintMatching
		{
			Exception _exception;

			void GivenAnInvalidEnumType() => _exception = Catch.Exception(() => SUT = new EnumConstraintWrapper("DayOfWeek"));
			void ThenItFailsToInstantiate() => _exception.Should().NotBeNull();
		}

		public abstract class ForValidEnumType : EnumConstraintMatching
		{
			protected void GivenAValidEnumType() => SUT = new EnumConstraintWrapper("System.DayOfWeek");
			protected void WhenMatching() => Result = SUT.Match(Substitute.For<HttpContext>(), Substitute.For<IRouter>(), RouteKey, RouteValues, RouteDirection.IncomingRequest);

			public class ForMissingRouteValue : ForValidEnumType
			{
				void AndGivenNoMatchingRouteValue() { }
				void ThenItFailsToMatch() => Result.Should().BeFalse();
			}

			public class ForInvalidRouteValue : ForValidEnumType
			{
				void AndGivenAnInvalidMatchingRouteValue() => RouteValues.Add(RouteKey, "Invalid");
				void ThenItFailsToMatch() => Result.Should().BeFalse();
			}

			public class ForValidRouteValue : ForValidEnumType
			{
				void AndGivenAnInvalidMatchingRouteValue() => RouteValues.Add(RouteKey, DayOfWeek.Monday);
				void ThenItMatches() => Result.Should().BeTrue();
			}
		}
	}

	// Only so that we can make it public for testing, until such time that BDDfy can test internal classes
	public class EnumConstraintWrapper : IRouteConstraint
	{
		readonly EnumConstraint _inner;

		public EnumConstraintWrapper(string enumType)
		{
			_inner = new EnumConstraint(enumType);
		}

		public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
		{
			return _inner.Match(httpContext, route, routeKey, values, routeDirection);
		}
	}
}
