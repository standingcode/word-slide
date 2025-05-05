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

		public bool PlayerCanInteractWithTiles { get; private set; } = false;

		private IDictionaryService _dictionaryService;
		private IWordFinderService _wordFinderService;

		[SerializeField]
		private TileSwappedEventHandler _tileSwappedEventHandler;

		[SerializeField]
		GameObject LoadingPanelRoot;

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
			TilesManager.Instance.GenerateTileBoardAndRemoveAnyExistingValidWords(_wordFinderService, _dictionaryService);
			LoadingPanelRoot.SetActive(false);
			PlayerCanInteractWithTiles = true;
		}

		private void TileWasSwapped(List<SingleTileManagerSequence> rowsAndColumnsToCheck)
		{
			if (rowsAndColumnsToCheck.Count == 0)
			{
				//Debug.Log("No rows or columns to check for words");
				PlayerCanInteractWithTiles = true;
				return;
			}

			// TODO: We have to block user input here, and only enable it again after all animations, tile drops, check for new words etc. etc.
			PlayerCanInteractWithTiles = false;

			var validWords = _wordFinderService.GetListOfValidWordsFromGivenRowsAndOrColumns(_dictionaryService, rowsAndColumnsToCheck);
			validWords.ForEach(x => Debug.Log($"{x.ToString()}"));

			if (validWords.Count == 0)
			{
				//Debug.Log("No valid words found in the rows and columns checked");
				PlayerCanInteractWithTiles = true;
				return;
			}

			foreach (var word in validWords)
			{
				foreach (var singleTileManager in word.SingleTileManagers)
				{
					singleTileManager.StartDestroySequence();
				}
			}
		}

		public void OnDestroy()
		{
			_tileSwappedEventHandler.RemoveTileSwappedListener(TileWasSwapped);
			Instance = null;
		}
	}
}