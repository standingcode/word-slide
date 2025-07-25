using UnityEngine;

namespace WordSlide
{
	public class SizeManager : MonoBehaviour
	{
		private Vector2 boardSize;
		public Vector2 BoardSize => boardSize;

		private Vector2 tileSize;
		public Vector2 TileSize => tileSize;

		private Vector2 interiorPaddingSizes;
		public Vector2 InteriorPaddingSizes => interiorPaddingSizes;

		private Vector3 tileSpawnTopLeftStartingPoint;
		public Vector3 TileSpawnTopLeftStartingPoint => tileSpawnTopLeftStartingPoint;

		private Vector3[,] tileSpawnPositions;
		public Vector3[,] TileSpawnPositions => tileSpawnPositions;

		private Vector2 worldUnitsInCamera = Vector2.zero;
		public Vector2 WorldUnitsInCamera => worldUnitsInCamera;

		private SettingsScriptable settings => MainMenuManager.Instance.SettingsScriptable;

		public static SizeManager Instance { get; private set; }

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
				return;
			}

			Instance = this;

			SetSizes();
		}

		private void OnDestroy()
		{
			Instance = null;
		}

		public Vector3 GetAboveColumnStartingPosition(int columnIndex, int heightIndex)
		{
			float xPos = tileSpawnTopLeftStartingPoint.x + (columnIndex * (tileSize.x + interiorPaddingSizes.x));
			float yPos = tileSpawnTopLeftStartingPoint.y + tileSize.y + interiorPaddingSizes.y + (heightIndex * (tileSize.y + interiorPaddingSizes.y));

			return new Vector3(xPos, yPos, 0f);
		}

		private void SetSizes()
		{
			SetBoardSize();
			SetTileSizeAndInteriorPaddingSize(boardSize);
			SetTileSpawnTopLeftStartingPoint(tileSize, boardSize);
			SetTileSpawnPositions();
		}

		/// <summary>
		/// Determine the board size, based on the screen's narrowest side and minus the exterior margin required.
		/// </summary>
		private void SetBoardSize()
		{
			worldUnitsInCamera = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)) * 2;

			float sizeForWidthAndHeight;

			if (ScreenIsLandscape())
			{
				sizeForWidthAndHeight = WorldUnitsInCamera.y - (WorldUnitsInCamera.y * (settings.MinimumMarginFromBoardAsRatio * 2));
			}
			else
			{
				sizeForWidthAndHeight = WorldUnitsInCamera.x - (WorldUnitsInCamera.x * (settings.MinimumMarginFromBoardAsRatio * 2));
			}

			boardSize = new Vector2(sizeForWidthAndHeight, sizeForWidthAndHeight);
		}

		/// <summary>
		/// The method calculates the tile sizes and and interior padding sizes
		/// </summary>
		/// <param name="boardSize"></param>		
		private void SetTileSizeAndInteriorPaddingSize(Vector2 boardSize)
		{
			// Step 1: Calculate the initial tile size based on board dimensions and grid layout  
			float initialTileSizeX = boardSize.x / settings.Columns;
			float initialTileSizeY = boardSize.y / settings.Rows;

			// Step 2: Calculate the interior padding as a ratio of the initial tile size  
			float xInteriorPadding = initialTileSizeX * settings.TilePaddingRatio;
			float yInteriorPadding = initialTileSizeY * settings.TilePaddingRatio;

			// Step 3: Adjust the tile size to account for the interior padding  
			float adjustedTileSizeX = initialTileSizeX - (xInteriorPadding * (settings.Columns - 1) / settings.Columns);
			float adjustedTileSizeY = initialTileSizeY - (yInteriorPadding * (settings.Rows - 1) / settings.Rows);

			tileSize = new Vector2(adjustedTileSizeX, adjustedTileSizeY);
			interiorPaddingSizes = new Vector2(xInteriorPadding, yInteriorPadding);
		}

		/// <summary>
		/// Determine the tile top left starting point. Board size is passed, which already takes into account exterior margins.
		/// </summary>
		/// <param name="tileSize"></param>
		/// <param name="boardSize"></param>
		private void SetTileSpawnTopLeftStartingPoint(Vector2 tileSize, Vector2 boardSize)
		{
			// 0 position - (0.5 * the width) + (0.5 * one tile width) should be the x position
			float startingPointX = 0 - (0.5f * boardSize.x) + (0.5f * tileSize.x);

			// 0 position - (0.5 * the height) + (0.5 * one tile height)  should be the y position
			float startingPointY = 0 + (0.5f * boardSize.y) - (0.5f * tileSize.y);

			tileSpawnTopLeftStartingPoint = new Vector3(startingPointX, startingPointY, 0f);
		}

		/// <summary>
		/// Initializes and calculates the spawn positions for tiles based on the grid dimensions and layout settings.
		/// </summary>		
		private void SetTileSpawnPositions()
		{
			tileSpawnPositions = new Vector3[settings.Rows, settings.Columns];

			for (int i = 0; i < settings.Rows; i++)
			{
				for (int j = 0; j < settings.Columns; j++)
				{
					int rowMultiplier = i % settings.Rows;
					int columnMultiplier = j % settings.Columns;

					float requiredXPosition =
						tileSpawnTopLeftStartingPoint.x + (columnMultiplier * (tileSize.x + interiorPaddingSizes.x));

					float requiredYPosition =
						tileSpawnTopLeftStartingPoint.y - (rowMultiplier * (tileSize.y + interiorPaddingSizes.y));

					tileSpawnPositions[i, j] = new Vector3(requiredXPosition, requiredYPosition, 0f);
				}
			}
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
