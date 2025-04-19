using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WordSlide;

public class DictionaryImporter_Tests
{
	[Test]
	public async Task GetDictionary_ValidLanguage_ReturnsDictionary()
	{
		var dictionaryImporter = new DictionaryImporter();
		string language = "english";

		var result = await dictionaryImporter.GetDictionary(language);

		Debug.Log($"Imported dictionary contains {result.Count} entries");

		Assert.IsNotNull(result);
		Assert.IsInstanceOf<Dictionary<string, Word>>(result);
		Assert.IsTrue(result.Count > 0);
	}
}
