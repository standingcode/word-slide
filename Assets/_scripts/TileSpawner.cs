using Pooling;
using UnityEngine;

namespace WordSlide
{
	public class TileSpawner : MonoBehaviour
	{
		[SerializeField]
		private int columns = 10, rows = 10;

		[SerializeField]
		private float tilePaddingRatio = 0.1f, minimumMarginFromBoardAsRatio = 0.05f;

		void Start()
		{
			// Set initial tiles
			SetInitialTiles();
		}

		/// <summary>
		/// Determine the board size, based on the screen's narrowest side and minus the exterior margin required.
		/// </summary>
		/// <returns>Vector2 of the board size x and y. Currently just using a square.</returns>
		private Vector2 GetBoardSize()
		{
			float sizeForWidthAndHeight;
			float orthographicVertical = Camera.main.orthographicSize * 2;

			if (ScreenIsLandscape())
			{
				// The camera orphograpic size is vertical, so we can use that as the max size of the board
				sizeForWidthAndHeight = orthographicVertical - (orthographicVertical * (minimumMarginFromBoardAsRatio * 2));
			}
			else
			{
				// The camera orphographic size is vertical, we need to determine the width first using ratios
				Debug.Log(Screen.width);
				Debug.Log(Screen.height);
				float ratio = (float)Screen.width / (float)Screen.height;
				float orthographicSize = ratio * orthographicVertical;
				sizeForWidthAndHeight = orthographicSize - (orthographicSize * (minimumMarginFromBoardAsRatio * 2));
			}

			return new Vector2(sizeForWidthAndHeight, sizeForWidthAndHeight);
		}

		/// <summary>
		/// Determine the tile top left starting point. Board size is passed, which already takes into account exterior margins.
		/// </summary>
		/// <param name="tileSize"></param>
		/// <param name="boardSize"></param>
		/// <returns>Vector2 representing the starting point in 2D of the top left tile</returns>
		private Vector2 GetTileSpawnTopLeftStartingPoint(Vector2 tileSize, Vector2 boardSize)
		{
			// 0 position - (0.5 * the width) + (0.5 * one tile width) should be the x position
			float startingPointX = 0 - (0.5f * boardSize.x) + (0.5f * tileSize.x);

			// 0 position - (0.5 * the height) + (0.5 * one tile height)  should be the y position
			float startingPointY = 0 + (0.5f * boardSize.y) - (0.5f * tileSize.y);

			return new Vector2(startingPointX, startingPointY);
		}

		/// <summary>
		/// Determine if screen is portrait or landscape
		/// </summary>
		/// <returns>bool true if screen is landscape or a square screen, false if screen is portrait.</returns>
		private bool ScreenIsLandscape()
		{
			return Screen.width >= Screen.height;
		}

		/// <summary>
		/// The method calculates the tile sizes and and interior padding sizes
		/// </summary>
		/// <param name="boardSize"></param>
		/// <returns>The tile sizes x and y and the interio margin sizes x and y</returns>
		private (Vector2, Vector2) GetTileSizeAndInterioPaddingSize(Vector2 boardSize)
		{
			// Step 1: Calculate the initial tile size based on board dimensions and grid layout  
			float initialTileSizeX = boardSize.x / columns;
			float initialTileSizeY = boardSize.y / rows;

			// Step 2: Calculate the interior padding as a ratio of the initial tile size  
			float xInteriorPadding = initialTileSizeX * tilePaddingRatio;
			float yInteriorPadding = initialTileSizeY * tilePaddingRatio;

			// Step 3: Adjust the tile size to account for the interior padding  
			float adjustedTileSizeX = initialTileSizeX - (xInteriorPadding * (columns - 1) / columns);
			float adjustedTileSizeY = initialTileSizeY - (yInteriorPadding * (rows - 1) / rows);

			return (new Vector2(adjustedTileSizeX, adjustedTileSizeY), new Vector2(xInteriorPadding, yInteriorPadding));
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
			int columnMultiplier = j % columns;
			int rowMultiplier = i % rows;

			float requiredXPosition = tileSpawnTopLeftStartingPoint.x + (columnMultiplier * (tileSize.x + tilePadding.x));
			float requiredYPosition = tileSpawnTopLeftStartingPoint.y - (rowMultiplier * (tileSize.y + tilePadding.y));

			tile.transform.position = new Vector3(requiredXPosition, requiredYPosition, 0);
		}

		/// <summary>
		/// Sets the initial tiles, we may have to do something later to cope with changing screen resolutions etc.
		/// </summary>
		private void SetInitialTiles()
		{
			// Baord size is determined from the orthogrpahic camera size
			Vector2 boardSize = GetBoardSize();

			// Tile size and margin size must be determined at the same time.
			// The reason being that tiles are boardsize / rows and columns, but then we need to reduce the size to allow
			// for margins which are a ratio of the tile size.
			(Vector2 tileSize, Vector2 interiorPaddingSizes) = GetTileSizeAndInterioPaddingSize(boardSize);

			Vector2 tileSpawnTopLeftStartingPoint = GetTileSpawnTopLeftStartingPoint(tileSize, boardSize);

			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < columns; j++)
				{
					// Create a new tile from the object pool
					PoolObject tile = Manager.PoolManager.GetObjectFromPool("tile");

					// Set the transfrom as a child of this transform.
					tile.transform.parent = transform;

					// Set the scale based on the tile size
					tile.transform.localScale = new Vector3(tileSize.x, tileSize.y, 1f);

					// Set the position of the tile
					SetSingleTilePosition(i, j, tile.transform, tileSize, interiorPaddingSizes, tileSpawnTopLeftStartingPoint);
				}
			}
		}
	}
}
