using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Code.Extensions.Generic;

static partial class DictionaryExtension
{
	/// <summary>
	/// Builds a hierarchical dictionary of the key/value pairs represented by the given <see cref="IDictionary{String,String}"/> instance.
	/// Uses the given <paramref name="keyDelimiter"/> for determining parent/child relationships among the dictionary entries. Uses the
	/// given <paramref name="keyComparer"/> for sorting of keys in order to distinguish between arrays and maps (if the sorted series of
	/// child keys under a given parent can be parsed as integers in ascending order beginning from zero, then the children represent an array).
	/// </summary>
	/// <param name="data">The <see cref="IDictionary{String,String}"/> instance from which to construct the dictionary.</param>
	/// <param name="keyDelimiter">The delimiter character to use for interpreting nesting in the keys.</param>
	/// <param name="keyComparer">The comparer to use for sorting keys in order to distinguish between arrays and maps.</param>
	/// <returns>A hierarchical dictionary of the key/value pairs in the given <see cref="IDictionary{String,String}"/> instance.</returns>
	internal static IDictionary<string, object?> ToNestedDictionary(this IDictionary<string, string?> data, char keyDelimiter, IComparer<string> keyComparer)
	{
		if (data == null) throw new ArgumentNullException(nameof(data));
		if (keyComparer == null) throw new ArgumentNullException(nameof(keyComparer));

		var result = new Dictionary<string, object?>(StringComparer.Ordinal);
		foreach (var item in data)
		{
			var segments = item.Key.Split(keyDelimiter);
			object? node = result;
			var path = string.Empty;
			for (var depth = 0; depth < segments.Length; depth++)
			{
				var segment = segments[depth];
				if (node is object?[] array)
				{
					var index = int.Parse(segment, CultureInfo.InvariantCulture);
					if (depth == segments.Length - 1)
						array[index] = item.Value;
					else
					{
						segment.AppendToPath(keyDelimiter, ref path);

						if (array[index] != null)
							node = array[index];
						else
							node = array[index] = data.GetChild(keyDelimiter, keyComparer, path, depth + 1);
					}
				}
				else if (node is Dictionary<string, object?> dictionary)
				{
					if (depth == segments.Length - 1)
						dictionary[segment] = item.Value;
					else
					{
						segment.AppendToPath(keyDelimiter, ref path);

						if (dictionary.TryGetValue(segment, out var child) && child != null)
							node = child;
						else
							node = dictionary[segment] = data.GetChild(keyDelimiter, keyComparer, path, depth + 1);
					}
				}
			}
		}

		return result;
	}

	static void AppendToPath(this string segment, char keyDelimiter, ref string path) =>
		path = path.Length == 0
			? segment
			: string.Join(keyDelimiter.ToString(CultureInfo.InvariantCulture), path, segment);

	static object GetChild(this IEnumerable<KeyValuePair<string, string?>> config, char keyDelimiter, IComparer<string> keyComparer, string path, int depth)
	{
		var prefix = depth > 0
			? path + keyDelimiter
			: path;

		var siblings = config.Where(x => x.Key.Length > prefix.Length && keyComparer.Compare(x.Key.Substring(0, prefix.Length), prefix) == 0)
			.Select(x => x.Key.Split(keyDelimiter)[depth])
			.Distinct()
			.OrderBy(x => x, keyComparer)
			.ToArray();

		return siblings.Where((sibling, i) => sibling != i.ToString(CultureInfo.InvariantCulture)).Any()
			? new Dictionary<string, object?>()
			: new object[siblings.Length];
	}
}