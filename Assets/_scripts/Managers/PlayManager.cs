using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
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

		public int TilesEnteringDestructSequenceCount { get; set; } = 0;

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
			_tileSwappedEventHandler.AddTileSwappedListener(CheckForWords);
			TilesManager.Instance.GenerateTileBoardAndRemoveAnyExistingValidWords(_wordFinderService, _dictionaryService);
			LoadingPanelRoot.SetActive(false);
			PlayerCanInteractWithTiles = true;

			InputSystem.pollingFrequency = 240.0f;
		}

		private void CheckForWords(List<SingleTileManagerSequence> rowsAndColumnsToCheck)
		{
			PlayerCanInteractWithTiles = false;

			if (rowsAndColumnsToCheck.Count == 0)
			{
				PlayerCanInteractWithTiles = true;
				return;
			}

			var validWords = _wordFinderService.GetListOfValidWordsFromGivenRowsAndOrColumns(_dictionaryService, rowsAndColumnsToCheck);
			validWords.ForEach(x => Debug.Log($"{x.ToString()}"));

			if (validWords.Count == 0)
			{
				PlayerCanInteractWithTiles = true;
				return;
			}

			List<SingleTileManager> tilesToDestroy = new();

			foreach (var word in validWords)
			{
				foreach (var singleTileManager in word.SingleTileManagers)
				{
					// TODO: Need to make sure that we still count this when working out points,
					// we just don't want to try and double destroy the same tile.
					if (tilesToDestroy.Contains(singleTileManager))
					{
						continue;
					}

					tilesToDestroy.Add(singleTileManager);
				}
			}

			TilesManager.Instance.DestroyTiles(tilesToDestroy);
		}

		public void OnDestroy()
		{
			_tileSwappedEventHandler.RemoveTileSwappedListener(CheckForWords);
			Instance = null;
		}
	}
}