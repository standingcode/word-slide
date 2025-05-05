using Moq;
using NUnit.Framework;
using UnityEngine;
using WordSlide;

public class TilesManager_Tests
{
	[Test]
	public void GenerateTileBoardAndRemoveAnyExistingValidWords_ValidInput_CreatesTiles()
	{
		// Arrange
		var gameObject = new GameObject();
		var tilesManager = gameObject.AddComponent<TilesManager>();

		var wordFinderService = new WordFinderService();
		var dictionaryService = new DictionaryService(new DictionaryImporterService());


		tilesManager.GenerateTileBoardAndRemoveAnyExistingValidWords(wordFinderService, dictionaryService);
	}
}
