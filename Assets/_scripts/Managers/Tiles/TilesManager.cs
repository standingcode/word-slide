using Pooling;
using System;
using System.Collections.Generic;
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
		/// Self evident but this will handle all the event subscriptions
		/// </summary>
		private void SubscribeToEvents()
		{
			gameStateEventHandler.AddNewGameStartedListener(GenerateTileBoard);
		}

		/// <summary>
		/// SElf evident but this removes all the event subscriptions
		/// </summary>
		private void RemoveEventSubscriptions()
		{
			gameStateEventHandler.RemoveNewGameStartedListener(GenerateTileBoard);
		}

		/// <summary>
		/// Initial call to generate the board
		/// </summary>
		public void GenerateTileBoard()
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

			TriggerBoardGeneratedEvent();
		}

		/// <summary>
		/// This is used as part of the process to remove words until we get a board without existing words.
		/// </summary>
		public void ChangeCharactersForTiles(List<SingleTileManager> tilesToChangeCharactersFor)
		{
			foreach (var singleTileManager in tilesToChangeCharactersFor)
			{
				singleTileManager.SetTileCharacter(dictionaryService.GetRandomChar());
			}

			TriggerBoardGeneratedEvent();
		}

		/// <summary>
		/// The board has been generated, subscribers to be notified
		/// </summary>
		private void TriggerBoardGeneratedEvent()
		{
			List<SingleTileManagerSequence> entireBoard = new();

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

			tileEventHandler.RaiseNewBoardGenerated(entireBoard);
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

		/// <summary>
		/// Destroy the given tiles
		/// </summary>
		/// <param name="tilesToRemove"></param>
		public void DestroyTiles(HashSet<SingleTileManager> tilesToRemove)
		{
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
		public SingleTileManager GenerateAndInitializeTile(int i, int j, Vector3? overrideStartPosition = null)
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
		public void SwapTilesAndAnimate(SingleTileManager tile1, SingleTileManager tile2)
		{
			// Swap in the matrix
			boardTiles[tile1.Row, tile1.Column] = tile2;
			boardTiles[tile2.Row, tile2.Column] = tile1;

			// Set the SingleTileManager indexes correctly
			var tile1Row = tile1.Row;
			var tile1Column = tile1.Column;

			tile1.SetNewGridPosition(tile2.Row, tile2.Column);
			tile2.SetNewGridPosition(tile1Row, tile1Column);

			tileEventHandler.RaiseWordCheckNeeded();

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
		public void MoveTileToNewMatrixPosition(int newRowIndex, int newColumnIndex, int oldRowIndex, int oldcolumnIndex)
		{
			boardTiles[newRowIndex, newColumnIndex] = boardTiles[oldRowIndex, oldcolumnIndex];
			boardTiles[oldRowIndex, oldcolumnIndex] = null;

			boardTiles[newRowIndex, newColumnIndex].SetNewGridPosition(newRowIndex, newColumnIndex);

		}

		/// <summary>
		/// Check which tile, if any is due to be swapped with the currently moving tile.
		/// </summary>
		/// <returns></returns>
		public SingleTileManager TileToBeSwappedWithGivenTile(SingleTileManager singleFileManager)
		{
			var vectorFromOriginalPosition = singleFileManager.transform.position - singleFileManager.TileRestingPosition;

			// tile went up
			if (vectorFromOriginalPosition.y > 0)
			{
				int indexOfRowAbove = singleFileManager.Row - 1;

				var ratioOfLimit =
					(singleFileManager.transform.position.y - singleFileManager.TileRestingPosition.y)
					/ (singleFileManager.MovementRestrictions.yMax - singleFileManager.TileRestingPosition.y);

				return ratioOfLimit > SettingsScriptable.RatioOfOverlapToSwapTile ? boardTiles[indexOfRowAbove, singleFileManager.Column] : null;
			}

			// tile went right
			if (vectorFromOriginalPosition.x > 0)
			{
				int indexOfColumnRight = singleFileManager.Column + 1;

				var ratioOfLimit =
				(singleFileManager.transform.position.x - singleFileManager.TileRestingPosition.x)
				/ (singleFileManager.MovementRestrictions.xMax - singleFileManager.TileRestingPosition.x);

				return ratioOfLimit > SettingsScriptable.RatioOfOverlapToSwapTile ? boardTiles[singleFileManager.Row, indexOfColumnRight] : null;
			}

			// tile went down
			if (vectorFromOriginalPosition.y < 0)
			{
				int indexOfRowBelow = singleFileManager.Row + 1;

				var ratioOfLimit =
				(singleFileManager.TileRestingPosition.y - singleFileManager.transform.position.y)
				/ (singleFileManager.TileRestingPosition.y - singleFileManager.MovementRestrictions.yMin);

				return ratioOfLimit > SettingsScriptable.RatioOfOverlapToSwapTile ? boardTiles[indexOfRowBelow, singleFileManager.Column] : null;
			}

			// tile went left	
			if (vectorFromOriginalPosition.x < 0)
			{
				int indexOfColumnLeft = singleFileManager.Column - 1;

				var ratioOfLimit =
				(singleFileManager.TileRestingPosition.x - singleFileManager.transform.position.x)
				/ (singleFileManager.TileRestingPosition.x - singleFileManager.MovementRestrictions.xMin);

				return ratioOfLimit > SettingsScriptable.RatioOfOverlapToSwapTile ? boardTiles[singleFileManager.Row, indexOfColumnLeft] : null;
			}

			return null;
		}
	}
}
