using System.Collections.Generic;
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

		private IDictionaryService _dictionaryManager;
		private IWordFinderService _wordFinderService;

		[SerializeField]
		private TileSwappedEventHandler _tileSwappedEventHandler;

		[Inject]
		public async Task Construct(IDictionaryService dictionaryManager, IWordFinderService wordFinderService)
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
				return;
			}
			Instance = this;

			_dictionaryManager = dictionaryManager;
			_wordFinderService = wordFinderService;

			await _dictionaryManager.LoadDictionary("english");
			await _dictionaryManager.LoadCharacterSet("english");
		}

		public void Start()
		{
			_tileSwappedEventHandler.AddTileSwappedListener(TileWasSwapped);
		}

		public void GeneratetileBoardUntilNoWordsPresent()
		{
			SingleTileManager[,] board;
			int foundWords = 0;

			do
			{
				board = TilesManager.Instance.GenerateFullTileBoard();

				// Get the rows and columns to check for words
				// TODO: We now need to get the entire board as list of SingleTileManagersRepresentingAString
				// We probably need to add a new method to the TilesManager to get the rows and columns as a list of SingleTileManagersRepresentingAString


			} while (foundWords > 0);
		}

		public void OnDestroy()
		{
			Instance = null;
		}

		public void TileWasSwapped(List<SingleTileManagersRepresentingAString> rowsAndColumnsToCheck)
		{
			var validWords = _wordFinderService.GetListOfValidWordsFromGivenRowsAndOrColumns(_dictionaryManager, rowsAndColumnsToCheck);
			validWords.ForEach(x => Debug.Log($"{x.ToString()}"));
		}

	}
}