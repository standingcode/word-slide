using NUnit.Framework;
using UnityEngine;
using WordSlide;

public class WordManager_Tests
{
	[Test]
	public void CheckWord_WordDoesntExist_MethodReturnsFalse()
	{
		var sut = new WordManager(new DictionaryImporter());

		var result = sut.CheckWord("heello");

		Assert.IsFalse(result, "CheckWord should return false for a word that doesn't exist in the dictionary.");
	}

	[Test]
	public void CheckWord_WordDoesExist_MethodReturnsTrue()
	{
		var sut = new WordManager(new DictionaryImporter());

		var result = sut.CheckWord("hello");

		Assert.IsTrue(result, "CheckWord should return true for a word that does exist in the dictionary.");
	}

	[Test]
	public void CheckWord_EmptyString_MethodReturnsFalse()
	{
		var sut = new WordManager(new DictionaryImporter());
		var result = sut.CheckWord("");
		Assert.IsFalse(result, "CheckWord should return false for an empty string.");
	}

	[Test]
	public void CheckWord_ValidWord_TimeTakenIsAcceptable()
	{
		int acceptableMilliseconds = 2;

		var sut = new WordManager(new DictionaryImporter());
		var sw = System.Diagnostics.Stopwatch.StartNew();

		sut.CheckWord("hello");

		sw.Stop();

		Debug.Log($"Time taken to check word: {sw.ElapsedMilliseconds} ms");

		Assert.Less(sw.ElapsedMilliseconds, acceptableMilliseconds,
		$"CheckWord should take less than {acceptableMilliseconds} ms for a valid word.");
	}
}
