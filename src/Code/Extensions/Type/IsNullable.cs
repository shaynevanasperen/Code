using System;
using System.Reflection;

namespace Code.Extensions.Type
{
	static partial class TypeExtension
	{
		internal static bool IsNullable(this System.Type type)
		{
			return type.GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(typeof(Nullable<>).GetTypeInfo());
		}
	}
}
