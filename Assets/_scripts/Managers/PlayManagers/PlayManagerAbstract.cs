using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace WordSlide
{
	/// <summary>
	/// Could potentially use later to add defintions etc for the word
	/// </summary>
	public struct Word
	{
		public string word;
	}

	public abstract class PlayManagerAbstract : MonoBehaviour
	{
		public static PlayManagerAbstract Instance { get; private set; }

		[SerializeField]
		protected bool playerCanInteractWithTiles = false;

		protected IDictionaryService _dictionaryService;
		protected IWordFinderService _wordFinderService;

		[SerializeField]
		protected GameStateEventHandler _gameStateEventHandler;

		public virtual void Initialize(IDictionaryService dictionaryService, IWordFinderService wordFinderService, GameStateEventHandler gameStateEventHandler)
		{
			_dictionaryService = dictionaryService;
			_wordFinderService = wordFinderService;
			_gameStateEventHandler = gameStateEventHandler;

			ConfigureRendering();
			ConfigureInputHandling();
			InitializeEventSubscriptions();
			TriggerNewGame();
		}

		/// <summary>
		/// Any intialization relating to rendering should be done here.
		/// </summary>
		protected void ConfigureRendering()
		{
			Application.targetFrameRate = 120;
		}

		/// <summary>
		/// Any intialization relating to input handling should be done here.
		/// </summary>
		protected void ConfigureInputHandling()
		{
			InputSystem.pollingFrequency = 120;
		}

		/// <summary>
		/// All event subscriptions should be done here.
		/// </summary>
		protected virtual void InitializeEventSubscriptions()
		{
			_gameStateEventHandler.AddTileSwappedListener(TilesSwappedByUser);
			_gameStateEventHandler.AddNewBoardGeneratedListener(BoardGenerated);
			_gameStateEventHandler.AddPlayerCanInteractWithTilesChangedListener(PlayerCanInteractWithTiles);
		}

		/// <summary>
		/// All removal of event subscriptions should be done here.
		/// </summary>
		protected virtual void RemoveEventSubscriptions()
		{
			_gameStateEventHandler.RemoveTileSwappedListener(TilesSwappedByUser);
			_gameStateEventHandler.RemoveNewBoardGeneratedListener(BoardGenerated);
			_gameStateEventHandler.RemovePlayerCanInteractWithTilesChangedListener(PlayerCanInteractWithTiles);
		}

		/// <summary>
		/// Updated every time the bool for can player interact is changed
		/// </summary>
		/// <param name="value"></param>
		private void PlayerCanInteractWithTiles(bool value)
		{
			playerCanInteractWithTiles = value;
		}

		/// <summary>
		/// Triggers a new game by raising the event
		/// </summary>
		protected void TriggerNewGame()
		{
			// This should trigger the TilesManagerSomeGameMode to generate a new board
			_gameStateEventHandler.RaiseNewGameStarted();
		}

		/// <summary>
		/// A board has been generated
		/// </summary>
		/// <param name="generatedBoard"></param>
		protected void BoardGenerated(List<SingleTileManagerSequence> generatedBoard)
		{
			var foundWords = FindWords(generatedBoard);

			// Board contains no words, we are ready to start
			if (foundWords.Count == 0)
			{
				// If there are no words, we can start the game
				LoadingComplete();
				return;
			}
			else
			{
				// We need to swap the tiles which contain words and try again
				TilesManager.Instance.ChangeCharactersForTiles(foundWords);
			}
		}

		/// <summary>
		/// Loading is complete, usually this means a board is generated without current valid words
		/// </summary>
		protected void LoadingComplete()
		{
			InGameUIController.Instance.HideLoadingCanvas();
			_gameStateEventHandler.RaisePlayerCanInteractWithTilesChanged(true);
		}

		/// <summary>
		/// Find any words in the given rows and columns to check
		/// </summary>
		/// <param name="rowsAndColumnsToCheck"></param>
		/// <returns>A list of SingleTileManagerSequences which is a list of valid words in SingleTileManagerSequence form</returns>
		protected List<SingleTileManagerSequence> FindWords(List<SingleTileManagerSequence> rowsAndColumnsToCheck)
		{
			return _wordFinderService.GetListOfValidWordsFromGivenRowsAndOrColumns(_dictionaryService, rowsAndColumnsToCheck);
		}

		public virtual void OnDestroy()
		{
			RemoveEventSubscriptions();
			Instance = null;
		}

		/// <summary>
		/// Abstract method which needs to be implemented by all game modes
		/// </summary>
		/// <param name="rowsAndColumnsToCheck"></param>
		protected abstract void TilesSwappedByUser(List<SingleTileManagerSequence> rowsAndColumnsToCheck);
	}
}