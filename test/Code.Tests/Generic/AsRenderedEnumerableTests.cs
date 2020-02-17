using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Code.Extensions.Generic;
using FluentAssertions;

namespace Code.Tests.Generic
{
	public abstract class RenderingADictionary : RenderingTests
	{
		void GivenADictionaryWithTokensInTheValues() { }

		async Task WhenProjectingToRenderedEnumerable()
		{
			PrintInputData(Input);

			Rendered = Array.Empty<KeyValuePair<string, string>>();
			await Task
				.Run(() => Rendered = Input.AsRenderedEnumerable().ToArray())
				.TimeoutAfterSeconds(2);

			PrintExpectedData(Expected);
			PrintRenderedData(Rendered);
		}

		void ThenTheValuesAreRenderedAsExpected() => Rendered.Should().Equal(Expected);
	}
}
