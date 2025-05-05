using Pooling;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
		private ClickEventHandler ClickEventHandler;

		[SerializeField]
		private TileSwappedEventHandler TileSwappedEventHandler;

		[SerializeField]
		private float ratioToSwapTiles = 0.7f;

		private SingleTileManager currentlyMovingTile = null;

		public static TilesManager Instance { get; private set; }

		private Settings settings => Settings.Instance;

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
			GenerateFullTileBoard();

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
		/// Sets the tiles which will be seen initially on the screen.
		/// </summary>
		private void GenerateFullTileBoard()
		{
			DestroyExistingTiles();

			boardTiles = new SingleTileManager[settings.Rows, settings.Columns];

			for (int i = 0; i < boardTiles.GetLength(0); i++)
			{
				for (int j = 0; j < boardTiles.GetLength(1); j++)
				{
					// Create new tile from the object pool
					PoolObject boardTile = PoolManager.Instance.GetObjectFromPool("tile", boardTilesRoot);

					SingleTileManager singletile = boardTile.GetComponent<SingleTileManager>();

					singletile.InitializeTile(dictionaryService.GetRandomChar(), i, j);
					boardTiles[i, j] = singletile;
				}
			}
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
			tile1.MoveTileToNewPosition(tile2.MatrixIndex.Item1, tile2.MatrixIndex.Item2);
			tile2.MoveTileToNewPosition(tile1MatrixIndex.Item1, tile1MatrixIndex.Item2);
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
