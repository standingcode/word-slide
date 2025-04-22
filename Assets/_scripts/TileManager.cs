using Pooling;
using UnityEngine;
using Zenject;

namespace WordSlide
{
	public class TileManager : MonoBehaviour
	{
		[Inject]
		private IDictionaryManager dictionaryManager;

		[SerializeField]
		private SingleTile[,] boardTiles;

		[SerializeField]
		private SingleTile[,] waitingTiles;

		[SerializeField]
		private Transform boardTilesRoot, waitingTilesRoot;

		void Start()
		{
			sizeManager = SizeManager.Instance;
			settings = Settings.Instance;

			// Set initial tiles		
			SetInitialTiles();
		}

		private SizeManager sizeManager;
		private Settings settings;

		/// <summary>
		/// Sets the initial tiles
		/// </summary>
		private void SetInitialTiles()
		{
			SetTiles();
		}

		private void SetWaitingTiles()
		{
			Debug.LogError("Please implement waiting tiles");
		}

		/// <summary>
		/// Sets the tiles which will be seen initially on the screen.
		/// </summary>
		private void SetTiles()
		{
			boardTiles = new SingleTile[settings.Rows, settings.Columns];
			waitingTiles = new SingleTile[settings.Rows, settings.Columns];

			for (int i = 0; i < settings.Rows; i++)
			{
				for (int j = 0; j < settings.Columns; j++)
				{
					// Create 2 new tiles from the object pool
					PoolObject boardTile = PoolManager.Instance.GetObjectFromPool("tile");
					PoolObject waitingTile = PoolManager.Instance.GetObjectFromPool("tile");

					boardTile.transform.parent = boardTilesRoot;
					waitingTile.transform.parent = waitingTilesRoot;

					// Set the scales based on the tile size
					waitingTile.transform.localScale = boardTile.transform.localScale = new Vector3(
					sizeManager.TileSize.x,
					sizeManager.TileSize.y,
					1f);

					// Set the position of the board tile
					SetSingleTilePosition(
						i,
						j,
						boardTile.transform, sizeManager.TileSize,
						sizeManager.InteriorPaddingSizes,
						sizeManager.TileSpawnTopLeftStartingPoint
					);

					// Set the character of the tiles to a random character
					SingleTile boardSingleTile = boardTile.GetComponent<SingleTile>();
					SingleTile waitingSingleTile = waitingTile.GetComponent<SingleTile>();

					// For the board tile
					if (boardSingleTile != null)
					{
						SetSingleTileCharacterToRandomCharacter(boardSingleTile);
						boardTiles[i, j] = boardSingleTile;
						boardSingleTile.ActivateTile();
					}
					else
					{
						Debug.LogError("SingleTile component not found on the tile prefab.");
					}

					// For the waiting tile
					if (waitingSingleTile != null)
					{
						SetSingleTileCharacterToRandomCharacter(boardSingleTile);
						waitingTiles[i, j] = boardSingleTile;
					}
					else
					{
						Debug.LogError("SingleTile component not found on the tile prefab.");
					}
				}
			}
		}

		/// <summary>
		/// Sets a single tile position for the given index within a martix (i,j) the tile transform, 
		/// the tile size, the tile padding and the top left starting point
		/// </summary>
		/// <param name="i">Index of the row</param>
		/// <param name="j">Index of the column</param>
		/// <param name="tile">The transform of the tile to be positioned</param>
		/// <param name="tileSize">The size of the tiles</param>
		/// <param name="tilePadding">The padding between the tiles</param>
		/// <param name="tileSpawnTopLeftStartingPoint">The top left starting point of the board</param>
		private void SetSingleTilePosition(int i, int j, Transform tile, Vector2 tileSize, Vector2 tilePadding, Vector2 tileSpawnTopLeftStartingPoint)
		{
			int columnMultiplier = j % settings.Columns;
			int rowMultiplier = i % settings.Rows;

			float requiredXPosition = tileSpawnTopLeftStartingPoint.x + (columnMultiplier * (tileSize.x + tilePadding.x));
			float requiredYPosition = tileSpawnTopLeftStartingPoint.y - (rowMultiplier * (tileSize.y + tilePadding.y));

			tile.transform.position = new Vector3(requiredXPosition, requiredYPosition, 0);
		}

		private void SetSingleTileCharacterToRandomCharacter(SingleTile singleTile)
		{
			singleTile.SetShownCharacter(dictionaryManager.GetRandomChar());
		}
	}
}
