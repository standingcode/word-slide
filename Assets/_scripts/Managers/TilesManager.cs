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
		private IDictionaryManager dictionaryManager;

		[SerializeField]
		private SingleTileManager[,] boardTiles;

		[SerializeField]
		private SingleTileManager[,] waitingTiles;

		[SerializeField]
		private Transform boardTilesRoot, waitingTilesRoot;

		[SerializeField]
		private ClickEventHandler ClickEventHandler;

		[SerializeField]
		private TileSwappedEventHandler TileSwappedEventHandler;

		[SerializeField]
		private float ratioToSwapTiles = 0.7f;

		private SingleTileManager currentlyMovingTile = null;

		public TilesManager Instance { get; private set; }

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
				Debug.LogError("TilesManager instance already exists. Destroying the new instance.");
				return;
			}
		}

		private void Start()
		{
			ClickEventHandler.AddClickDownListener(CheckIfTileWasClicked);
			ClickEventHandler.AddClickUpListener(CheckIfTileNeedsToBeDropped);

			//sizeManager = SizeManager.Instance;
			settings = Settings.Instance;

			// Set the tiles		
			SetTiles();
		}

		private Settings settings;

		/// <summary>
		/// Sets the tiles which will be seen initially on the screen.
		/// </summary>
		public void SetTiles()
		{
			DestroyExistingTiles();

			boardTiles = new SingleTileManager[settings.Rows, settings.Columns];
			waitingTiles = new SingleTileManager[settings.Rows, settings.Columns];

			for (int i = 0; i < settings.Rows; i++)
			{
				for (int j = 0; j < settings.Columns; j++)
				{
					// Create 2 new tiles from the object pool
					PoolObject boardTile = PoolManager.Instance.GetObjectFromPool("tile", boardTilesRoot);
					PoolObject waitingTile = PoolManager.Instance.GetObjectFromPool("tile", waitingTilesRoot);

					SingleTileManager boardSingleTile = boardTile.GetComponent<SingleTileManager>();
					SingleTileManager waitingSingleTile = waitingTile.GetComponent<SingleTileManager>();

					boardSingleTile.InitializeTile(dictionaryManager.GetRandomChar(), i, j);
					boardSingleTile.ActivateTile();
					boardTiles[i, j] = boardSingleTile;

					waitingSingleTile.InitializeTile(dictionaryManager.GetRandomChar(), i, j);
					waitingTiles[i, j] = waitingSingleTile;
				}
			}
		}

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

			if (waitingTiles != null)
			{
				foreach (SingleTileManager tile in waitingTiles)
				{
					if (tile != null)
					{
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
					SwapMovingTileWithThisTile(currentlyMovingTile, tileToSwapWith);
					DetermineWhichRowsAndColumnsAreAffectedAndRaiseEvent(currentlyMovingTile, tileToSwapWith);
				}
				currentlyMovingTile = null;
			}
		}

		private void DetermineWhichRowsAndColumnsAreAffectedAndRaiseEvent(SingleTileManager tile1, SingleTileManager tile2)
		{
			List<SingleTileManagersRepresentingAString> listOfRowsAndColumnsToCheck = new();

			// Determine which rows and columns were affected by the swap	
			if (tile1.MatrixIndex.Item1 == tile2.MatrixIndex.Item1)
			{
				listOfRowsAndColumnsToCheck = new List<SingleTileManagersRepresentingAString>
				{
					new SingleTileManagersRepresentingAString(GetFullRow(tile1.MatrixIndex.Item1)),
					new SingleTileManagersRepresentingAString(GetFullColumn(tile1.MatrixIndex.Item2)),
					new SingleTileManagersRepresentingAString(GetFullColumn(tile2.MatrixIndex.Item2))
				};
			}
			else
			{
				listOfRowsAndColumnsToCheck = new List<SingleTileManagersRepresentingAString>
				{
					new SingleTileManagersRepresentingAString(GetFullRow(tile1.MatrixIndex.Item1)),
					new SingleTileManagersRepresentingAString(GetFullRow(tile2.MatrixIndex.Item1)),
					new SingleTileManagersRepresentingAString(GetFullColumn(tile1.MatrixIndex.Item2))
				};
			}

			TileSwappedEventHandler.RaiseTileSwapped(listOfRowsAndColumnsToCheck);
		}

		private SingleTileManager[] GetFullRow(int index)
		{
			var row = new List<SingleTileManager>();

			for (int i = 0; i < boardTiles.GetLength(1); i++)
			{
				row.Add(boardTiles[index, i]);
			}

			return row.ToArray();
		}

		private SingleTileManager[] GetFullColumn(int index)
		{
			var column = new List<SingleTileManager>();

			for (int i = 0; i < boardTiles.GetLength(0); i++)
			{
				column.Add(boardTiles[i, index]);
			}

			return column.ToArray();
		}

		private void SwapMovingTileWithThisTile(SingleTileManager tile1, SingleTileManager tile2)
		{
			// Swap in the matrix
			boardTiles[tile1.MatrixIndex.Item1, tile1.MatrixIndex.Item2] = tile2;
			boardTiles[tile2.MatrixIndex.Item1, tile2.MatrixIndex.Item2] = tile1;

			// Now set the SingleTileManage settings correctly
			var tile1MatrixIndex = tile1.MatrixIndex;
			tile1.MoveTileToNewPosition(tile2.MatrixIndex.Item1, tile2.MatrixIndex.Item2);
			tile2.MoveTileToNewPosition(tile1MatrixIndex.Item1, tile1MatrixIndex.Item2);
		}

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
