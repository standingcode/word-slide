using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
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

	public abstract class PlayManagerAbstract : MonoBehaviour
	{
		public static PlayManagerAbstract Instance { get; private set; }

		[SerializeField]
		private bool playerCanInteractWithTiles;
		public bool PlayerCanInteractWithTiles => playerCanInteractWithTiles;

		private IDictionaryService _dictionaryService;
		private IWordFinderService _wordFinderService;

		[SerializeField]
		private GameStateEventHandler _gameStateEventHandler;

		public int TilesEnteringDestructSequenceCount { get; set; } = 0;

		public void Awake()
		{
			_gameStateEventHandler = (GameStateEventHandler)Resources.Load("ScriptableObjects/GameStateEventHandler");
		}

		public void Initialize(IDictionaryService dictionaryService, IWordFinderService wordFinderService, GameStateEventHandler gameStateEventHandler)
		{
			_dictionaryService = dictionaryService;
			_wordFinderService = wordFinderService;
			_gameStateEventHandler = gameStateEventHandler;

			ConfigureRendering();
			ConfigureInputHandling();
			InitializeEventSubscriptions();
			TriggerNewGame();
		}

		private void ConfigureRendering()
		{
			Application.targetFrameRate = 120;
		}

		private void ConfigureInputHandling()
		{
			InputSystem.pollingFrequency = 120;
		}

		private void InitializeEventSubscriptions()
		{
			_gameStateEventHandler.AddTileSwappedListener(TilesSwappedByUser);
			_gameStateEventHandler.AddNewBoardGeneratedListener(BoardGenerated);
		}

		private void RemoveEventSubscriptions()
		{
			_gameStateEventHandler.RemoveTileSwappedListener(TilesSwappedByUser);
		}

		private void TriggerNewGame()
		{
			// This should trigger the TilesManagerSomeGameMode to generate a new board
			_gameStateEventHandler.RaiseNewGameStarted();
		}

		private void BoardGenerated(List<SingleTileManagerSequence> generatedBoard)
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
				_gameStateEventHandler.RaiseChangeTilesRequestedDueToContainingWords(foundWords);
			}
		}

		private void LoadingComplete()
		{
			InGameUIController.Instance.HideLoadingCanvas();
			playerCanInteractWithTiles = true;
		}

		private void TilesSwappedByUser(List<SingleTileManagerSequence> rowsAndColumnsToCheck)
		{
			playerCanInteractWithTiles = false;

			if (rowsAndColumnsToCheck.Count == 0)
			{
				playerCanInteractWithTiles = true;
				return;
			}

			var validWords = FindWords(rowsAndColumnsToCheck);

			validWords.ForEach(x => Debug.Log($"{x.ToString()}"));

			if (validWords.Count == 0)
			{
				playerCanInteractWithTiles = true;
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

			// TODO: Trigger destory tiles event
			//TilesManager.Instance.DestroyTiles(tilesToDestroy);



			return;

			//playerCanInteractWithTiles = false;

			//if (rowsAndColumnsToCheck.Count == 0)
			//{
			//	playerCanInteractWithTiles = true;
			//	return;
			//}

			//var validWords = _wordFinderService.GetListOfValidWordsFromGivenRowsAndOrColumns(_dictionaryService, rowsAndColumnsToCheck);
			//validWords.ForEach(x => Debug.Log($"{x.ToString()}"));

			//if (validWords.Count == 0)
			//{
			//	playerCanInteractWithTiles = true;
			//	return;
			//}

			//List<SingleTileManager> tilesToDestroy = new();

			//foreach (var word in validWords)
			//{
			//	foreach (var singleTileManager in word.SingleTileManagers)
			//	{
			//		// TODO: Need to make sure that we still count this when working out points,
			//		// we just don't want to try and double destroy the same tile.
			//		if (tilesToDestroy.Contains(singleTileManager))
			//		{
			//			continue;
			//		}

			//		tilesToDestroy.Add(singleTileManager);
			//	}
			//}

			//// TODO: Trigger destory tiles event
			////TilesManager.Instance.DestroyTiles(tilesToDestroy);
		}

		private List<SingleTileManagerSequence> FindWords(List<SingleTileManagerSequence> rowsAndColumnsToCheck)
		{
			return _wordFinderService.GetListOfValidWordsFromGivenRowsAndOrColumns(_dictionaryService, rowsAndColumnsToCheck);
		}

		public void OnDestroy()
		{
			RemoveEventSubscriptions();
			Instance = null;
		}
	}
}