using Pooling;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace WordSlide
{
	public class TilesManager : MonoBehaviour
	{
		[Inject]
		private IDictionaryService dictionaryService;

		[SerializeField]
		private SingleTileManager[,] boardTiles;
		public SingleTileManager[,] BoardTiles => boardTiles;

		[SerializeField]
		private Transform boardTilesRoot;

		protected SingleTileManager currentlyMovingTile = null;

		protected HashSet<SingleTileManager> tilesBeingAnimated = new();
		protected HashSet<SingleTileManager> tilesLastAnimated = new();

		[SerializeField]
		private GameStateEventHandler gameStateEventHandler;

		[SerializeField]
		private TileEventHandler tileEventHandler;

		public static TilesManager Instance { get; private set; }

		public void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
				return;
			}
			Instance = this;

			SubscribeToEvents();
		}

		public void OnDestroy()
		{
			RemoveEventSubscriptions();
			StopAllCoroutines();
			Instance = null;
		}


		/// <summary>
		/// Get a full row of tiles from the board by index number
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public SingleTileManager[] GetFullRow(int index)
		{
			var row = new List<SingleTileManager>();

			for (int i = 0; i < boardTiles.GetLength(1); i++)
			{
				var tile = boardTiles[index, i];

				if (tile != null)
				{
					row.Add(tile);
				}
			}

			return row.ToArray();
		}

		/// <summary>
		/// Get a full column of tiles from the board by index number
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public SingleTileManager[] GetFullColumn(int index)
		{
			var column = new List<SingleTileManager>();

			for (int i = 0; i < boardTiles.GetLength(0); i++)
			{
				var tile = boardTiles[i, index];

				if (tile != null)
				{
					column.Add(boardTiles[i, index]);
				}
			}

			return column.ToArray();
		}

		// PRIVATE METHODS

		/// <summary>
		/// Self evident but this will handle all the event subscriptions
		/// </summary>
		private void SubscribeToEvents()
		{
			gameStateEventHandler.AddNewGameStartedListener(GenerateTileBoard);

			tileEventHandler.AddTileWasClickedOnListener(TileWasClickedOn);
			tileEventHandler.AddChangeCharactersForTilesListener(ChangeCharactersForTiles);

			tileEventHandler.AddTileWasClickedOnListener(TileWasClickedOn);
			tileEventHandler.AddCheckIfTileWasClickedOffListener(CheckIfTileWasClickedOff);

			tileEventHandler.AddTilesNeedSwappingBackListener(SwapTilesAndAnimate);

			tileEventHandler.AddSingleTileFinishedAnimationListener(SingleTileHasFinishedAnimation);

			tileEventHandler.AddTilesNeedsToBeDestroyedListener(DestroyTiles);
		}

		/// <summary>
		/// Self evident but this removes all the event subscriptions
		/// </summary>
		private void RemoveEventSubscriptions()
		{
			gameStateEventHandler.RemoveNewGameStartedListener(GenerateTileBoard);

			tileEventHandler.RemoveTileWasClickedOnListener(TileWasClickedOn);
			tileEventHandler.RemoveChangeCharactersForTilesListener(ChangeCharactersForTiles);

			tileEventHandler.RemoveTileWasClickedOnListener(TileWasClickedOn);
			tileEventHandler.RemoveCheckIfTileWasClickedOffListener(CheckIfTileWasClickedOff);

			tileEventHandler.RemoveTilesNeedSwappingBackListener(SwapTilesAndAnimate);

			tileEventHandler.RemoveSingleTileFinishedAnimationListener(SingleTileHasFinishedAnimation);

			tileEventHandler.RemoveTilesNeedsToBeDestroyedListener(DestroyTiles);
		}

		/// <summary>
		/// Initial call to generate the board
		/// </summary>
		private void GenerateTileBoard()
		{
			DestroyExistingTiles();

			boardTiles = new SingleTileManager[SettingsScriptable.Rows, SettingsScriptable.Columns];

			for (int i = 0; i < boardTiles.GetLength(0); i++)
			{
				for (int j = 0; j < boardTiles.GetLength(1); j++)
				{
					boardTiles[i, j] = GenerateAndInitializeTile(i, j);
				}
			}

			tileEventHandler.RaiseBoardGenerated(GetAllRowsAndColumnsAsTileSequences());
		}

		/// <summary>
		/// A tile has been clicked on
		/// </summary>
		/// <param name="singleTileManager"></param>
		/// <param name="mousePosition"></param>
		private void TileWasClickedOn(SingleTileManager singleTileManager, Vector2 mousePosition)
		{
			currentlyMovingTile = singleTileManager;
			currentlyMovingTile.TileWasClickedOn(mousePosition);
		}

		/// <summary>
		/// Check if there is a currently moving tile, and if so whether it should swap with another tile
		/// </summary>
		private void CheckIfTileWasClickedOff()
		{
			if (currentlyMovingTile == null)
			{
				return;
			}

			var tileToSwapWith = TileToBeSwappedWithCurrentlyMovingTile();

			if (tileToSwapWith == null)
			{
				// Animate back to original position if no swapping
				gameStateEventHandler.RaiseGameStateChanged(GameState.SingleTileIsBeingAnimatedBackToOriginalPosition);

				tilesBeingAnimated.Clear();
				tilesLastAnimated.Clear();

				tilesBeingAnimated.Add(currentlyMovingTile);
				tilesLastAnimated.Add(currentlyMovingTile);

				currentlyMovingTile.AnimateToRestingPositionInGrid();
			}
			else
			{
				gameStateEventHandler.RaiseGameStateChanged(GameState.TilesAreBeingSwapped);
				SwapTilesAndAnimate(currentlyMovingTile, tileToSwapWith);
			}

			currentlyMovingTile = null;
		}

		/// <summary>
		/// This is used as part of the process to remove words until we get a board without existing words.
		/// </summary>
		private void ChangeCharactersForTiles(HashSet<SingleTileManager> tilesToChangeCharactersFor)
		{
			foreach (var singleTileManager in tilesToChangeCharactersFor)
			{
				singleTileManager.SetTileCharacter(dictionaryService.GetRandomChar());
			}

			tileEventHandler.RaiseBoardGenerated(GetAllRowsAndColumnsAsTileSequences());
		}

		/// <summary>
		/// Returns all of the rows and all of the columns as tile sequences.
		/// </summary>
		/// <returns></returns>
		private HashSet<SingleTileManagerSequence> GetAllRowsAndColumnsAsTileSequences()
		{
			HashSet<SingleTileManagerSequence> entireBoard = new();

			// Get all rows
			for (int i = 0; i < boardTiles.GetLength(0); i++)
			{
				entireBoard.Add(new SingleTileManagerSequence(GetFullRow(i)));
			}

			// Get all columns
			for (int i = 0; i < boardTiles.GetLength(1); i++)
			{
				entireBoard.Add(new SingleTileManagerSequence(GetFullColumn(i)));
			}

			return entireBoard;
		}

		/// <summary>
		/// Called from the event triggered by a SingleTileManager
		/// </summary>
		/// <param name="singleTileManager"></param>
		private void SingleTileHasFinishedAnimation(SingleTileManager singleTileManager)
		{
			tilesBeingAnimated.Remove(singleTileManager);

			if (tilesBeingAnimated.Count == 0)
			{
				if (PlayManagerAbstract.GameState == GameState.TilesAreBeingDestroyed)
				{
					foreach (var tile in tilesLastAnimated)
					{
						tile.DeactivateTile();
						PoolManager.Instance.ReturnObjectToPool(tile.GetComponent<PoolObject>());
					}
				}

				tileEventHandler.RaiseAllTilesFinishedAnimating(tilesLastAnimated.ToHashSet());
			}
		}

		/// <summary>
		/// Destroy the given tiles
		/// </summary>
		/// <param name="tilesToRemove"></param>
		private void DestroyTiles(HashSet<SingleTileManager> tilesToRemove)
		{
			gameStateEventHandler.RaiseGameStateChanged(GameState.TilesAreBeingDestroyed);
			tilesLastAnimated = tilesToRemove.ToHashSet();
			tilesBeingAnimated = tilesToRemove.ToHashSet();

			foreach (var tile in tilesToRemove)
			{
				boardTiles[tile.Row, tile.Column] = null;
				tile.StartDestroySequence();
			}
		}

		/// <summary>
		/// Generate and initialize a tile, optonally set position
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="overrideStartPosition"></param>
		/// <returns></returns>
		private SingleTileManager GenerateAndInitializeTile(int i, int j, Vector3? overrideStartPosition = null)
		{
			// Create new tile from the object pool
			PoolObject boardTile = PoolManager.Instance.GetObjectFromPool("tile", boardTilesRoot);

			SingleTileManager singletile = boardTile.GetComponent<SingleTileManager>();
			boardTiles[i, j] = singletile;

			singletile.InitializeTile(dictionaryService.GetRandomChar(), i, j, overrideStartPosition);
			singletile.ActivateTile();

			return singletile;
		}

		/// <summary>
		/// Destroy the existing tiles before generating a new board.
		/// </summary>
		private void DestroyExistingTiles()
		{
			if (boardTiles != null)
			{
				foreach (SingleTileManager tile in boardTiles)
				{
					if (tile != null)
					{
						tile.DeactivateTile();
						PoolManager.Instance.ReturnObjectToPool(tile.GetComponent<PoolObject>());
					}
				}
			}
		}

		/// <summary>
		/// Do the actual swap of 2 tiles, this is done by swapping the references in the matrix and then setting the correct positions for each tile.
		/// </summary>
		/// <param name="tile1"></param>
		/// <param name="tile2"></param>
		private void SwapTilesAndAnimate(SingleTileManager tile1, SingleTileManager tile2)
		{
			tilesLastAnimated.Clear();
			tilesLastAnimated.Add(tile1);
			tilesLastAnimated.Add(tile2);

			tilesBeingAnimated.Clear();
			tilesBeingAnimated.Add(tile1);
			tilesBeingAnimated.Add(tile2);

			// Swap in the matrix
			boardTiles[tile1.Row, tile1.Column] = tile2;
			boardTiles[tile2.Row, tile2.Column] = tile1;

			// Set the SingleTileManager indexes correctly
			var tile1Row = tile1.Row;
			var tile1Column = tile1.Column;

			tile1.SetNewGridPosition(tile2.Row, tile2.Column);
			tile2.SetNewGridPosition(tile1Row, tile1Column);

			tile1.AnimateToRestingPositionInGrid();
			tile2.AnimateToRestingPositionInGrid();
		}

		/// <summary>
		/// Move a tile at a certain grid position to another position. Also updates the tile's matrix indexes and resting position.
		/// </summary>
		/// <param name="newRowIndex"></param>
		/// <param name="newColumnIndex"></param>
		/// <param name="oldRowIndex"></param>
		/// <param name="oldcolumnIndex"></param>
		private void MoveTileToNewMatrixPosition(int newRowIndex, int newColumnIndex, int oldRowIndex, int oldcolumnIndex)
		{
			boardTiles[newRowIndex, newColumnIndex] = boardTiles[oldRowIndex, oldcolumnIndex];
			boardTiles[oldRowIndex, oldcolumnIndex] = null;

			boardTiles[newRowIndex, newColumnIndex].SetNewGridPosition(newRowIndex, newColumnIndex);
		}

		/// <summary>
		/// Checks if a tile overlaps and should be swapped
		/// </summary>
		/// <param name="singleFileManager"></param>
		/// <returns></returns>
		private SingleTileManager TileToBeSwappedWithCurrentlyMovingTile()
		{
			var vectorFromOriginalPosition = currentlyMovingTile.transform.position - currentlyMovingTile.TileRestingPosition;

			// tile went up
			if (vectorFromOriginalPosition.y > 0)
			{
				int indexOfRowAbove = currentlyMovingTile.Row - 1;

				var ratioOfLimit =
					(currentlyMovingTile.transform.position.y - currentlyMovingTile.TileRestingPosition.y)
					/ (currentlyMovingTile.MovementRestrictions.yMax - currentlyMovingTile.TileRestingPosition.y);

				return ratioOfLimit > SettingsScriptable.RatioOfOverlapToSwapTile ? boardTiles[indexOfRowAbove, currentlyMovingTile.Column] : null;
			}

			// tile went right
			if (vectorFromOriginalPosition.x > 0)
			{
				int indexOfColumnRight = currentlyMovingTile.Column + 1;

				var ratioOfLimit =
				(currentlyMovingTile.transform.position.x - currentlyMovingTile.TileRestingPosition.x)
				/ (currentlyMovingTile.MovementRestrictions.xMax - currentlyMovingTile.TileRestingPosition.x);

				return ratioOfLimit > SettingsScriptable.RatioOfOverlapToSwapTile ? boardTiles[currentlyMovingTile.Row, indexOfColumnRight] : null;
			}

			// tile went down
			if (vectorFromOriginalPosition.y < 0)
			{
				int indexOfRowBelow = currentlyMovingTile.Row + 1;

				var ratioOfLimit =
				(currentlyMovingTile.TileRestingPosition.y - currentlyMovingTile.transform.position.y)
				/ (currentlyMovingTile.TileRestingPosition.y - currentlyMovingTile.MovementRestrictions.yMin);

				return ratioOfLimit > SettingsScriptable.RatioOfOverlapToSwapTile ? boardTiles[indexOfRowBelow, currentlyMovingTile.Column] : null;
			}

			// tile went left	
			if (vectorFromOriginalPosition.x < 0)
			{
				int indexOfColumnLeft = currentlyMovingTile.Column - 1;

				var ratioOfLimit =
				(currentlyMovingTile.TileRestingPosition.x - currentlyMovingTile.transform.position.x)
				/ (currentlyMovingTile.TileRestingPosition.x - currentlyMovingTile.MovementRestrictions.xMin);

				return ratioOfLimit > SettingsScriptable.RatioOfOverlapToSwapTile ? boardTiles[currentlyMovingTile.Row, indexOfColumnLeft] : null;
			}

			return null;
		}

		/// <summary>
		/// Spawn a single tile at the given position above a certain column
		/// </summary>
		/// <param name="rowIndex"></param>
		/// <param name="columnIndex"></param>
		/// <param name="positionAbove"></param>
		protected void SpawnANewTile(int rowIndex, int columnIndex, int positionAbove)
		{
			var generatedTile = GenerateAndInitializeTile(
				rowIndex,
				columnIndex,
				SizeManager.Instance.GetAboveColumnStartingPosition(columnIndex, positionAbove)
				);

			generatedTile.ActivateTile();

			// Important to add it to the matrix
			boardTiles[rowIndex, columnIndex] = generatedTile;

			// Add to tiles being dropped								
			tilesBeingAnimated.Add(generatedTile);
			tilesLastAnimated.Add(generatedTile);

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
			MoveTileToNewMatrixPosition(rowIndex, columnIndex, oldRowIndex, columnIndex);
			boardTiles[rowIndex, columnIndex].AnimateToRestingPositionInGrid();

			// Add to tiles being animated
			tilesBeingAnimated.Add(boardTiles[rowIndex, columnIndex]);
			tilesLastAnimated.Add(boardTiles[rowIndex, columnIndex]);
		}

		/// <summary>
		/// When the tiles destruct animation stuff is complete, we spawn or drop existing tiles
		/// </summary>
		protected void MoveAndSpawnTiles()
		{
			gameStateEventHandler.RaiseGameStateChanged(GameState.BoardIsBeingReconfigured);
			tilesBeingAnimated.Clear();
			tilesLastAnimated.Clear();
			var rowsAndColumnsAffected = HelperMethods.GetAffectedRowsAndColumns(tilesLastAnimated);

			// For each column
			foreach (var columnIndex in rowsAndColumnsAffected.rows)
			{
				int indexForTileAbove = 0;

				//For each row(each element in the column starting from the bottom)
				for (int rowIndex = rowsAndColumnsAffected.rows.Max(); rowIndex >= 0; rowIndex--)
				{
					// If a null is found, the tile is destroyed, work up from the index above and find a tile to move down.
					if (boardTiles[rowIndex, columnIndex] == null)
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
								if (boardTiles[rowAbove, columnIndex] != null)
								{
									SetTileToNewPosition(rowAbove, rowIndex, columnIndex);
									break;
								}
							}
						}
					}
				}
			}
		}
	}
}
