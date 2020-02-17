using System;
using System.Collections.Generic;

namespace Code.Tests.Generic
{
	public abstract class RenderingTests : ScenarioFor<object>
	{
		protected IDictionary<string, string> Input = new Dictionary<string, string>
		{
			{"key1", "value1"},
			{"key2", "{key1}"},
			{"key3", "{key2}"},
			{"key4", "{key1}.{key4}"},
			{"key5", "{key4}"},
			{"key6", "{key1}.{key5}"},
			{"key7", "{key6}.{{key5}}"},
			{"key8", ""},
			{"key9", "{key1"},
			{"key10", "{key11}"},
			{"key11", "{key10}"},
			{"key:12", "{{foo}}.{{{bar}}}"}
		};
		protected KeyValuePair<string, string>[] Rendered;
		protected IDictionary<string, string> Expected = new Dictionary<string, string>
		{
			{ "key1", "value1" },
			{ "key2", "value1" },
			{ "key3", "value1" },
			{ "key4", "value1.{#key4}" },
			{ "key5", "{#key4}"},
			{ "key6", "value1.{#key5}" },
			{ "key7", "{#key6}.{key5}" },
			{ "key8", "" },
			{ "key9", "{key1" },
			{ "key10", "{#key11}" },
			{ "key11", "{#key10}" },
			{ "key:12", "{{foo}}.{{{bar}}}" }
		};

		protected void PrintInputData(IEnumerable<KeyValuePair<string, string>> data) => PrintData(data, "INPUT");
		protected void PrintExpectedData(IEnumerable<KeyValuePair<string, string>> data) => PrintData(data, "EXPECTED");
		protected void PrintRenderedData(IEnumerable<KeyValuePair<string, string>> data) => PrintData(data, "RENDERED");

		private void PrintData(IEnumerable<KeyValuePair<string, string>> data, string heading)
		{
			Console.WriteLine($"### {heading} DATA ###");
			foreach (var item in data)
				Console.WriteLine($"{item.Key}:{item.Value}");
		}
	}
}