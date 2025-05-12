using System.Collections.Generic;
using UnityEngine;

namespace WordSlide
{
	/// <summary>
	/// Method called once player has swapped 2 tiles, logic here deals with checking words, destructing tiles, and what to do next
	/// TODO: Pass score to some ScoreManager
	/// </summary>
	public class PlayManagerClassic : PlayManagerAbstract
	{
		protected override void TilesSwappedByUser(List<SingleTileManagerSequence> rowsAndColumnsToCheck)
		{
			// Block player input
			_gameStateEventHandler.RaisePlayerCanInteractWithTilesChanged(false);

			// If this has somehow been called by accident and there is nothing to check, we can return control to the player
			if (rowsAndColumnsToCheck.Count == 0)
			{
				_gameStateEventHandler.RaisePlayerCanInteractWithTilesChanged(true);
				return;
			}

			// Get any valid words from the given rows and columns to check
			var validWords = FindWords(rowsAndColumnsToCheck);

			// Print each of the valid words to console
			validWords.ForEach(x => Debug.Log($"{x.ToString()}"));

			// If there are no valid words we can return control to the player
			if (validWords.Count == 0)
			{
				_gameStateEventHandler.RaisePlayerCanInteractWithTilesChanged(true);
				return;
			}

			List<SingleTileManager> tilesToDestroy = new();

			// For each of the words (one word is a SingleTileManagerSequence)
			foreach (var word in validWords)
			{
				// For each of the tiles in the word
				foreach (var singleTileManager in word.SingleTileManagers)
				{
					// TODO: Need to make sure that we still count this when working out points,
					// we just don't want to try and double destroy the same tile.
					if (tilesToDestroy.Contains(singleTileManager))
					{
						continue;
					}

					// This tile will need to be removed,
					// if it has not already been added to the list of tiles to destroy, add it
					if (!tilesToDestroy.Contains(singleTileManager))
					{
						tilesToDestroy.Add(singleTileManager);
					}
				}
			}

			// Call TilesManager to destroy the tiles
			TilesManager.Instance.DestroyTiles(tilesToDestroy);
		}
	}
}
