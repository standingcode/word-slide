using TMPro;
using UnityEngine;
using WordSlide;

public class SingleTileManager : MonoBehaviour
{
	[SerializeField]
	private char tileCharacter;
	public char TileCharacter => tileCharacter;

	[SerializeField]
	private (int, int) matrixIndex;
	public (int, int) MatrixIndex => matrixIndex;

	[SerializeField]
	private TextMeshProUGUI textMesh;

	[SerializeField]
	private Renderer meshRenderer;

	[SerializeField]
	private BoxCollider boxCollider;

	[SerializeField]
	private bool tileIsActive = false;
	public bool TileIsActive => tileIsActive;

	[SerializeField]
	private SingleTileMover singleTileMover;

	private Vector2 tileRestingPosition;
	public Vector2 TileRestingPosition => tileRestingPosition;

	public void Awake()
	{
		DeactivateTile();
	}

	public void InitializeTile(char character, int row, int column)
	{
		SetTileShownCharacter(character);
		SetTileMatrixIndex(row, column);
		SetTileScale();
		SetTileDefaultPosition();
		SetMovementRestrictions();
	}

	private void SetTileScale()
	{
		// Set the scales based on the tile size
		transform.localScale = new Vector3(
		SizeManager.Instance.TileSize.x,
		SizeManager.Instance.TileSize.y,
		1f);
	}

	private void SetTileDefaultPosition()
	{
		int columnMultiplier = matrixIndex.Item2 % Settings.Instance.Columns;
		int rowMultiplier = matrixIndex.Item1 % Settings.Instance.Rows;

		float requiredXPosition =
			SizeManager.Instance.TileSpawnTopLeftStartingPoint.x + (columnMultiplier * (SizeManager.Instance.TileSize.x + SizeManager.Instance.InteriorPaddingSizes.x));

		float requiredYPosition =
			SizeManager.Instance.TileSpawnTopLeftStartingPoint.y - (rowMultiplier * (SizeManager.Instance.TileSize.y + SizeManager.Instance.InteriorPaddingSizes.y));


		tileRestingPosition = new Vector3(requiredXPosition, requiredYPosition, 0f);
		transform.position = tileRestingPosition;
	}

	public void MoveTileToNewPosition(int i, int j)
	{
		SetTileMatrixIndex(i, j);
		SetTileDefaultPosition();
		SetMovementRestrictions();
	}

	public void SetTileShownCharacter(char character)
	{
		tileCharacter = character;
		textMesh.text = tileCharacter.ToString().ToUpper();
	}

	public void SetTileMatrixIndex(int row, int column)
	{
		matrixIndex = (row, column);
	}

	public void SetMovementRestrictions()
	{
		var movementRestrictions = new MovementRestrictions();

		// X PLAIN

		// If the tile is on the left-hand side of the board, it can only move to the right
		if (matrixIndex.Item2 == 0)
		{
			movementRestrictions.xMin = transform.position.x;
		}
		// Otherwise the tile can move left one tile size plus padding		
		else
		{
			movementRestrictions.xMin = transform.position.x - SizeManager.Instance.TileSize.x - SizeManager.Instance.InteriorPaddingSizes.x;
		}


		// if the tile is on the right-hand side of the board, it can only move to the left
		if (matrixIndex.Item2 == Settings.Instance.Columns - 1)
		{
			movementRestrictions.xMax = transform.position.x;
		}
		// Otherwise the tile can move right one tile size plus padding
		else
		{
			movementRestrictions.xMax = transform.position.x + SizeManager.Instance.TileSize.x + SizeManager.Instance.InteriorPaddingSizes.x;
		}

		// Y PLAIN

		// if the tile is on the top of the board, it can only move down
		if (matrixIndex.Item1 == 0)
		{
			movementRestrictions.yMax = transform.position.y;
		}
		// Otherwise the tile can move up one tile size plus padding
		else
		{
			movementRestrictions.yMax = transform.position.y + SizeManager.Instance.TileSize.y + SizeManager.Instance.InteriorPaddingSizes.y;
		}


		// if the tile is on the bottom of the board, it can only move up
		if (matrixIndex.Item1 == Settings.Instance.Rows - 1)
		{
			movementRestrictions.yMin = transform.position.y;
		}
		// Otherwise the tile can move down one tile size plus padding
		else
		{
			movementRestrictions.yMin = transform.position.y - SizeManager.Instance.TileSize.y - SizeManager.Instance.InteriorPaddingSizes.y;
		}

		singleTileMover.SetMovementRestrictions(movementRestrictions);
	}

	// When the tile is first selected, this can be called from the tile manager which should get a reference via a ray	
	public void TileWasClickedOn(Vector2 mousePosition)
	{
		singleTileMover.StartMoving(mousePosition);
	}

	public void TileShouldBeDropped()
	{
		singleTileMover.StopMoving();
	}

	public void ActivateTile()
	{
		tileIsActive = true;
		boxCollider.enabled = true;
		meshRenderer.enabled = true;
		textMesh.enabled = true;
	}

	public void DeactivateTile()
	{
		tileIsActive = false;
		boxCollider.enabled = false;
		meshRenderer.enabled = false;
		textMesh.enabled = false;
	}
}
