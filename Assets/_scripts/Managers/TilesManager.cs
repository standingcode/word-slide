using Pooling;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
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

		private SingleTileManager currentlyMovingTile = null;

		void Start()
		{
			ClickEventHandler.AddClickDownListener(CheckIfTileWasClicked);
			ClickEventHandler.AddClickUpListener(CheckIfTileNeedsToBeDropped);

			//sizeManager = SizeManager.Instance;
			settings = Settings.Instance;

			// Set the tiles		
			SetTiles();
		}

		//private SizeManager sizeManager;
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

		public void DestroyExistingTiles()
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
					Debug.Log($"{singleTileManager.MatrixIndex}");
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
				currentlyMovingTile.TileShouldBeDropped();
				SwapMovingTileWithThisTile(TileToBeSwappedWithCurrentlyMovingTile());
				currentlyMovingTile = null;
			}
		}

		private void SwapMovingTileWithThisTile(SingleTileManager tileToSwapWithMovingTile)
		{
			if (tileToSwapWithMovingTile == null)
			{
				return;
			}

			Debug.Log($"Swap {currentlyMovingTile.MatrixIndex} and {tileToSwapWithMovingTile.MatrixIndex}");

			// Swap in the matrix
			boardTiles[currentlyMovingTile.MatrixIndex.Item1, currentlyMovingTile.MatrixIndex.Item2] = tileToSwapWithMovingTile;
			boardTiles[tileToSwapWithMovingTile.MatrixIndex.Item1, tileToSwapWithMovingTile.MatrixIndex.Item2] = currentlyMovingTile;

			// Now set the SingleTileManage settings correctly
			var currentlyMovingTileMatrixIndex = currentlyMovingTile.MatrixIndex;
			currentlyMovingTile.MoveTileToNewPosition(tileToSwapWithMovingTile.MatrixIndex.Item1, tileToSwapWithMovingTile.MatrixIndex.Item2);
			tileToSwapWithMovingTile.MoveTileToNewPosition(currentlyMovingTileMatrixIndex.Item1, currentlyMovingTileMatrixIndex.Item2);
		}

		private SingleTileManager TileToBeSwappedWithCurrentlyMovingTile()
		{
			// Check for the 4 tiles around this tile

			// Check the index to make sure there is a row above
			int indexOfRowAbove = currentlyMovingTile.MatrixIndex.Item1 - 1;
			int indexOfRowBelow = currentlyMovingTile.MatrixIndex.Item1 + 1;
			int indexOfColumnLeft = currentlyMovingTile.MatrixIndex.Item2 - 1;
			int indexOfColumnRight = currentlyMovingTile.MatrixIndex.Item2 + 1;

			if (indexOfRowAbove >= 0)
			{
				SingleTileManager tileAbove = boardTiles[indexOfRowAbove, currentlyMovingTile.MatrixIndex.Item2];

				// If moving tile is closer to tile above than its resting position they need swapping
				if (Vector2.Distance(currentlyMovingTile.transform.position, tileAbove.transform.position)
				< Vector2.Distance(currentlyMovingTile.transform.position, currentlyMovingTile.TileRestingPosition))
				{
					return tileAbove;
				}
			}

			if (indexOfRowBelow < settings.Rows)
			{
				SingleTileManager tileBelow = boardTiles[indexOfRowBelow, currentlyMovingTile.MatrixIndex.Item2];

				// If moving tile is closer to tile below than its resting position they need swapping
				if (Vector2.Distance(currentlyMovingTile.transform.position, tileBelow.transform.position)
				< Vector2.Distance(currentlyMovingTile.transform.position, currentlyMovingTile.TileRestingPosition))
				{
					return tileBelow;
				}
			}

			if (indexOfColumnLeft >= 0)
			{
				SingleTileManager tileToTheLeft = boardTiles[currentlyMovingTile.MatrixIndex.Item1, indexOfColumnLeft];

				// If moving tile is closer to tile to the left than its resting position they need swapping
				if (Vector2.Distance(currentlyMovingTile.transform.position, tileToTheLeft.transform.position)
				< Vector2.Distance(currentlyMovingTile.transform.position, currentlyMovingTile.TileRestingPosition))
				{
					return tileToTheLeft;
				}
			}

			if (indexOfColumnRight < settings.Columns)
			{
				SingleTileManager tileToTheRight = boardTiles[currentlyMovingTile.MatrixIndex.Item1, indexOfColumnRight];

				// If moving tile is closer to tile to the right than its resting position they need swapping
				if (Vector2.Distance(currentlyMovingTile.transform.position, tileToTheRight.transform.position)
				< Vector2.Distance(currentlyMovingTile.transform.position, currentlyMovingTile.TileRestingPosition))
				{
					return tileToTheRight;
				}
			}

			return null;
		}
	}
}
