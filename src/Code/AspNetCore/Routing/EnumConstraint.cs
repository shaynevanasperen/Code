using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Code.AspNetCore.Routing
{
	class EnumConstraint<T> : IRouteConstraint where T : struct 
	{
		// ReSharper disable once StaticMemberInGenericType
		static readonly ConcurrentDictionary<Type, string[]> Cache = new ConcurrentDictionary<Type, string[]>();
		readonly string[] _validOptions;
		public IEnumerable<string> Names => _validOptions;

		public EnumConstraint()
		{
			var enumType = typeof(T);
			if (!enumType.IsEnum)
				throw new InvalidOperationException($"'{enumType.Name}' is not a valid enum type.");
			
			_validOptions = Cache.GetOrAdd(enumType, key => Enum.GetNames(enumType));
		}

		public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
		{
			if (values.TryGetValue(routeKey, out var value) && value != null)
			{
				return _validOptions.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase);
			}
			return false;
		}
	}
}
