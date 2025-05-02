using System.Collections.Generic;
using System.Linq;
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

		[SerializeField]
		private TileSwappedEventHandler _tileSwappedEventHandler;

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

			_tileSwappedEventHandler.AddTileSwappedListener(TileWasSwapped);
		}

		public void TileWasSwapped(List<SingleTileManagersRepresentingAString> rowsAndColumnsToCheck)
		{
			var validWords = GetListOfValidWordsFromGivenRowsAndColumns(rowsAndColumnsToCheck);

			validWords.ForEach(x => Debug.Log($"{x.ToString()}"));
		}

		public List<SingleTileManagersRepresentingAString> GetListOfValidWordsFromGivenRowsAndColumns(List<SingleTileManagersRepresentingAString> rowsAndColumnsToCheck)
		{
			var foundWordsInAllRowsAndColumns = new List<SingleTileManagersRepresentingAString>();

			// For each of the rows/columns to check, check the string for words
			foreach (var rowOrColumn in rowsAndColumnsToCheck)
			{
				var foundWordsInThisRowOrColumn = CheckWordsInSingleTileManager(rowOrColumn);

				if (foundWordsInThisRowOrColumn.Count > 0)
				{
					foundWordsInAllRowsAndColumns.AddRange(foundWordsInThisRowOrColumn);
				}
			}

			return foundWordsInAllRowsAndColumns;
		}

		private List<SingleTileManagersRepresentingAString> CheckWordsInSingleTileManager(SingleTileManagersRepresentingAString tileManagerStringToCheck)
		{
			return WordFinderHelperMethods.CheckRowOrColumnForWords(_dictionaryManager, tileManagerStringToCheck);
		}
	}
}