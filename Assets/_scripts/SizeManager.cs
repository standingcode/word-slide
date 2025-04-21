using UnityEngine;
using UnityEngine.UIElements;

namespace WordSlide
{
	public class SizeManager : MonoBehaviour
	{
		private Vector2? boardSize = null;
		public Vector2 BoardSize
		{
			get
			{
				if (boardSize == null)
				{
					boardSize = GetBoardSize();
					return boardSize.Value;
				}
				else
				{
					return boardSize.Value;
				}
			}
		}

		private Vector2? tileSize = null;
		public Vector2 TileSize
		{
			get
			{
				if (tileSize == null)
				{
					(tileSize, interiorPaddingSizes) = GetTileSizeAndInteriorPaddingSize(BoardSize);
					return tileSize.Value;
				}
				else
				{
					return tileSize.Value;
				}
			}
		}

		private Vector2? interiorPaddingSizes = null;
		public Vector2 InteriorPaddingSizes
		{
			get
			{
				if (interiorPaddingSizes == null)
				{
					(tileSize, interiorPaddingSizes) = GetTileSizeAndInteriorPaddingSize(BoardSize);
					return interiorPaddingSizes.Value;
				}
				else
				{
					return interiorPaddingSizes.Value;
				}
			}
		}

		private Vector2? tileSpawnTopLeftStartingPoint = null;
		public Vector2 TileSpawnTopLeftStartingPoint
		{
			get
			{
				if (tileSpawnTopLeftStartingPoint == null)
				{
					tileSpawnTopLeftStartingPoint = GetTileSpawnTopLeftStartingPoint(TileSize, BoardSize);
					return tileSpawnTopLeftStartingPoint.Value;
				}
				else
				{
					return tileSpawnTopLeftStartingPoint.Value;
				}
			}
		}

		public static SizeManager Instance { get; private set; }

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
				return;
			}

			Instance = this;
		}

		[SerializeField]
		private float tilePaddingRatio = 0.1f, minimumMarginFromBoardAsRatio = 0.05f;

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
		/// The method calculates the tile sizes and and interior padding sizes
		/// </summary>
		/// <param name="boardSize"></param>
		/// <returns>The tile sizes x and y and the interior margin sizes x and y</returns>
		private (Vector2, Vector2) GetTileSizeAndInteriorPaddingSize(Vector2 boardSize)
		{
			// Step 1: Calculate the initial tile size based on board dimensions and grid layout  
			float initialTileSizeX = boardSize.x / Manager.Instance.Columns;
			float initialTileSizeY = boardSize.y / Manager.Instance.Rows;

			// Step 2: Calculate the interior padding as a ratio of the initial tile size  
			float xInteriorPadding = initialTileSizeX * tilePaddingRatio;
			float yInteriorPadding = initialTileSizeY * tilePaddingRatio;

			// Step 3: Adjust the tile size to account for the interior padding  
			float adjustedTileSizeX = initialTileSizeX - (xInteriorPadding * (Manager.Instance.Columns - 1) / Manager.Instance.Columns);
			float adjustedTileSizeY = initialTileSizeY - (yInteriorPadding * (Manager.Instance.Rows - 1) / Manager.Instance.Rows);

			return (new Vector2(adjustedTileSizeX, adjustedTileSizeY), new Vector2(xInteriorPadding, yInteriorPadding));
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
	}
}
