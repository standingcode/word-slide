using Pooling;
using System.Collections.Generic;
using System.Linq;
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

			// Move to swap checking etc.
			_gameStateEventHandler.RaiseGameStateChanged(GameState.TileSwapInProgress);

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
			tilesBeingAnimated.Remove(singleTileManager);

			if (tilesBeingAnimated.Count > 0)
			{
				return;
			}

			// By here, No tiles being animated

			if (tilesToBeDestroyed.Count > 0)
			{
				TilesManager.Instance.DestroyTiles(tilesToBeDestroyed);
				return;
			}

			// Return control to the player
			_gameStateEventHandler.RaiseGameStateChanged(GameState.WaitingForPlayer);
		}

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

			MoveAndSpawnTiles();
		}

		/// <summary>
		/// Method is called when 2 tiles have completed a swap
		/// </summary>
		/// <param name="rowsAndColumnsToCheck"></param>
		protected override void CheckWords()
		{
			Debug.Log($"{nameof(CheckWords)} called");

			// Get the valid words for the affected rows and columns
			var validWords = GetValidWordsForAffectedRowsAndColumns();

			// Print each of the valid words to console
			validWords.ForEach(x => Debug.Log($"{x.ToString()}"));

			rowsAffected.Clear();
			columnsAffected.Clear();

			// If there are no valid words
			if (validWords.Count == 0)
			{
				// If there are no tiles being animated
				if (tilesBeingAnimated.Count == 0)
				{
					// Return control to the player
					_gameStateEventHandler.RaiseGameStateChanged(GameState.WaitingForPlayer);
				}

				return;
			}

			// Now set rowsAffected and columnsAffected to be the ones containing valid words.
			foreach (var word in validWords)
			{
				foreach (var tile in word.SingleTileManagers)
				{
					rowsAffected.Add(tile.Row);
					columnsAffected.Add(tile.Column);
				}
			}

			// Valid words are marked for destruction
			AddTilesToBeDestroyedToList(validWords);

			// If no tiles are being animated or dropped, we need to destroy the tiles now (otherwise they will be destroyed when animations are completed)
			if (tilesBeingAnimated.Count == 0)
			{
				TilesManager.Instance.DestroyTiles(tilesToBeDestroyed);
			}
		}

		/// <summary>
		/// Add all the tiles to be destroyed to tilesToBeDestroyed
		/// </summary>
		/// <param name="tileSequencesToDestroy"></param>
		private void AddTilesToBeDestroyedToList(List<SingleTileManagerSequence> tileSequencesToDestroy)
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
		/// Spawn a single tile at the given position above a certain column
		/// </summary>
		/// <param name="rowIndex"></param>
		/// <param name="columnIndex"></param>
		/// <param name="positionAbove"></param>
		protected void SpawnANewTile(int rowIndex, int columnIndex, int positionAbove)
		{
			var generatedTile = TilesManager.Instance.GenerateAndInitializeTile(
				rowIndex,
				columnIndex,
				SizeManager.Instance.GetAboveColumnStartingPosition(columnIndex, positionAbove)
				);

			generatedTile.ActivateTile();

			// Important to add it to the matrix
			BoardTiles[rowIndex, columnIndex] = generatedTile;

			// Add to tiles being dropped								
			tilesBeingAnimated.Add(generatedTile);

			// Animate the tile to its resting position
			generatedTile.AnimateToRestingPositionInGrid();
		}

		/// <summary>
		/// Move a tile from one matrix position to another, this is used when a tile is destroyed and we need to drop tiles from above
		/// </summary>
		/// <param name="oldRowIndex"></param>
		/// <param name="rowIndex"></param>
		/// <param name="columnIndex"></param>
		protected void SetTileToNewPosition(int oldRowIndex, int rowIndex, int columnIndex)
		{
			// Swap in the matrix
			TilesManager.Instance.MoveTileToNewMatrixPosition(rowIndex, columnIndex, oldRowIndex, columnIndex);
			BoardTiles[rowIndex, columnIndex].AnimateToRestingPositionInGrid();

			// Add to tiles being animated
			tilesBeingAnimated.Add(BoardTiles[rowIndex, columnIndex]);
		}

		/// <summary>
		/// When the tiles destruct animation stuff is complete, we spawn or drop existing tiles
		/// </summary>
		protected override void MoveAndSpawnTiles()
		{
			_gameStateEventHandler.RaiseGameStateChanged(GameState.GeneratingNewTilesAndDroppingInProgress);

			var listOfColumnsAffected = columnsAffected.ToList();
			listOfColumnsAffected.Sort();

			var listOfRowsAffected = rowsAffected.ToList();
			listOfRowsAffected.Sort();

			// For each column
			foreach (var columnIndex in listOfColumnsAffected)
			{
				int indexForTileAbove = 0;

				//For each row(each element in the column starting from the bottom)
				for (int rowIndex = listOfRowsAffected.Max(); rowIndex >= 0; rowIndex--)
				{
					// If a null is found, the tile is destroyed, work up from the index above and find a tile to move down.
					if (BoardTiles[rowIndex, columnIndex] == null)
					{
						// For each of the rows above this null tile
						for (int rowAbove = rowIndex - 1; rowAbove >= -1; rowAbove--)
						{
							// If row above is -1 we need to spawn a tile
							if (rowAbove == -1)
							{
								SpawnANewTile(rowIndex, columnIndex, indexForTileAbove);
								indexForTileAbove++;
							}
							else
							{
								// If we find a tile
								if (BoardTiles[rowAbove, columnIndex] != null)
								{
									SetTileToNewPosition(rowAbove, rowIndex, columnIndex);
									break;
								}
							}
						}
					}
				}
			}

			CheckWords();
		}
	}
}
