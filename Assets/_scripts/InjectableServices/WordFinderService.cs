using System.Collections.Generic;
using System.Linq;

namespace WordSlide
{
	public struct SingleTileManagerSequence
	{
		SingleTileManager[] singleTileManagers;

		public SingleTileManager[] SingleTileManagers => singleTileManagers;

		public SingleTileManagerSequence(SingleTileManager[] singleTileManagers)
		{
			this.singleTileManagers = singleTileManagers;
		}

		public override string ToString()
		{
			return string.Join("", singleTileManagers.Select(x => x.TileCharacter));
		}

		public int Length => singleTileManagers.Length;

		public SingleTileManagerSequence GetSubsetOfSingleTileManagers(int startIndex, int length)
		{
			return new SingleTileManagerSequence(singleTileManagers.Skip(startIndex).Take(length).ToArray());
		}
	}

	public class WordFinderService : IWordFinderService
	{
		public List<SingleTileManagerSequence> GetListOfValidWordsFromGivenRowsAndOrColumns(IDictionaryService _dictionaryService, List<SingleTileManagerSequence> rowsAndColumnsToCheck)
		{
			var foundWordsInAllRowsAndColumns = new List<SingleTileManagerSequence>();

			// For each of the rows/columns to check, check the string for words
			foreach (var rowOrColumn in rowsAndColumnsToCheck)
			{
				var foundWordsInThisRowOrColumn = CheckRowOrColumnForWords(_dictionaryService, rowOrColumn);

				if (foundWordsInThisRowOrColumn.Count > 0)
				{
					foundWordsInAllRowsAndColumns.AddRange(foundWordsInThisRowOrColumn);
				}
			}

			return foundWordsInAllRowsAndColumns;
		}

		private List<SingleTileManagerSequence> CheckRowOrColumnForWords(IDictionaryService _dictionaryService, SingleTileManagerSequence singleTileManagerStringToCheck)
		{
			string listOfSingleTileManagersToCheckAsString = singleTileManagerStringToCheck.ToString();

			// The entire string is a word
			if (_dictionaryService.CheckWord(listOfSingleTileManagersToCheckAsString))
			{
				return new List<SingleTileManagerSequence>() { singleTileManagerStringToCheck };
			}

			List<SingleTileManagerSequence> foundWords = new();
			List<int> forbiddenIndexes = new List<int>();
			string subString;

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
			for (int slidingWindowLength = singleTileManagerStringToCheck.Length - 1; slidingWindowLength >= SettingsScriptable.MinimumWordLength; slidingWindowLength--)
			{
				// Each iteration of the loop is a sliding window size, reducing from 1 below the length, down to minimum word length

				// For each sliding window, check the indexes, only starting from indexes which will keep the window within the string length
				for (int slidingWindowStartIndex = 0; slidingWindowStartIndex < singleTileManagerStringToCheck.Length - (slidingWindowLength - 1); slidingWindowStartIndex++)
				{
					if (SlidingWindowContainsForbiddenIndexes(slidingWindowStartIndex, slidingWindowStartIndex + slidingWindowLength - 1))
					{
						continue;
					}

					subString = listOfSingleTileManagersToCheckAsString.Substring(slidingWindowStartIndex, slidingWindowLength);

					// Check if the substring is a word
					if (_dictionaryService.CheckWord(subString))
					{
						// Add the correct word as a list of tile managers to the list of found words
						SingleTileManagerSequence singleTileManagerString = singleTileManagerStringToCheck.GetSubsetOfSingleTileManagers(slidingWindowStartIndex, slidingWindowLength);
						foundWords.Add(singleTileManagerString);

						// Add the range of forbidden indexes to the list
						Enumerable.Range(slidingWindowStartIndex, slidingWindowLength).ToList().ForEach(x => forbiddenIndexes.Add(x));
					}
				}
			}

			return foundWords;
		}
	}
}

