using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WordSlide
{
	public static class WordFinder
	{
		public static List<string> CheckStringForWords(IDictionaryManager _dictionaryManager, string stringToCheck)
		{
			List<string> foundWords = new List<string>();
			List<int> forbiddenIndexes = new List<int>();
			string subString;

			// The entire string is a word
			if (_dictionaryManager.CheckWord(stringToCheck))
			{
				foundWords.Add(stringToCheck);
				return foundWords;
			}

			// Define local function to check if a sliding window contains forbidden indexes
			bool SlidingWindowContainsForbiddenIndexes(int startIndex, int endIndex)
			{
				for (int i = startIndex; i <= endIndex; i++)
				{
					// Check if the index is forbidden
					if (forbiddenIndexes.Contains(i))
					{
						return true;
					}
				}
				return false;
			}

			// Main loop starts with stringLength -1 size of sliding window, down to minimum word length
			for (int slidingWindowLength = stringToCheck.Length - 1; slidingWindowLength >= Settings.Instance.MinimumWordLength; slidingWindowLength--)
			{
				// Each iteration of the loop is a sliding window size, reducing from 1 below the length, down to minimum word length

				// For each sliding window, check the indexes, only starting from indexes which will keep the window within the string length
				for (int slidingWindowStartIndex = 0; slidingWindowStartIndex < stringToCheck.Length - (slidingWindowLength - 1); slidingWindowStartIndex++)
				{
					if (SlidingWindowContainsForbiddenIndexes(slidingWindowStartIndex, slidingWindowStartIndex + slidingWindowLength - 1))
					{
						continue;
					}

					subString = stringToCheck.Substring(slidingWindowStartIndex, slidingWindowLength);

					// Check if the substring is a word
					if (_dictionaryManager.CheckWord(subString))
					{
						// TODO: remove this debug
						//Debug.Log($"Found word {stringToCheck.Substring(slidingWindowStartIndex, slidingWindowLength)} " +
						//$"at {slidingWindowStartIndex} to {slidingWindowStartIndex + (slidingWindowLength - 1)}");

						// Add the correct word to the list of found words
						foundWords.Add(subString);

						// Add the range of forbidden indexes to the list
						Enumerable.Range(slidingWindowStartIndex, slidingWindowLength).ToList().ForEach(x => forbiddenIndexes.Add(x));
					}
				}
			}

			return foundWords;
		}
	}
}

