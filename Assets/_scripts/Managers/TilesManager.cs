using Pooling;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Android;
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
			StopAllCoroutines();
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
					boardTiles[i, j] = GenerateAndInitializeTile(i, j);
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
		private int tilesFalling = 0;
		private List<(int, int)> tilesAffected = new();

		public void DestroyTiles(List<SingleTileManager> tilesToRemove)
		{
			foreach (var tile in tilesToRemove)
			{
				BoardTiles[tile.Row, tile.Column] = null;
				tilesWaitingToBeRemoved.Add(tile);
				tile.StartDestroySequence();
			}
		}

		/// <summary>
		/// Triggered at the end of tile self-destruct sequence
		/// </summary>
		/// <param name="singleTileManagerCaller"></param>
		public void TileHasFinishedSelfDestructSequence(SingleTileManager singleTileManagerCaller)
		{
			tilesWaitingToBeRemoved.RemoveAt(0);
			singleTileManagerCaller.DeactivateTile();
			PoolManager.Instance.ReturnObjectToPool(singleTileManagerCaller.GetComponent<PoolObject>());

			if (tilesWaitingToBeRemoved.Count > 0)
			{
				return;
			}

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
						// This tile is affected
						tilesAffected.Add((rowIndex, columnIndex));

						// For each of the rows above this null tile
						for (int rowAbove = rowIndex - 1; rowAbove >= -1; rowAbove--)
						{
							// If row above is -1 we need to spawn a tile
							if (rowAbove == -1)
							{
								// Add to number of tiles being dropped in
								tilesFalling++;

								boardTiles[rowIndex, columnIndex]
									= GenerateAndInitializeTile(rowIndex, columnIndex, SizeManager.Instance.GetAboveColumnStartingPosition(columnIndex, indexForTileAbove));

								boardTiles[rowIndex, columnIndex].ActivateTile();

								indexForTileAbove++;
							}
							else
							{
								// If we find a tile
								if (boardTiles[rowAbove, columnIndex] != null)
								{
									// Add to number of tiles being dropped in
									tilesFalling++;

									// Swap in the matrix
									boardTiles[rowIndex, columnIndex] = BoardTiles[rowAbove, columnIndex];
									boardTiles[rowAbove, columnIndex] = null;

									boardTiles[rowIndex, columnIndex].MakeExistingTileDropToNewPosition(rowIndex, columnIndex);

									break;
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Called from a single tile when it has finished dropping
		/// </summary>
		public void TileFinishedDropIn()
		{
			tilesFalling--;

			if (tilesFalling == 0)
			{
				DetermineWhichRowsAndColumnsAreAffectedAndRaiseEvent();
			}
		}

		private SingleTileManager GenerateAndInitializeTile(int i, int j, Vector3? overrideStartPosition = null)
		{
			// Create new tile from the object pool
			PoolObject boardTile = PoolManager.Instance.GetObjectFromPool("tile", boardTilesRoot);

			SingleTileManager singletile = boardTile.GetComponent<SingleTileManager>();

			singletile.InitializeTile(dictionaryService.GetRandomChar(), i, j, overrideStartPosition);

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
					currentlyMovingTile.ResetTileToItsRestingPosition();
				}
				else
				{
					SwapTiles(currentlyMovingTile, tileToSwapWith);

					tilesAffected.Add((currentlyMovingTile.Row, currentlyMovingTile.Column));
					tilesAffected.Add((tileToSwapWith.Row, tileToSwapWith.Column));
					DetermineWhichRowsAndColumnsAreAffectedAndRaiseEvent();
				}
				currentlyMovingTile = null;
			}
		}

		/// <summary>
		/// Returns the rows and columns affected by two tiles, which will be ones that have just been swapped.
		/// </summary>
		/// <param name="tile1"></param>
		/// <param name="tile2"></param>
		private void DetermineWhichRowsAndColumnsAreAffectedAndRaiseEvent()
		{
			List<SingleTileManagerSequence> listOfRowsAndColumnsToCheck = new();

			HashSet<int> rows = new();
			HashSet<int> columns = new();

			foreach (var index in tilesAffected)
			{
				rows.Add(index.Item1);
				columns.Add(index.Item2);
			}

			tilesAffected.Clear();

			foreach (var row in rows)
			{
				listOfRowsAndColumnsToCheck.Add(new SingleTileManagerSequence(GetFullRow(row)));
			}
			foreach (var column in columns)
			{
				listOfRowsAndColumnsToCheck.Add(new SingleTileManagerSequence(GetFullColumn(column)));
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
			boardTiles[tile1.Row, tile1.Column] = tile2;
			boardTiles[tile2.Row, tile2.Column] = tile1;

			// Now set the SingleTileManage settings correctly
			var tile1Row = tile1.Row;
			var tile1Column = tile1.Column;

			tile1.MoveToNewPositionInGrid(tile2.Row, tile2.Column);
			tile2.MoveToNewPositionInGrid(tile1Row, tile1Column);
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
				int indexOfRowAbove = currentlyMovingTile.Row - 1;

				var ratioOfLimit =
					(currentlyMovingTile.transform.position.y - currentlyMovingTile.TileRestingPosition.y)
					/ (currentlyMovingTile.MovementRestrictions.yMax - currentlyMovingTile.TileRestingPosition.y);

				return ratioOfLimit > ratioToSwapTiles ? boardTiles[indexOfRowAbove, currentlyMovingTile.Column] : null;
			}

			// tile went right
			if (vectorFromOriginalPosition.x > 0)
			{
				int indexOfColumnRight = currentlyMovingTile.Column + 1;

				var ratioOfLimit =
				(currentlyMovingTile.transform.position.x - currentlyMovingTile.TileRestingPosition.x)
				/ (currentlyMovingTile.MovementRestrictions.xMax - currentlyMovingTile.TileRestingPosition.x);

				return ratioOfLimit > ratioToSwapTiles ? boardTiles[currentlyMovingTile.Row, indexOfColumnRight] : null;
			}

			// tile went down
			if (vectorFromOriginalPosition.y < 0)
			{
				int indexOfRowBelow = currentlyMovingTile.Row + 1;

				var ratioOfLimit =
				(currentlyMovingTile.TileRestingPosition.y - currentlyMovingTile.transform.position.y)
				/ (currentlyMovingTile.TileRestingPosition.y - currentlyMovingTile.MovementRestrictions.yMin);

				return ratioOfLimit > ratioToSwapTiles ? boardTiles[indexOfRowBelow, currentlyMovingTile.Column] : null;
			}

			// tile went left	
			if (vectorFromOriginalPosition.x < 0)
			{
				int indexOfColumnLeft = currentlyMovingTile.Column - 1;

				var ratioOfLimit =
				(currentlyMovingTile.TileRestingPosition.x - currentlyMovingTile.transform.position.x)
				/ (currentlyMovingTile.TileRestingPosition.x - currentlyMovingTile.MovementRestrictions.xMin);

				return ratioOfLimit > ratioToSwapTiles ? boardTiles[currentlyMovingTile.Row, indexOfColumnLeft] : null;
			}

			return null;
		}
	}
}
