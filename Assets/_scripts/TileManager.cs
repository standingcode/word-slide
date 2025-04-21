using Pooling;
using System;
using UnityEngine;
using Zenject;

namespace WordSlide
{
	public class TileManager : MonoBehaviour
	{
		[Inject]
		private IDictionaryManager dictionaryManager;

		private SingleTile[,] boardTiles;
		private SingleTile[,] waitingTiles;

		void Start()
		{
			sizeManager = SizeManager.Instance;
			manager = Manager.Instance;

			// Set initial tiles		
			SetInitialTiles();
		}

		private SizeManager sizeManager;
		private Manager manager;

		/// <summary>
		/// Sets the initial tiles
		/// </summary>
		private void SetInitialTiles()
		{
			SetWaitingTiles();
			SetBoardTiles();
		}

		private void SetWaitingTiles()
		{
			Debug.LogError("Please implement waiting tiles");
		}

		//private SingleTile[,] getSetOfTiles(bool setCannotContainValidWord)
		//{


		//}

		/// <summary>
		/// Sets the tiles which will be seen initially on the screen.
		/// </summary>
		private void SetBoardTiles()
		{
			for (int i = 0; i < manager.Rows; i++)
			{
				for (int j = 0; j < manager.Columns; j++)
				{
					// Create a new tile from the object pool
					PoolObject tile = Manager.PoolManager.GetObjectFromPool("tile");

					// Set the transfrom as a child of this transform.
					tile.transform.parent = transform;

					// Set the scale based on the tile size
					tile.transform.localScale = new Vector3(sizeManager.TileSize.x, sizeManager.TileSize.y, 1f);

					// Set the position of the tile
					SetSingleTilePosition(
						i,
						j,
						tile.transform, sizeManager.TileSize,
						sizeManager.InteriorPaddingSizes,
						sizeManager.TileSpawnTopLeftStartingPoint
					);

					// Set the character of the tile to a random character
					SingleTile singleTile = tile.GetComponent<SingleTile>();

					if (singleTile != null)
					{
						SetSingleTileCharacterToRandomCharacter(singleTile);
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
			int columnMultiplier = j % manager.Columns;
			int rowMultiplier = i % manager.Rows;

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
