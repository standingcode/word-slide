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
		}

		private void TileWasSwapped(List<SingleTileManagerSequence> rowsAndColumnsToCheck)
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