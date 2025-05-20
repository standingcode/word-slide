using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WordSlide
{
	/// <summary>
	/// "Classic" version of the game
	/// - Tiles swap back if no words are found
	/// - Round is timed
	/// </summary>
	public class PlayManagerClassic : PlayManagerAbstract
	{
		/// <inheritdoc/>
		public override void AllTileAnimationsCompleted(HashSet<SingleTileManager> singleTileManagers)
		{
			switch (GameStateEventHandler.GameState)
			{
				case GameState.TilesAreBeingSwapped:
				case GameState.BoardIsBeingReconfigured:
					CheckWords(singleTileManagers);
					break;
				case GameState.SingleTileIsBeingAnimatedBackToOriginalPosition:
				case GameState.TilesAreBeingSwappedBack:
					_gameStateEventHandler.RaiseChangeGameState(GameState.WaitingForPlayer);
					break;
				case GameState.TilesAreBeingDestroyed:
					_tileEventHandler.RaiseBoardRequiresReconfiguring();
					break;
			}
		}

		/// <inheritdoc/>
		protected override void CheckWords(HashSet<SingleTileManager> singleTileManagers)
		{
			// Get the valid words for the affected rows and columns
			var validWords = GetValidWordsForAffectedRowsAndColumns(singleTileManagers);

			// Print each of the valid words to console
			validWords.ToList().ForEach(x => Debug.Log($"{x.ToString()}"));

			// If there are no valid words
			if (validWords.Count == 0)
			{
				if (GameStateEventHandler.GameState == GameState.TilesAreBeingSwapped)
				{
					var singleTileManagersToSwapBack = singleTileManagers.ToList();
					_tileEventHandler.RaiseTilesNeedSwappingBack(singleTileManagersToSwapBack[0], singleTileManagersToSwapBack[1]);
				}
				else if (GameStateEventHandler.GameState == GameState.BoardIsBeingReconfigured)
				{
					_gameStateEventHandler.RaiseChangeGameState(GameState.WaitingForPlayer);
				}

				return;
			}

			// If there are valid words, we need to destroy the tiles in the words
			RaiseDestroy(validWords);
		}

		/// <inheritdoc/>
		protected override void RaiseDestroy(HashSet<SingleTileManagerSequence> tileSequencesToDestroy)
		{
			HashSet<SingleTileManager> tilesToBeDestroyed = new HashSet<SingleTileManager>();

			// For each of the words (one word is a SingleTileManagerSequence)
			foreach (var word in tileSequencesToDestroy)
			{
				// For each of the tiles in the word
				foreach (var singleTileManager in word.SingleTileManagers)
				{
					tilesToBeDestroyed.Add(singleTileManager);
				}
			}

			_tileEventHandler.RaiseTilesNeedsToBeDestroyed(tilesToBeDestroyed);
		}

	}
}
