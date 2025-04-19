using NUnit.Framework;
using UnityEngine;
using WordSlide;

public class WordManager_Tests
{
	[Test]
	public void Test()
	{
		var wordManager = new WordManager(new DictionaryImporter());

		Debug.Log(wordManager.CheckWord("heello"));
	}
}
