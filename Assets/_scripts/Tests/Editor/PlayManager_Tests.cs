using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using WordSlide;

public class PlayManager_Tests
{
	[Test]
	public async Task TestCheckStringForWords()
	{
		// Arrange, create a new Monobehavior test object
		var gameObject = new GameObject();
		var playManager = gameObject.AddComponent<PlayManager>();

		// Create a mock of the Settings class
		var settings = gameObject.AddComponent<Settings>();
		settings.Awake();

		await playManager.Construct(new DictionaryManager(new DictionaryImporter()));

		string inputString = "antexample";
		List<string> expectedWords = new List<string> { "example", "ant" }; // Adjust based on your dictionary


		// Act
		List<string> foundWords = playManager.CheckStringForWords(inputString);


		// Assert
		Assert.IsNotNull(foundWords);
		Assert.IsTrue(foundWords.Count > 0);
		Assert.IsTrue(expectedWords.All(word => foundWords.Contains(word)));
	}
}
