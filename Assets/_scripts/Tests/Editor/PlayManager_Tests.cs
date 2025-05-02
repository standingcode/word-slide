using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using WordSlide;

public class PlayManager_Tests
{
	[Test]
	public async Task TestCheckListOfListOfSingleTileManagerForWords()
	{
		// Arrange
		Debug.unityLogger.logEnabled = true;

		// Create a new Monobehavior test object
		var gameObject = new GameObject();
		var playManager = gameObject.AddComponent<PlayManager>();

		// Create a mock of the Settings class
		var settings = gameObject.AddComponent<Settings>();
		settings.Awake();

		// Trigger the constructor to get a working dictionary 
		await playManager.Construct(new DictionaryManager(new DictionaryImporter()));

		List<SingleTileManagersRepresentingAString> InputRowsOrColumns = GetListOfListOfSingleTileManagersForListOfStrings(new List<string> { "exampleant", "antexample" });
		List<SingleTileManagersRepresentingAString> expectedWords = GetListOfListOfSingleTileManagersForListOfStrings(new List<string> { "example", "ant" });

		// Act
		var sw = System.Diagnostics.Stopwatch.StartNew();

		List<SingleTileManagersRepresentingAString> foundWords = playManager.CheckForWords(InputRowsOrColumns);

		sw.Stop();
		Debug.Log(sw.ElapsedMilliseconds);

		// Assert
		Assert.IsNotNull(foundWords);
		Assert.IsTrue(foundWords.Count > 0);
		Assert.IsTrue(AllListsOfSingleTileManagersContainTheSameStrings(foundWords, expectedWords));
	}

	private bool AllListsOfSingleTileManagersContainTheSameStrings(List<SingleTileManagersRepresentingAString> foundWords, List<SingleTileManagersRepresentingAString> expectedWords)
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

	private List<SingleTileManagersRepresentingAString> GetListOfListOfSingleTileManagersForListOfStrings(List<string> strings)
	{
		var listOfListOfSingleTileManagers = new List<SingleTileManagersRepresentingAString>();

		foreach (var str in strings)
		{
			var singleTileManagers = GenerateSingleTileManagersRepresentingAStringForString(str);
			listOfListOfSingleTileManagers.Add(singleTileManagers);
		}

		return listOfListOfSingleTileManagers;
	}

	private SingleTileManagersRepresentingAString GenerateSingleTileManagersRepresentingAStringForString(string inputString)
	{
		List<SingleTileManager> listOfSingleTileManagers = new List<SingleTileManager>();

		foreach (char c in inputString)
		{
			SingleTileManager singleTileManager = new GameObject().AddComponent<SingleTileManager>();
			singleTileManager.gameObject.AddComponent<TextMeshProUGUI>();
			singleTileManager.SetTileShownCharacter(c);
			listOfSingleTileManagers.Add(singleTileManager);
		}
		return new SingleTileManagersRepresentingAString(listOfSingleTileManagers.ToArray());
	}
}
