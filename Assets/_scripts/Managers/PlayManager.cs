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

		private IDictionaryService _dictionaryService;
		private IWordFinderService _wordFinderService;

		[SerializeField]
		private TileSwappedEventHandler _tileSwappedEventHandler;

		[Inject]
		public async Task Construct(IDictionaryService dictionaryService, IWordFinderService wordFinderService)
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
				return;
			}
			Instance = this;

			_dictionaryService = dictionaryService;
			_wordFinderService = wordFinderService;

			await _dictionaryService.LoadDictionary("english");
			await _dictionaryService.LoadCharacterSet("english");
		}

		public void Start()
		{
			_tileSwappedEventHandler.AddTileSwappedListener(TileWasSwapped);
			GenerateTileBoardUntilNoWordsPresent();
		}

		public void GenerateTileBoardUntilNoWordsPresent()
		{
			var sw = System.Diagnostics.Stopwatch.StartNew();

			List<SingleTileManagerSequence> foundWords = new();
			TilesManager.Instance.GenerateFullTileBoard();

			do
			{
				List<SingleTileManagerSequence> rowsAndColumnsToCheck = new();

				// Get all rows
				for (int i = 0; i < TilesManager.Instance.BoardTiles.GetLength(0); i++)
				{
					rowsAndColumnsToCheck.Add(new SingleTileManagerSequence(TilesManager.Instance.GetFullRow(i)));
				}

				// Get all columns
				for (int i = 0; i < TilesManager.Instance.BoardTiles.GetLength(1); i++)
				{
					rowsAndColumnsToCheck.Add(new SingleTileManagerSequence(TilesManager.Instance.GetFullColumn(i)));
				}

				foundWords = _wordFinderService.GetListOfValidWordsFromGivenRowsAndOrColumns(_dictionaryService, rowsAndColumnsToCheck);

				if (foundWords.Count > 0)
				{
					Debug.Log($"Found words: {foundWords.Count}, attempting to remove them.....");

					foreach (var singleTileManagerSequence in foundWords)
					{
						TilesManager.Instance.ChangeCharactersForTiles(singleTileManagerSequence.SingleTileManagers);
					}
				}

			} while (foundWords.Count > 0);

			sw.Stop();
			Debug.Log($"Generating the board witout words took {sw.ElapsedMilliseconds} milliseconds.");

			TilesManager.Instance.ShowBoard();
		}

		public void TileWasSwapped(List<SingleTileManagerSequence> rowsAndColumnsToCheck)
		{
			var validWords = _wordFinderService.GetListOfValidWordsFromGivenRowsAndOrColumns(_dictionaryService, rowsAndColumnsToCheck);
			validWords.ForEach(x => Debug.Log($"{x.ToString()}"));
		}

		public void OnDestroy()
		{
			Instance = null;
		}
	}
}