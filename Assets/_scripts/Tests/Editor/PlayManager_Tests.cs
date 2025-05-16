using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using WordSlide;

public class PlayManager_Tests
{

	[Test]
	public async Task TestGameStateSystem()
	{
		//GameState gameState = GameState.None;

		//gameState = GameState.Initializing | GameState.WaitingForPlayer | GameState.TileAnimationInProgress;

		//gameState = GameState.None;

		//Debug.Log($"GameState: {gameState}");

		//Assert.IsTrue(true);

	}

	// TODO: Fix or remove this test
	//[Test]
	//public async Task TestCheckListOfListOfSingleTileManagerForWords()
	//{
	//	// Arrange
	//	Debug.unityLogger.logEnabled = true;

	//	// Create a new Monobehavior test object
	//	var gameObject = new GameObject();
	//	var playManager = gameObject.AddComponent<PlayManager>();

	//	// Create a mock of the Settings class
	//	var settings = gameObject.AddComponent<Settings>();
	//	settings.Awake();

	//	// Trigger the constructor to get a working dictionary 
	//	await playManager.Construct(new DictionaryService(new DictionaryImporterService()), new WordFinderService());

	//	List<SingleTileManagersRepresentingAString> InputRowsOrColumns = GetListOfListOfSingleTileManagersForListOfStrings(new List<string> { "exampleant", "antexample" });
	//	List<SingleTileManagersRepresentingAString> expectedWords = GetListOfListOfSingleTileManagersForListOfStrings(new List<string> { "example", "ant" });

	//	// Act
	//	var sw = System.Diagnostics.Stopwatch.StartNew();

	//	//List<SingleTileManagersRepresentingAString> foundWords = playManager.GetListOfValidWordsFromGivenRowsAndColumns(InputRowsOrColumns);

	//	sw.Stop();
	//	Debug.Log(sw.ElapsedMilliseconds);

	//	// Assert
	//	Assert.IsNotNull(foundWords);
	//	Assert.IsTrue(foundWords.Count > 0);
	//	Assert.IsTrue(AllListsOfSingleTileManagersContainTheSameStrings(foundWords, expectedWords));
	//}

	private bool AllListsOfSingleTileManagersContainTheSameStrings(List<SingleTileManagerSequence> foundWords, List<SingleTileManagerSequence> expectedWords)
	{
		for (int i = 0; i < expectedWords.Count; i++)
		{
			var expectedString = expectedWords[i].ToString();
			var foundString = foundWords[i].ToString();

			Debug.Log($"Expected: {expectedString}, Found: {foundString}");

			// If the strings don't match
			if (!string.Equals(expectedString, foundString))
			{
				return false;
			}
		}

		return true;
	}

	private List<SingleTileManagerSequence> GetListOfListOfSingleTileManagersForListOfStrings(List<string> strings)
	{
		var listOfListOfSingleTileManagers = new List<SingleTileManagerSequence>();

		foreach (var str in strings)
		{
			var singleTileManagers = GenerateSingleTileManagersRepresentingAStringForString(str);
			listOfListOfSingleTileManagers.Add(singleTileManagers);
		}

		return listOfListOfSingleTileManagers;
	}

	private SingleTileManagerSequence GenerateSingleTileManagersRepresentingAStringForString(string inputString)
	{
		List<SingleTileManager> listOfSingleTileManagers = new List<SingleTileManager>();

		foreach (char c in inputString)
		{
			SingleTileManager singleTileManager = new GameObject().AddComponent<SingleTileManager>();
			singleTileManager.gameObject.AddComponent<TextMeshProUGUI>();
			singleTileManager.SetTileCharacter(c);
			listOfSingleTileManagers.Add(singleTileManager);
		}
		return new SingleTileManagerSequence(listOfSingleTileManagers.ToArray());
	}
}
