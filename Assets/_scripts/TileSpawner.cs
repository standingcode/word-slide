using UnityEngine;

public class TileSpawner : MonoBehaviour
{
	[SerializeField]
	private float tileWidth = 1.0f, tileHeight = 1.0f;

	[SerializeField]
	private int columns = 10, rows = 10;

	[SerializeField]
	private float tileMargin = 0.1f;

	void Start()
	{
		// Set initial tiles
		SetInitialTiles();

		DetectScreenSizeChange.ScreenSizeChanged += ScaleToScreensize;
	}

	private void ScaleToScreensize(Vector2 differenceInScale)
	{
		transform.localScale = new Vector3(differenceInScale.x, differenceInScale.y, 1);
	}

	private void SetInitialTiles()
	{
		//Debug.Log("Yea I'm spawning");

		// Work out the width
		float boardWidth = (columns * tileWidth) + ((columns - 1) * tileMargin);

		// 0 position - (0.5 * the width) + (0.5 * one tile width) should be the x position
		float startingPointX = 0 - (0.5f * boardWidth) + (0.5f * tileWidth);

		// Work out the height
		float boardHeight = (rows * tileHeight) + ((rows - 1) * tileMargin);

		// 0 position - (0.5 * the height) + (0.5 * one tile height)  should be the y position
		float startingPointY = 0 + (0.5f * boardHeight) - (0.5f * tileHeight);

		//Debug.Log($"width: {boardWidth}, height:{boardHeight}, x: {startingPointX}, y: {startingPointY}");

		Vector3 endPoint = Vector3.zero;

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < columns; j++)
			{
				int columnMultiplier = j % columns;
				int rowMultiplier = i % rows;

				float requiredXPosition = startingPointX + (columnMultiplier * (tileWidth + tileMargin));
				float requiredYPosition = startingPointY - (rowMultiplier * (tileHeight + tileMargin));

				// Create a new tile
				PoolObject tile = Manager.PoolManager.GetObjectFromPool("tile");

				tile.transform.parent = transform;

				// Set the position
				tile.transform.position = new Vector3(requiredXPosition, requiredYPosition, 0);
			}
		}
	}
}
