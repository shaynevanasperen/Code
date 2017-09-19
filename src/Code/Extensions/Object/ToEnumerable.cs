using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Code.Extensions.Object
{
	static partial class ObjectExtension
	{
		internal static IEnumerable<object> ToEnumerable(this object source)
		{
			var objectEnumerable = source as IEnumerable<object>;
			if (objectEnumerable != null)
				return objectEnumerable;
			if (source is IEnumerable sourceEnumerable)
				objectEnumerable = sourceEnumerable.Cast<object>().ToArray();
			return objectEnumerable ?? source.GetType().GetRuntimeProperties().OrderBy(x => x.Name).Select(x => x.GetValue(source, null));
		}
	}
}
