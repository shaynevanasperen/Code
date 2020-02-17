using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Code.Extensions.Generic;
using Microsoft.Extensions.Configuration;

namespace Code.Tests.Generic
{
	public abstract class SubstitutingAbsentEnvironmentVariablesForCurrentProcessFromADictionary : RenderingTests
	{
		void GivenADictionaryWithTokensInTheValues()
		{
			var environmentData = new Dictionary<string, string>
			{
				{ "key1", "value2" },
				{ "key8", "value3" },
			};

			var items = new KeyValuePair<string, string>[Expected.Count];
			Expected.CopyTo(items, 0);
			var expectedData = items.ToDictionary(x => x.Key, x => x.Value);

			foreach (var item in environmentData)
				Environment.SetEnvironmentVariable(item.Key, item.Value);

			foreach (var item in expectedData.ToArray())
				expectedData[item.Key] = item.Value.Replace("value1", "value2", StringComparison.Ordinal);

			expectedData["key8"] = environmentData["key8"];
			Expected = expectedData;
		}

		async Task WhenSubstitutingAbsentEnvironmentVariablesForCurrentProcess()
		{
			PrintInputData(Input);

			var rendered = new List<KeyValuePair<string, string>>();
			await Task
				.Run(() =>
				{
					Input.SubstituteAbsentEnvironmentVariablesForCurrentProcess();
					foreach (var item in Input)
						rendered.Add(new KeyValuePair<string, string>(item.Key, Environment.GetEnvironmentVariable(item.Key.Replace(ConfigurationPath.KeyDelimiter, "__"))));
				})
				.TimeoutAfterSeconds(2);
			Rendered = rendered.ToArray();

			PrintExpectedData(Expected);
			PrintRenderedData(Rendered);
		}

		void ThenTheValuesAreRenderedAsExpectedAndAddedOnlyForThoseWhichAreNotAlreadyThere() => Rendered.Should().Equal(Expected);
	}
}
