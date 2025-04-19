using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;
using WordSlide;

public class WordManager_Tests
{
	private WordManager sut;

	public async Task<WordManager> GetWordManagerWithDictionaryLoaded(string dictionaryToLoad)
	{
		sut = new WordManager(new DictionaryImporter());
		await sut.LoadDictionary(dictionaryToLoad);

		return sut;
	}

	[Test]
	public async Task CheckWord_WordDoesntExist_MethodReturnsFalse()
	{
		var sut = await GetWordManagerWithDictionaryLoaded("english");

		var result = sut.CheckWord("heello");

		Assert.IsFalse(result, "CheckWord should return false for a word that doesn't exist in the dictionary.");
	}

	[Test]
	public async Task CheckWord_WordDoesExist_MethodReturnsTrue()
	{
		var sut = await GetWordManagerWithDictionaryLoaded("english");

		var result = sut.CheckWord("hello");

		Assert.IsTrue(result, "CheckWord should return true for a word that does exist in the dictionary.");
	}

	[Test]
	public async Task CheckWord_EmptyString_MethodReturnsFalse()
	{
		var sut = await GetWordManagerWithDictionaryLoaded("english");

		var result = sut.CheckWord("");

		Assert.IsFalse(result, "CheckWord should return false for an empty string.");
	}

	[Test]
	public async Task CheckWord_ValidWord_TimeTakenIsAcceptable()
	{
		int acceptableMilliseconds = 2;

		var sut = await GetWordManagerWithDictionaryLoaded("english");
		var sw = System.Diagnostics.Stopwatch.StartNew();

		sut.CheckWord("hello");

		sw.Stop();

		Debug.Log($"Time taken to check word: {sw.ElapsedMilliseconds} ms");

		Assert.Less(sw.ElapsedMilliseconds, acceptableMilliseconds,
		$"CheckWord should take less than {acceptableMilliseconds} ms for a valid word.");
	}
}
