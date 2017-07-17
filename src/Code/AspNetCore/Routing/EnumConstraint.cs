using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Code.AspNetCore.Routing
{
	class EnumConstraint : IRouteConstraint
	{
		static readonly ConcurrentDictionary<string, string[]> Cache = new ConcurrentDictionary<string, string[]>();
		readonly string[] _validOptions;
		public IEnumerable<string> Names => _validOptions;

		public EnumConstraint(string enumType)
		{
			_validOptions = Cache.GetOrAdd(enumType, key =>
			{
				var type = Type.GetType(key);
				if (type == null)
					throw new InvalidOperationException($"'{enumType}' is not a valid enum type. Please use a fully qualified type name.");
				return Enum.GetNames(type);
			});
		}

		public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
		{
			if (values.TryGetValue(routeKey, out object value) && value != null)
			{
				return _validOptions.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase);
			}
			return false;
		}
	}
}
