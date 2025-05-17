using System;
using System.Collections.Generic;
using System.Linq;
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

		protected IDictionaryService _dictionaryService;
		protected IWordFinderService _wordFinderService;

		protected GameStateEventHandler _gameStateEventHandler;
		protected TileEventHandler _tileEventHandler;
		protected ClickEventHandler _clickEventHandler;
		protected SingleTileManager[,] boardTiles => TilesManager.Instance.BoardTiles;


		// VIRTUAL
		public virtual void Initialize(
			IDictionaryService dictionaryService,
			IWordFinderService wordFinderService,
			GameStateEventHandler gameStateEventHandler,
			TileEventHandler tileEventHandler,
			ClickEventHandler clickEventHandler)
		{
			_dictionaryService = dictionaryService;
			_wordFinderService = wordFinderService;

			_gameStateEventHandler = gameStateEventHandler;
			_tileEventHandler = tileEventHandler;
			_clickEventHandler = clickEventHandler;

			ConfigureRendering();
			ConfigureInputHandling();
			SubscribeToEvents();
			TriggerNewGame();
		}

		public virtual void OnDestroy()
		{
			RemoveEventSubscriptions();
		}

		/// <summary>
		/// All event subscriptions should be done here.
		/// </summary>
		protected virtual void SubscribeToEvents()
		{
			_tileEventHandler.AddAllTilesFinishedAnimatingListener(AllTileAnimationsCompleted);
			_tileEventHandler.AddNewBoardGeneratedListener(BoardGenerated);

			_clickEventHandler.AddClickDownListener(ClickDown);
			_clickEventHandler.AddClickUpListener(ClickUp);
		}

		/// <summary>
		/// All removal of event subscriptions should be done here.
		/// </summary>
		protected virtual void RemoveEventSubscriptions()
		{
			_tileEventHandler.RemoveAllTilesFinishedAnimatingListener(AllTileAnimationsCompleted);
			_tileEventHandler.RemoveNewBoardGeneratedListener(BoardGenerated);

			_clickEventHandler.RemoveClickDownListener(ClickDown);
			_clickEventHandler.RemoveClickUpListener(ClickUp);
		}

		// ABSTRACT		

		/// <summary>
		/// Method is called by an event triggerd by TilesManager when all tiles completed animations
		/// </summary>
		/// <param name="singleTileManager"></param>
		public abstract void AllTileAnimationsCompleted(HashSet<SingleTileManager> singleTileManagers);

		/// <summary>
		/// Check words (or the outcome at least) is different for different game modes
		/// </summary>		
		protected abstract void CheckWords(HashSet<SingleTileManager> singleTileManagers);

		/// <summary>
		/// Raise destroy for tiles when a word is found
		/// </summary>
		/// <param name="tileSequencesToDestroy"></param>
		protected abstract void RaiseDestroy(HashSet<SingleTileManagerSequence> tileSequencesToDestroy);


		// NO OVERRIDE

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
		/// Method trigger upon pointer click up
		/// </summary>
		protected void ClickUp()
		{
			_tileEventHandler.RaiseCheckIfTileWasClickedOff();
		}

		/// <summary>
		/// Method triggered upon pointer click down
		/// </summary>
		/// <param name="mousePosition"></param>
		protected void ClickDown(Vector2 mousePosition)
		{
			CheckIfTileWasClicked(mousePosition);
		}

		/// <summary>
		/// When mouse is clicked, we fire a ray and see if it hits a tile's collider, we then know if the tile is selected.
		/// </summary>
		/// <param name="mousePosition"></param>
		protected void CheckIfTileWasClicked(Vector2 mousePosition)
		{
			//If the player is not allowed to interact, return
			if (GameStateEventHandler.GameState != GameState.WaitingForPlayer)
			{
				return;
			}

			// Shoot ray from main camera and detect what it hits
			Ray ray = Camera.main.ScreenPointToRay(mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				if (hit.collider.TryGetComponent(out SingleTileManager singleTileManager))
				{
					_tileEventHandler.RaiseTileWasClickedOn(singleTileManager, mousePosition);
				}
			}
		}

		/// <summary>
		/// Triggers a new game by raising the event
		/// </summary>
		protected void TriggerNewGame()
		{
			// This should trigger the TilesManager to generate a new board
			_gameStateEventHandler.RaiseNewGame();
		}

		/// <summary>
		/// A board has been generated
		/// </summary>
		/// <param name="generatedBoard"></param>
		protected void BoardGenerated(HashSet<SingleTileManagerSequence> boardGenerated)
		{
			var foundWords = FindWords(boardGenerated);

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
				_tileEventHandler.RaiseChangeCharactersForTiles(foundWords.SelectMany(x => x.SingleTileManagers).ToHashSet());
			}
		}

		/// <summary>
		/// Loading is complete, usually this means a board is generated without current valid words
		/// </summary>
		protected void LoadingComplete()
		{
			InGameUIController.Instance.HideLoadingCanvas();
			_gameStateEventHandler.RaiseChangeGameState(GameState.WaitingForPlayer);
		}

		/// <summary>
		/// Find any words in the given rows and columns to check
		/// </summary>
		/// <param name="rowsAndColumnsToCheck"></param>
		/// <returns>A list of SingleTileManagerSequences which is a list of valid words in SingleTileManagerSequence form</returns>
		protected HashSet<SingleTileManagerSequence> FindWords(HashSet<SingleTileManagerSequence> rowsAndColumnsToCheck)
		{
			return _wordFinderService.GetListOfValidWordsFromGivenRowsAndOrColumns(_dictionaryService, rowsAndColumnsToCheck);
		}

		/// <summary>
		/// Method valid words for rowsAffected and columnsAffected sequences
		/// </summary>
		/// <returns></returns>
		protected HashSet<SingleTileManagerSequence> GetValidWordsForAffectedRowsAndColumns(HashSet<SingleTileManager> singletileManagers)
		{
			HashSet<SingleTileManagerSequence> rowsAndColumnsToCheck = new();

			var affectedRowsAndColumns = HelperMethods.GetAffectedRowsAndColumns(singletileManagers);

			// We have to check all rows above affected rows since new tiles drop in
			for (int rowIndex = affectedRowsAndColumns.rows.Max(); rowIndex >= 0; rowIndex--)
			{
				rowsAndColumnsToCheck.Add(new SingleTileManagerSequence(TilesManager.Instance.GetFullRow(rowIndex)));
			}

			// We only need to check affected columns
			foreach (var columnIndex in affectedRowsAndColumns.columns)
			{
				rowsAndColumnsToCheck.Add(new SingleTileManagerSequence(TilesManager.Instance.GetFullColumn(columnIndex)));
			}

			// return any valid words from the affected rows and columns to check
			return FindWords(rowsAndColumnsToCheck);
		}

	}
}