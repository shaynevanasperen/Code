using System;
using System.Collections.Generic;
using System.Net;
using Code.AspNetCore.Routing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Specify;

namespace Code.Tests.AspNetCore.Routing;

public abstract class EnumConstraintMatching : ScenarioFor<EnumConstraintWrapper<HttpStatusCode>>
{
	protected string RouteKey = "RouteKey";
	protected RouteValueDictionary RouteValues = [];
	protected bool Result;

	public class ForInvalidEnumType : EnumConstraintMatching
	{
		Exception? _exception;

		void GivenAnInvalidEnumType() => _exception = Catch.Exception(() => new EnumConstraintWrapper<int>());
		void ThenItFailsToInstantiate() => _exception.Should().NotBeNull();
	}

	public abstract class ForValidEnumType : EnumConstraintMatching
	{
		protected void GivenAValidEnumType() => SUT = new();

		protected void WhenMatching() => Result = SUT.Match(Substitute.For<HttpContext>(), Substitute.For<IRouter>(), RouteKey, RouteValues, RouteDirection.IncomingRequest);
		protected void ThenNamesAreExposed() => SUT.Names.Should().BeEquivalentTo(Enum.GetNames<HttpStatusCode>());

		public class ForMissingRouteValue : ForValidEnumType
		{
			void AndGivenNoMatchingRouteValue() { }
			void AndThenItFailsToMatch() => Result.Should().BeFalse();
		}

		public class ForInvalidRouteValue : ForValidEnumType
		{
			void AndGivenAnInvalidMatchingRouteValue() => RouteValues.Add(RouteKey, "Invalid");
			void AndThenItFailsToMatch() => Result.Should().BeFalse();
		}

		public class ForValidRouteValue : ForValidEnumType
		{
			void AndGivenAnInvalidMatchingRouteValue() => RouteValues.Add(RouteKey, HttpStatusCode.Accepted);
			void AndThenItMatches() => Result.Should().BeTrue();
		}
	}
}

// Only so that we can make it public for testing, until such time that BDDfy can test internal classes
public class EnumConstraintWrapper<T> : IRouteConstraint where T : struct
{
	readonly EnumConstraint<T> _inner = new();

	public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection) => _inner.Match(httpContext, route, routeKey, values, routeDirection);

	public IEnumerable<string> Names => _inner.Names;
}