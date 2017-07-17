using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyModel;

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
				foreach (var runtimeLibrary in DependencyContext.Default.RuntimeLibraries)
				{
					try
					{
						var assembly = Assembly.Load(new AssemblyName(runtimeLibrary.Name));
						var type = assembly.GetType(key);
						if (type != null)
							return Enum.GetNames(type);
					}
					catch
					{
						// ignored
					}
				}

				throw new InvalidOperationException($"'{enumType}' is not a valid enum type. Please use a fully qualified type name.");
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
