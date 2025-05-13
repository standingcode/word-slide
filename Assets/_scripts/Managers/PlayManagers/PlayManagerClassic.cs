using Pooling;
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
		/// <summary>
		/// Check for tile swap or return to original position if click event is up and there is a tile in motion
		/// </summary>
		protected override void UserUnClickedPointerCheckNextStep()
		{
			if (currentlyMovingTile == null)
			{
				return;
			}

			// Block player input
			_gameStateEventHandler.RaisePlayerCanInteractWithTilesChanged(false);

			var tileToSwapWith = TilesManager.Instance.TileToBeSwappedWithGivenTile(currentlyMovingTile);

			if (tileToSwapWith == null)
			{
				// This is to wait for the animation to return the tile
				tilesBeingAnimated.Add(currentlyMovingTile);

				// Animate back to original position if no swapping
				currentlyMovingTile.AnimateToRestingPositionInGrid();
			}
			else
			{
				tilesBeingAnimated.Add(currentlyMovingTile);
				tilesBeingAnimated.Add(tileToSwapWith);

				// Mark the affected rows and columns
				rowsAffected.Add(currentlyMovingTile.Row);
				rowsAffected.Add(tileToSwapWith.Row);
				columnsAffected.Add(currentlyMovingTile.Column);
				columnsAffected.Add(tileToSwapWith.Column);

				// Animate the swap
				TilesManager.Instance.SwapTilesAndAnimate(currentlyMovingTile, tileToSwapWith);
			}

			currentlyMovingTile = null;
		}

		/// <summary>
		/// Tile animation complete
		/// </summary>
		/// <param name="singleTileManager"></param>
		public override void TileAnimationComplete(SingleTileManager singleTileManager)
		{
			Debug.Log($"Tile animation complete: {singleTileManager.TileCharacter}");

			// If the animations ending are swap animations
			if (tilesBeingAnimated.Count > 0)
			{
				tilesBeingAnimated.Remove(singleTileManager);

				if (tilesBeingAnimated.Count == 0)
				{
					if (tilesToBeDestroyed.Count > 0)
					{
						TilesManager.Instance.DestroyTiles(tilesToBeDestroyed);
					}
					else
					{
						_gameStateEventHandler.RaisePlayerCanInteractWithTilesChanged(true);
					}
				}
			}
			// If the animations ending are dropping animations
			else if (tilesBeingDropped.Count > 0)
			{
				tilesBeingDropped.Remove(singleTileManager);

				if (tilesBeingDropped.Count == 0)
				{
					_gameStateEventHandler.RaisePlayerCanInteractWithTilesChanged(true);
				}
			}
		}

		//public override void TileDropComplete(SingleTileManager singleTileManager)
		//{
		//	Debug.Log($"Tile drop complete: {singleTileManager.TileCharacter}");

		//	if (tilesToBeDestroyed.Count == 0)
		//	{
		//		_gameStateEventHandler.RaisePlayerCanInteractWithTilesChanged(true);
		//	}
		//	else
		//	{
		//		// Call TilesManager to destroy the tiles
		//		TilesManager.Instance.DestroyTiles(tilesToBeDestroyed);
		//	}
		//}

		/// <summary>
		/// Once destruction is completed we will need to call the spawn logic
		/// </summary>
		/// <param name="singleTileManager"></param>
		protected override void TileDestructSequenceCompleted(SingleTileManager singleTileManager)
		{
			tilesToBeDestroyed.Remove(singleTileManager);
			singleTileManager.DeactivateTile();
			PoolManager.Instance.ReturnObjectToPool(singleTileManager.GetComponent<PoolObject>());

			if (tilesToBeDestroyed.Count > 0)
			{
				return;
			}

			Debug.Log("Destruct sequence completed, now we need to spawn new tiles etc");

			TileDestructSequenceCompleted();
		}

		/// <summary>
		/// Method is called when 2 tiles have completed a swap
		/// </summary>
		/// <param name="rowsAndColumnsToCheck"></param>
		protected override void CheckWords()
		{
			List<SingleTileManagerSequence> rowsAndColumnsToCheck = new();

			foreach (var row in rowsAffected)
			{
				rowsAndColumnsToCheck.Add(new SingleTileManagerSequence(TilesManager.Instance.GetFullRow(row)));
			}

			foreach (var column in columnsAffected)
			{
				rowsAndColumnsToCheck.Add(new SingleTileManagerSequence(TilesManager.Instance.GetFullColumn(column)));
			}

			// Get any valid words from the given rows and columns to check
			var validWords = FindWords(rowsAndColumnsToCheck);

			// Print each of the valid words to console
			validWords.ForEach(x => Debug.Log($"{x.ToString()}"));

			if (validWords.Count == 0)
			{
				if (tilesBeingAnimated.Count == 0)
				{
					_gameStateEventHandler.RaisePlayerCanInteractWithTilesChanged(true);
				}

				return;
			}

			SetDestroyRequest(validWords);
		}

		private void SetDestroyRequest(List<SingleTileManagerSequence> tileSequencesToDestroy)
		{
			// For each of the words (one word is a SingleTileManagerSequence)
			foreach (var word in tileSequencesToDestroy)
			{
				// For each of the tiles in the word
				foreach (var singleTileManager in word.SingleTileManagers)
				{
					tilesToBeDestroyed.Add(singleTileManager);
				}
			}
		}

		/// <summary>
		/// When the tiles destruct animation stuff is complete, we spawn or drop existing tiles
		/// </summary>
		protected override void TileDestructSequenceCompleted()
		{
			// For each column
			for (int columnIndex = 0; columnIndex < BoardTiles.GetLength(1); columnIndex++)
			{
				int indexForTileAbove = 0;

				// For each row (each element in the column starting from the bottom)
				for (int rowIndex = BoardTiles.GetLength(0) - 1; rowIndex >= 0; rowIndex--)
				{
					// If a null is found, the tile is destroyed, work up from the index above and find a tile to move down.
					if (BoardTiles[rowIndex, columnIndex] == null)
					{
						// This row and column is affected
						rowsAffected.Add(rowIndex);
						columnsAffected.Add(columnIndex);

						// For each of the rows above this null tile
						for (int rowAbove = rowIndex - 1; rowAbove >= -1; rowAbove--)
						{
							// If row above is -1 we need to spawn a tile
							if (rowAbove == -1)
							{
								var generatedTile = TilesManager.Instance.GenerateAndInitializeTile(
									rowIndex,
									columnIndex,
									SizeManager.Instance.GetAboveColumnStartingPosition(columnIndex, indexForTileAbove)
									);

								generatedTile.ActivateTile();

								// Add to tiles being dropped								
								tilesBeingDropped.Add(generatedTile);

								generatedTile.MakeTileDropToRestingPosition();

								indexForTileAbove++;
							}
							else
							{
								// If we find a tile
								if (BoardTiles[rowAbove, columnIndex] != null)
								{
									// Add to tiles being dropped
									tilesBeingDropped.Add(BoardTiles[rowAbove, columnIndex]);

									// Swap in the matrix
									TilesManager.Instance.MoveTileToNewMatrixPosition(rowIndex, columnIndex, rowAbove, columnIndex);
									BoardTiles[rowIndex, columnIndex].MakeTileDropToRestingPosition();
								}
							}
						}
					}
				}
			}
		}
	}
}
