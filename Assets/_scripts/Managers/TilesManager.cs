using Pooling;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Android;
using Zenject;

namespace WordSlide
{
	public struct TileWaitingForSpawning
	{
		public SingleTileManager singletileManager;
		public Vector2 spawningPosition;
	}

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
		private ClickEventHandler ClickEventHandler;

		[SerializeField]
		private TileSwappedEventHandler TileSwappedEventHandler;

		[SerializeField]
		private float ratioToSwapTiles = 0.7f;

		private SingleTileManager currentlyMovingTile = null;

		public static TilesManager Instance { get; private set; }

		[SerializeField]
		private SettingsScriptable settings;

		public void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
				return;
			}
			Instance = this;
		}

		private void Start()
		{
			ClickEventHandler.AddClickDownListener(CheckIfTileWasClicked);
			ClickEventHandler.AddClickUpListener(CheckIfTileNeedsToBeDropped);
		}

		public void OnDestroy()
		{
			Instance = null;
		}

		/// <summary>
		/// Initial call to generate the board with no words present. Calls GenerateFullTileBoard()
		/// </summary>
		/// <param name="wordFinderService"></param>
		/// <param name="dictionaryService"></param>
		public void GenerateTileBoardAndRemoveAnyExistingValidWords(IWordFinderService wordFinderService, IDictionaryService dictionaryService)
		{
			List<SingleTileManagerSequence> foundWords = new();

			DestroyExistingTiles();

			boardTiles = new SingleTileManager[SettingsScriptable.Rows, SettingsScriptable.Columns];

			for (int i = 0; i < boardTiles.GetLength(0); i++)
			{
				for (int j = 0; j < boardTiles.GetLength(1); j++)
				{
					boardTiles[i, j] = GenerateTile(i, j);
				}
			}

			var sw = System.Diagnostics.Stopwatch.StartNew();

			do
			{
				List<SingleTileManagerSequence> rowsAndColumnsToCheck = new();

				// Get all rows
				for (int i = 0; i < BoardTiles.GetLength(0); i++)
				{
					rowsAndColumnsToCheck.Add(new SingleTileManagerSequence(GetFullRow(i)));
				}

				// Get all columns
				for (int i = 0; i < BoardTiles.GetLength(1); i++)
				{
					rowsAndColumnsToCheck.Add(new SingleTileManagerSequence(GetFullColumn(i)));
				}

				foundWords = wordFinderService.GetListOfValidWordsFromGivenRowsAndOrColumns(dictionaryService, rowsAndColumnsToCheck);

				if (foundWords.Count > 0)
				{
					foreach (var singleTileManagerSequence in foundWords)
					{
						ChangeCharactersForTiles(singleTileManagerSequence.SingleTileManagers);
					}
				}

			} while (foundWords.Count > 0);

			sw.Stop();
			Debug.Log($"Generating the board witout words took {sw.ElapsedMilliseconds} milliseconds.");
			ShowBoard();
		}

		private SingleTileManager GenerateTile(int i, int j)
		{
			// Create new tile from the object pool
			PoolObject boardTile = PoolManager.Instance.GetObjectFromPool("tile", boardTilesRoot);

			SingleTileManager singletile = boardTile.GetComponent<SingleTileManager>();

			singletile.InitializeTile(dictionaryService.GetRandomChar(), i, j);

			return singletile;
		}

		/// <summary>
		/// Allows the board to be shown after we are generating and making sure there are no words present.
		/// </summary>
		public void ShowBoard()
		{
			for (int i = 0; i < boardTiles.GetLength(0); i++)
			{
				for (int j = 0; j < boardTiles.GetLength(1); j++)
				{
					boardTiles[i, j].ActivateTile();
				}
			}
		}

		/// <summary>
		/// This is used as part of the process to remove words until we get a board without existing words.
		/// </summary>
		public void ChangeCharactersForTiles(SingleTileManager[] listOfTilesToChangeCharacter)
		{
			foreach (var singleTileManager in listOfTilesToChangeCharacter)
			{
				singleTileManager.SetTileCharacter(dictionaryService.GetRandomChar());
			}
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
				row.Add(boardTiles[index, i]);
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
				column.Add(boardTiles[i, index]);
			}

			return column.ToArray();
		}

		List<SingleTileManager> tilesWaitingToBeRemoved = new();
		public void RemoveTileFromBoard(SingleTileManager singleTileManager)
		{
			tilesWaitingToBeRemoved.Add(singleTileManager);
			BoardTiles[singleTileManager.MatrixIndex.Item1, singleTileManager.MatrixIndex.Item2] = null;
		}

		public void NewTilesNeeded(SingleTileManager singleTileManagerCaller)
		{
			// We want this to be triggered by animation end, but we only want it called once
			// All SingleTileManagers will call this method, so just use index 0
			if (tilesWaitingToBeRemoved.Count == 0 || singleTileManagerCaller != tilesWaitingToBeRemoved[0])
			{
				return;
			}

			singleTileManagerCaller = null;

			Debug.Log("New tiles needed called");

			// For each column
			for (int column = 0; column < BoardTiles.GetLength(1); column++)
			{
				SingleTileManager singleTileManagerSpawnedOutSideHighest = null;

				// For each row (each element in the column)
				for (int row = BoardTiles.GetLength(0) - 1; row >= 0; row--)
				{
					// If a null is found, work up from the index above and find a tile to move down.
					if (BoardTiles[row, column] == null)
					{
						// For each of the rows above this null tile
						for (int rowAbove = row - 1; rowAbove >= -1; rowAbove--)
						{
							// If row above is -1 we need to spawn a tile
							if (rowAbove == -1)
							{
								int columnMultiplier = column % SettingsScriptable.Columns;
								int rowMultiplier = 0 % SettingsScriptable.Rows;

								var yTopLeftPoint
									= SizeManager.Instance.TileSpawnTopLeftStartingPoint.y - (rowMultiplier * (SizeManager.Instance.TileSize.y + SizeManager.Instance.InteriorPaddingSizes.y));

								var xTopLeftPoint
									= SizeManager.Instance.TileSpawnTopLeftStartingPoint.x + (columnMultiplier * (SizeManager.Instance.TileSize.x + SizeManager.Instance.InteriorPaddingSizes.x));

								float requiredXPosition = xTopLeftPoint;

								float requiredYPosition = singleTileManagerSpawnedOutSideHighest != null ? singleTileManagerSpawnedOutSideHighest.transform.position.y + (SizeManager.Instance.TileSize.y + SizeManager.Instance.InteriorPaddingSizes.y) :
									yTopLeftPoint + (SizeManager.Instance.TileSize.y + SizeManager.Instance.InteriorPaddingSizes.y);

								var generatedTile = GenerateTile(row, column);

								generatedTile.SetTilePosition(new Vector3(requiredXPosition, requiredYPosition, 0f));

								generatedTile.ActivateTile();

								singleTileManagerSpawnedOutSideHighest = generatedTile;

								boardTiles[row, column] = generatedTile;
							}
							else
							{
								// If we find a tile
								if (boardTiles[rowAbove, column] != null)
								{
									// Swap in the matrix
									boardTiles[row, column] = BoardTiles[rowAbove, column];
									boardTiles[rowAbove, column] = null;

									boardTiles[row, column].InitializeTile(boardTiles[row, column].TileCharacter, row, column);
									boardTiles[row, column].ActivateTile();

									break;
								}
							}
						}
					}
				}
			}

			foreach (var singleTileManager in tilesWaitingToBeRemoved)
			{
				for (int i = singleTileManager.MatrixIndex.Item1; i >= 0; i--)
				{
					Debug.Log($"{i}");
				}
			}

			tilesWaitingToBeRemoved.Clear();

			// TODO: We need to call word finder on the affected rows and columns after the tile drops.			
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
		/// When mouse is clicked, we fire a ray and see if it hits a tile's collider, we then know if the tile is selected.
		/// </summary>
		/// <param name="mousePosition"></param>
		private void CheckIfTileWasClicked(Vector2 mousePosition)
		{
			//If the player is not allowed to interact, return
			if (!PlayManager.Instance.PlayerCanInteractWithTiles)
			{
				return;
			}

			// If a tile is alerady in motion, it needs dropping first
			CheckIfTileNeedsToBeDropped();

			// Shoot ray from main camera and detect what it hits
			Ray ray = Camera.main.ScreenPointToRay(mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				if (hit.collider.TryGetComponent(out SingleTileManager singleTileManager))
				{
					//Debug.Log($"{singleTileManager.MatrixIndex}");
					currentlyMovingTile = singleTileManager;
					currentlyMovingTile.TileWasClickedOn(mousePosition);
				}
			}
		}

		/// <summary>
		/// Check for tile swap or return to original position if click event is up and there is a tile in motion
		/// This dropping procedure will then need to check for words eventually
		/// </summary>
		private void CheckIfTileNeedsToBeDropped()
		{
			if (currentlyMovingTile != null)
			{
				var tileToSwapWith = TileToBeSwappedWithCurrentlyMovingTile();

				if (tileToSwapWith == null)
				{
					currentlyMovingTile.ResetTileToOriginalPosition();
				}
				else
				{
					SwapTiles(currentlyMovingTile, tileToSwapWith);
					DetermineWhichRowsAndColumnsAreAffectedAndRaiseEvent(currentlyMovingTile, tileToSwapWith);
				}
				currentlyMovingTile = null;
			}
		}

		/// <summary>
		/// Returns the rows and columns affected by two tiles, which will be ones that have just been swapped.
		/// </summary>
		/// <param name="tile1"></param>
		/// <param name="tile2"></param>
		private void DetermineWhichRowsAndColumnsAreAffectedAndRaiseEvent(SingleTileManager tile1, SingleTileManager tile2)
		{
			List<SingleTileManagerSequence> listOfRowsAndColumnsToCheck = new();

			// Determine which rows and columns were affected by the swap	
			if (tile1.MatrixIndex.Item1 == tile2.MatrixIndex.Item1)
			{
				listOfRowsAndColumnsToCheck = new List<SingleTileManagerSequence>
				{
					new SingleTileManagerSequence(GetFullRow(tile1.MatrixIndex.Item1)),
					new SingleTileManagerSequence(GetFullColumn(tile1.MatrixIndex.Item2)),
					new SingleTileManagerSequence(GetFullColumn(tile2.MatrixIndex.Item2))
				};
			}
			else
			{
				listOfRowsAndColumnsToCheck = new List<SingleTileManagerSequence>
				{
					new SingleTileManagerSequence(GetFullRow(tile1.MatrixIndex.Item1)),
					new SingleTileManagerSequence(GetFullRow(tile2.MatrixIndex.Item1)),
					new SingleTileManagerSequence(GetFullColumn(tile1.MatrixIndex.Item2))
				};
			}

			TileSwappedEventHandler.RaiseTileSwapped(listOfRowsAndColumnsToCheck);
		}

		/// <summary>
		/// Do the actual swap of 2 tiles, this is done by swapping the references in the matrix and then setting the correct positions for each tile.
		/// </summary>
		/// <param name="tile1"></param>
		/// <param name="tile2"></param>
		private void SwapTiles(SingleTileManager tile1, SingleTileManager tile2)
		{
			// Swap in the matrix
			boardTiles[tile1.MatrixIndex.Item1, tile1.MatrixIndex.Item2] = tile2;
			boardTiles[tile2.MatrixIndex.Item1, tile2.MatrixIndex.Item2] = tile1;

			// Now set the SingleTileManage settings correctly
			var tile1MatrixIndex = tile1.MatrixIndex;
			tile1.SwapTileToNewPosition(tile2.MatrixIndex.Item1, tile2.MatrixIndex.Item2);
			tile2.SwapTileToNewPosition(tile1MatrixIndex.Item1, tile1MatrixIndex.Item2);
		}

		/// <summary>
		/// Check which tile, if any is due to be swapped with the currently moving tile.
		/// </summary>
		/// <returns></returns>
		private SingleTileManager TileToBeSwappedWithCurrentlyMovingTile()
		{
			var vectorFromOriginalPosition = currentlyMovingTile.transform.position - currentlyMovingTile.TileRestingPosition;

			// tile went up
			if (vectorFromOriginalPosition.y > 0)
			{
				int indexOfRowAbove = currentlyMovingTile.MatrixIndex.Item1 - 1;

				var ratioOfLimit =
					(currentlyMovingTile.transform.position.y - currentlyMovingTile.TileRestingPosition.y)
					/ (currentlyMovingTile.MovementRestrictions.yMax - currentlyMovingTile.TileRestingPosition.y);

				return ratioOfLimit > ratioToSwapTiles ? boardTiles[indexOfRowAbove, currentlyMovingTile.MatrixIndex.Item2] : null;
			}

			// tile went right
			if (vectorFromOriginalPosition.x > 0)
			{
				int indexOfColumnRight = currentlyMovingTile.MatrixIndex.Item2 + 1;

				var ratioOfLimit =
				(currentlyMovingTile.transform.position.x - currentlyMovingTile.TileRestingPosition.x)
				/ (currentlyMovingTile.MovementRestrictions.xMax - currentlyMovingTile.TileRestingPosition.x);

				return ratioOfLimit > ratioToSwapTiles ? boardTiles[currentlyMovingTile.MatrixIndex.Item1, indexOfColumnRight] : null;
			}

			// tile went down
			if (vectorFromOriginalPosition.y < 0)
			{
				int indexOfRowBelow = currentlyMovingTile.MatrixIndex.Item1 + 1;

				var ratioOfLimit =
				(currentlyMovingTile.TileRestingPosition.y - currentlyMovingTile.transform.position.y)
				/ (currentlyMovingTile.TileRestingPosition.y - currentlyMovingTile.MovementRestrictions.yMin);

				return ratioOfLimit > ratioToSwapTiles ? boardTiles[indexOfRowBelow, currentlyMovingTile.MatrixIndex.Item2] : null;
			}

			// tile went left	
			if (vectorFromOriginalPosition.x < 0)
			{
				int indexOfColumnLeft = currentlyMovingTile.MatrixIndex.Item2 - 1;

				var ratioOfLimit =
				(currentlyMovingTile.TileRestingPosition.x - currentlyMovingTile.transform.position.x)
				/ (currentlyMovingTile.TileRestingPosition.x - currentlyMovingTile.MovementRestrictions.xMin);

				return ratioOfLimit > ratioToSwapTiles ? boardTiles[currentlyMovingTile.MatrixIndex.Item1, indexOfColumnLeft] : null;
			}

			return null;
		}
	}
}
