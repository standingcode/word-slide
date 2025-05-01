using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace WordSlide
{
	/// <summary>
	/// Could potentially use later to add defintions etc for the word
	/// </summary>
	public struct Word
	{
		public string word;
	}

	public class PlayManager : MonoBehaviour
	{
		public static PlayManager Instance { get; private set; }

		private IDictionaryManager _dictionaryManager;

		[Inject]
		public async Task Construct(IDictionaryManager dictionaryManager)
		{
			if (Instance != null && Instance != this)
			{
				Debug.LogError("PlayManager instance already exists. Destroying the new instance.");
				Destroy(this);
				return;
			}

			_dictionaryManager = dictionaryManager;

			await _dictionaryManager.LoadDictionary("english");
			await _dictionaryManager.LoadCharacterSet("english");
		}

		public List<string> CheckStringForWords(string inputString)
		{
			var sw = System.Diagnostics.Stopwatch.StartNew();

			List<string> foundWords = new List<string>();
			List<int> forbiddenIndexes = new List<int>();

			// The entire string is a word
			if (_dictionaryManager.CheckWord(inputString))
			{
				foundWords.Add(inputString);
				return foundWords;
			}

			bool SlidingWindowContainsForbiddenIndexes(int startIndex, int endIndex)
			{
				for (int i = startIndex; i < startIndex + endIndex; i++)
				{
					//Debug.Log($"Checking string from {j} for length {i}");

					// Check if the index is forbidden
					if (forbiddenIndexes.Contains(i))
					{
						return true;
					}
				}
				return false;
			}

			for (int slidingWindowLength = inputString.Length - 1; slidingWindowLength >= Settings.Instance.MinimumWordLength; slidingWindowLength--)
			{
				// Each loop is a sliding window size

				Debug.Log($"Sliding window size....{slidingWindowLength}");

				// Slide the window
				for (int slidingWindowStartIndex = 0; slidingWindowStartIndex < inputString.Length - (slidingWindowLength - 1); slidingWindowStartIndex++)
				{
					if (SlidingWindowContainsForbiddenIndexes(slidingWindowStartIndex, (slidingWindowStartIndex + slidingWindowLength) - 1))
					{
						continue;
					}

					//// if the substring falls outside the bounds of the string, break loop
					//if (slidingWindowStartIndex + (slidingWindowLength - 1) > inputString.Length - 1)
					//{
					//	//Debug.Log($"Substring {j} to {j + (i - 1)} is out of bounds for string length {inputString.Length}");
					//	break;
					//}

					// Check if the substring is a word
					if (_dictionaryManager.CheckWord(inputString.Substring(slidingWindowStartIndex, slidingWindowLength)))
					{
						Debug.Log($"Found word {inputString.Substring(slidingWindowStartIndex, slidingWindowLength)} at {slidingWindowStartIndex} to {slidingWindowStartIndex + (slidingWindowLength - 1)}");
						foundWords.Add(inputString.Substring(slidingWindowStartIndex, slidingWindowLength));
						Enumerable.Range(slidingWindowStartIndex, (slidingWindowStartIndex + slidingWindowLength) - 1).ToList().ForEach(x => forbiddenIndexes.Add(x));
					}
				}
			}

			sw.Stop();
			Debug.Log(sw.ElapsedMilliseconds);


			return foundWords;
		}
	}
}