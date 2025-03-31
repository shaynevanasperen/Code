using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Code.Extensions.Configuration;
using Code.Extensions.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Code.Tests.Generic;

public class ConvertingADictionaryToANestedDictionary : ScenarioFor<IDictionary<string, string?>>
{
	protected object SourceData = new
	{
		key1 = Guid.NewGuid().ToString(),
		key2 = new
		{
			key1 = Guid.NewGuid().ToString(),
			key2 = new
			{
				key1 = Guid.NewGuid().ToString(),
				key2 = Guid.NewGuid().ToString()
			}
		},
		key3 = new object[]
		{
			Guid.NewGuid().ToString(),
			Guid.NewGuid().ToString(),
			new
			{
				key1 = Guid.NewGuid().ToString(),
				key2 = Guid.NewGuid().ToString()
			}
		}
	};

	protected IDictionary<string, object?>? Result;

	void GivenADictionaryWithKeysThatIndicateANestedStructure()
	{
		var json = JsonConvert.SerializeObject(SourceData);
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
		SUT = new ConfigurationBuilder()
			.AddJsonStream(stream)
			.Build()
			.ToDictionary();
	}

	void WhenConvertingToANestedDictionary() => Result = SUT.ToNestedDictionary(ConfigurationPath.KeyDelimiter.Single(), ConfigurationKeyComparer.Instance);

	void ThenTheResultIsAsExpected()
	{
		var sourceDataAsJson = JsonConvert.SerializeObject(SourceData, Formatting.Indented);
		var resultAsJson = JsonConvert.SerializeObject(Result, Formatting.Indented);
		resultAsJson.Should().Be(sourceDataAsJson);
	}
}