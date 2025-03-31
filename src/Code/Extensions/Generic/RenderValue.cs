using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Code.Extensions.Generic;

static partial class KeyValuePairExtension
{
	/// <summary>
	/// Parses the value of the given <paramref name="item"/> and replaces any tokens referencing keys in the given <paramref name="data"/>.
	/// Replacement tokens of the form "{KeyInData}" are supported, and can be escaped by doubling-up the braces. Tokens not matching any
	/// key in the given <paramref name="data"/> will be written out as they are. Rendering is recursive until no valid tokens are found,
	/// but cycles are detected and written out with a "#" prefix. Lookups are cached by writing back to <paramref name="data"/> values.
	/// </summary>
	/// <param name="item">The item from within <paramref name="data"/> to be rendered.</param>
	/// <param name="data">The data from which to resolve token references.</param>
	/// <param name="keyComparisonType">The <see cref="StringComparison"/> type for matching keys.</param>
	/// <returns>The final output from recursively resolving this <paramref name="item"/>'s value.</returns>
	internal static string? RenderValue(this KeyValuePair<string, string?> item, IDictionary<string, string?> data, StringComparison keyComparisonType = StringComparison.OrdinalIgnoreCase)
	{
		if (data == null) throw new ArgumentNullException(nameof(data));

		var path = ImmutableArray.Create(item.Key);
		var template = item.Value?.Parse(data, path, keyComparisonType);
		return template?.Render(data, path, keyComparisonType);
	}

	static DictionaryValueTemplate Parse(this string valueTemplate, IDictionary<string, string?> data, IReadOnlyCollection<string> path, StringComparison keyComparisonType) =>
		new(valueTemplate, valueTemplate.Tokenize(data, path, keyComparisonType));

	static IEnumerable<object> Tokenize(this string valueTemplate, IDictionary<string, string?> data, IReadOnlyCollection<string> path, StringComparison keyComparisonType)
	{
		if (valueTemplate.Length == 0)
		{
			yield return new TextToken("");
			yield break;
		}

		var validKeys = data.Keys.Except(path).ToArray();

		var nextIndex = 0;
		while (true)
		{
			var beforeText = nextIndex;
			var textToken = valueTemplate.ParseTextToken(nextIndex, out nextIndex);
			if (nextIndex > beforeText)
				yield return textToken;

			if (nextIndex == valueTemplate.Length)
				yield break;

			var beforeProp = nextIndex;
			var valueToken = valueTemplate.ParseValueToken(nextIndex, validKeys, path, keyComparisonType, out nextIndex);
			if (beforeProp < nextIndex)
				yield return valueToken;

			if (nextIndex == valueTemplate.Length)
				yield break;
		}
	}

	static object ParseValueToken(this string valueTemplate, int startAt, IReadOnlyCollection<string> validKeys, IEnumerable<string> path, StringComparison keyComparisonType, out int next)
	{
		var first = startAt;
		startAt++;
		while (startAt < valueTemplate.Length && valueTemplate[startAt] != '}')
			startAt++;

		if (startAt == valueTemplate.Length || valueTemplate[startAt] != '}')
		{
			next = startAt;
			return new TextToken(valueTemplate.Substring(first, next - first));
		}

		next = startAt + 1;

		var rawText = valueTemplate.Substring(first, next - first);
		var tagContent = rawText.Substring(1, next - (first + 2));
		if (tagContent.Length == 0 || !tagContent.IsValidKey(validKeys, keyComparisonType))
		{
			var key = path.SingleOrDefault(tagContent.StartsWith);
			if (key != null)
				rawText = ValueToken.Cycled(key);

			if (tagContent.Length > 2)
			{
				var unescapedTagContent = tagContent.Substring(1, tagContent.Length - 1);
				if (unescapedTagContent.Length != 0 && unescapedTagContent.IsValidKey(validKeys, keyComparisonType))
					return new TextToken(tagContent);
			}

			return new TextToken(rawText);
		}

		return new ValueToken(rawText);
	}

	static bool IsValidKey(this string text, IEnumerable<string> validKeys, StringComparison keyComparisonType) =>
		validKeys.Any(x => string.Compare(text, x, keyComparisonType) == 0);

	static TextToken ParseTextToken(this string valueTemplate, int startAt, out int next)
	{
		var text = new StringBuilder();
		do
		{
			var nextCharacter = valueTemplate[startAt];
			if (nextCharacter == '{')
				break;

			text.Append(nextCharacter);
			startAt++;
		} while (startAt < valueTemplate.Length);

		next = startAt;
		return new(text.ToString());
	}

	internal class DictionaryValueTemplate
	{
		internal DictionaryValueTemplate(string text, IEnumerable<object> tokens)
		{
			Text = text;
			TokenArray = tokens.ToArray();
		}

		internal string Text { get; }

		public override string ToString() => Text;

		internal object[] TokenArray { get; }

		internal string Render(IDictionary<string, string?> data, ImmutableArray<string> path, StringComparison keyComparisonType)
		{
			using var writer = new StringWriter(CultureInfo.InvariantCulture);
			Render(data, path, keyComparisonType, writer);
			return writer.ToString();
		}

		internal void Render(IDictionary<string, string?> data, ImmutableArray<string> path, StringComparison keyComparisonType, TextWriter output)
		{
			foreach (var token in TokenArray)
			{
				if (token is TextToken tt)
					RenderTextToken(tt, output);
				else
					RenderValueToken((ValueToken)token, output, data, path, keyComparisonType);
			}
		}

		static void RenderTextToken(TextToken textToken, TextWriter output) => output.Write(textToken.Text);

		static void RenderValueToken(ValueToken valueToken, TextWriter output, IDictionary<string, string?> data, ImmutableArray<string> path, StringComparison keyComparisonType)
		{
			var value = data[valueToken.Key];
			if (value != null)
			{
				var key = data.Keys.Single(x => string.Compare(valueToken.Key, x, keyComparisonType) == 0);
				path = path.Add(key);
				var template = value.Parse(data, path, keyComparisonType);
				value = template.Render(data, path, keyComparisonType);
#pragma warning disable CA1307 // Specify StringComparison
				if (data.Keys.Any(x => value.Contains(ValueToken.Cycled(x))))
#pragma warning restore CA1307 // Specify StringComparison
					value = ValueToken.Cycled(key);

				data[valueToken.Key] = value;
				output.Write(value);
			}
		}
	}

	internal sealed class TextToken
	{
		internal TextToken(string text) => Text = text;

		internal string Text { get; }

		public override string ToString() => Text;
	}

	internal sealed class ValueToken
	{
		internal ValueToken(string text) => Key = text.Substring(1, text.Length - 2);

		internal string Key { get; }

		internal static string Cycled(string key) => $"{{#{key}}}";

		public override string ToString() => Key;
	}
}