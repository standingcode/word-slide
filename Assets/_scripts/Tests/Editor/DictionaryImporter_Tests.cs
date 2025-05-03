using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using WordSlide;

public class DictionaryImporter_Tests
{
	[Test]
	public async Task GetDictionary_ValidLanguage_ReturnsDictionary()
	{
		var dictionaryImporter = new DictionaryImporterService();
		string language = "english";

		var result = await dictionaryImporter.GetDictionary(language);

		Debug.Log($"Imported dictionary contains {result.Count} entries");

		Assert.IsNotNull(result);
		Assert.IsInstanceOf<Dictionary<string, Word>>(result);
		Assert.IsTrue(result.Count > 0);
	}

	[Test]
	public async Task GetCharacterSetForDictionary_ValidLanguage_ReturnsCharacterSet()
	{
		var dictionaryImporter = new DictionaryImporterService();
		string language = "english";

		var result = await dictionaryImporter.GetCharacterSetForDictionary(language);

		Debug.Log($"Imported character set contains {result.Count} characters");
		Assert.IsNotNull(result);
		Assert.IsInstanceOf<List<char>>(result);
		Assert.IsTrue(result.Count > 0);
	}

	/// <summary>
	/// This test actually determines the weight of each character in a given dictionary.
	/// Usually do not want to run this
	/// </summary>
	/// <returns></returns>
	//[Test]
	public async Task GetAppearanceNumbersForCharacters()
	{
		var dictionaryImporter = new DictionaryImporterService();
		string language = "english";

		var dictionary = await dictionaryImporter.GetDictionary(language);

		Dictionary<char, int> charCounts = new();
		Dictionary<char, int> charWeights = new();

		var sw = System.Diagnostics.Stopwatch.StartNew();

		foreach (var kvp in dictionary)
		{
			foreach (char character in kvp.Value.word)
			{
				if (!charCounts.ContainsKey(character))
				{
					charCounts[character] = 0;
				}

				charCounts[character] = charCounts[character] + 1;
			}
		}

		sw.Stop();
		Debug.Log($"Time taken to count character appearance: {sw.ElapsedMilliseconds} ms");

		var lowestAppearance = charCounts.Min(kvp => kvp.Value);

		foreach (var kvp in charCounts)
		{
			charWeights[kvp.Key] = kvp.Value / lowestAppearance;
		}

		// Write the character weights to a file
		string filePath = "Assets/Resources/CharacterSets/english.txt";

		using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
		{
			foreach (var kvp in charWeights)
			{
				Debug.Log($"Character '{kvp.Key}' weight is {kvp.Value}.");
				file.WriteLine($"{kvp.Key} {kvp.Value}");
			}
		}
	}
}
