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

		[SerializeField]
		private TileSwappedEventHandler _tileSwappedEventHandler;

		[Inject]
		public async Task Construct(IDictionaryService dictionaryManager)
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
				return;
			}
			Instance = this;

			_dictionaryManager = dictionaryManager;

			await _dictionaryManager.LoadDictionary("english");
			await _dictionaryManager.LoadCharacterSet("english");

		}

		public void Start()
		{
			_tileSwappedEventHandler.AddTileSwappedListener(TileWasSwapped);
		}

		public void GeneratetileBoardUntilNoWordsPresent()
		{
			SingleTileManager[,] initialBoard;
			do
			{
				initialBoard = TilesManager.Instance.GenerateFullTileBoard();

			} while (initialBoard != null);
		}

		public void OnDestroy()
		{
			Instance = null;
		}

		public void TileWasSwapped(List<SingleTileManagersRepresentingAString> rowsAndColumnsToCheck)
		{
			var validWords = GetListOfValidWordsFromGivenRowsAndColumns(rowsAndColumnsToCheck);
			validWords.ForEach(x => Debug.Log($"{x.ToString()}"));
		}

	}
}