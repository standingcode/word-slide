using Pooling;
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
					SwapMovingTileWithThisTile(tileToSwapWith);
				}
				currentlyMovingTile = null;
			}
		}

		private void SwapMovingTileWithThisTile(SingleTileManager tileToSwapWithMovingTile)
		{
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
