using System.Collections.Generic;
using System.Linq;

namespace Code.Extensions.Object
{
	static partial class ObjectExtension
	{
		internal static IEnumerable<object> Flatten(this object source)
		{
			if (source.IsAtomic())
				yield return source;
			else
				foreach (var flattened in source.ToEnumerable().SelectMany(Flatten))
					yield return flattened;
		}
	}
}
