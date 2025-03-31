using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.Extensions.Generic;

static partial class DictionaryExtension
{
	/// <summary>
	/// Essentially a loop around Code.Extensions.Generic.RenderValue, but with a copy to array to avoid "list modified while enumerating"
	/// errors. An easy way to lazily render the entire dictionary.
	/// </summary>
	/// <param name="data">The data from which to resolve token references.</param>
	/// <returns>A lazily evaluated enumeration of the key/value pairs, with each rendered value being the final output from
	/// recursively resolving.</returns>
	internal static IEnumerable<KeyValuePair<string, string?>> AsRenderedEnumerable(this IDictionary<string, string?> data)
	{
		if (data == null) throw new ArgumentNullException(nameof(data));

		return data.ToArray().Select(keyValuePair => new KeyValuePair<string, string?>(keyValuePair.Key, keyValuePair.RenderValue(data)));
	}
}