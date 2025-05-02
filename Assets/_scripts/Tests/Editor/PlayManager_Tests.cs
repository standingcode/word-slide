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
		// Arrange
		// Create a new Monobehavior test object
		var gameObject = new GameObject();
		var playManager = gameObject.AddComponent<PlayManager>();

		// Create a mock of the Settings class
		var settings = gameObject.AddComponent<Settings>();
		settings.Awake();

		// Trigger the constructor to get a working dictionary 
		await playManager.Construct(new DictionaryManager(new DictionaryImporter()));

		string inputString = "antexample";
		List<string> expectedWords = new List<string> { "example", "ant" }; // Adjust based on your dictionary

		// Act
		var sw = System.Diagnostics.Stopwatch.StartNew();

		List<string> foundWords = playManager.CheckWordString(inputString);

		sw.Stop();
		Debug.Log(sw.ElapsedMilliseconds);


		// Assert
		Assert.IsNotNull(foundWords);
		Assert.IsTrue(foundWords.Count > 0);
		Assert.IsTrue(expectedWords.All(word => foundWords.Contains(word)));
	}
}
