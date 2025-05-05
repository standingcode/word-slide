using Pooling;
using System.Collections;
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
	private Animator animator;

	[SerializeField]
	private float gravitySpeed = 9.87f;

	[SerializeField]
	private BoxCollider boxCollider;

	[SerializeField]
	private bool tileIsActive = false;
	public bool TileIsActive => tileIsActive;

	[SerializeField]
	private string destroyMovementAnimationString;

	private Vector3 tileRestingPosition;
	public Vector3 TileRestingPosition => tileRestingPosition;

	private Quaternion tileRestingRotation;
	public Quaternion TileRestingRotation => tileRestingRotation;

	[SerializeField]
	private SingleTileMover singleTileMover;

	public MovementRestrictions MovementRestrictions => singleTileMover.MovementRestrictions;

	private void Awake()
	{
		tileRestingRotation = transform.rotation;
		DeactivateTile();
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}

	public void InitializeTile(char character, int row, int column)
	{
		SetTileCharacter(character);
		SetTileMatrixIndex(row, column);
		SetTileScale();
		SetTileDefaultPosition();
		SetMovementRestrictions();
	}

	public void SwapTileToNewPosition(int i, int j)
	{
		singleTileMover.StopMoving();
		SetTileMatrixIndex(i, j);
		SetTileDefaultPosition();

		SetTilePosition(tileRestingPosition);
		SetMovementRestrictions();
	}

	public void SetTileDefaultPosition()
	{
		int columnMultiplier = matrixIndex.Item2 % SettingsScriptable.Columns;
		int rowMultiplier = matrixIndex.Item1 % SettingsScriptable.Rows;

		float requiredXPosition =
			SizeManager.Instance.TileSpawnTopLeftStartingPoint.x + (columnMultiplier * (SizeManager.Instance.TileSize.x + SizeManager.Instance.InteriorPaddingSizes.x));

		float requiredYPosition =
			SizeManager.Instance.TileSpawnTopLeftStartingPoint.y - (rowMultiplier * (SizeManager.Instance.TileSize.y + SizeManager.Instance.InteriorPaddingSizes.y));


		tileRestingPosition = new Vector3(requiredXPosition, requiredYPosition, 0f);
	}

	public void SetTilePosition(Vector3 position)
	{
		transform.position = position;
	}

	public IEnumerator AnimateTileToNewPositionCoroutine()
	{
		while (transform.position != TileRestingPosition)
		{
			transform.position = Vector3.MoveTowards(transform.position, TileRestingPosition, Time.deltaTime * gravitySpeed);
			yield return null;
		}
	}

	public void SetTileScale()
	{
		// Set the scales based on the tile size
		transform.localScale = new Vector3(
		SizeManager.Instance.TileSize.x,
		SizeManager.Instance.TileSize.y,
		1f);
	}

	public void ResetTileToOriginalPosition()
	{
		singleTileMover.StopMoving();
		transform.position = tileRestingPosition;
	}

	public void SetTileCharacter(char character)
	{
		tileCharacter = character;

		if (textMesh == null)
		{
			Debug.Log("TextMesh is not assigned in the inspector.");
			return;
		}

		textMesh.text = tileCharacter.ToString().ToUpper();
	}

	// When the tile is first selected, this can be called from the tile manager which should get a reference via a ray	
	public void TileWasClickedOn(Vector2 mousePosition)
	{
		singleTileMover.StartMoving(mousePosition);
	}

	public void ActivateTile()
	{
		if (transform.position != TileRestingPosition)
		{
			StartCoroutine(AnimateTileToNewPositionCoroutine());
		}

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
		transform.rotation = tileRestingRotation;

		StopAllCoroutines();
	}

	public void StartDestroySequence()
	{
		Debug.Log($"Destroy: {TileCharacter}");
		TilesManager.Instance.RemoveTileFromBoard(this);
		animator.SetTrigger(destroyMovementAnimationString);
	}

	public void HighlightAnimationFinished()
	{
		DeactivateTile();
		PoolManager.Instance.ReturnObjectToPool(GetComponent<PoolObject>());
		TilesManager.Instance.NewTilesNeeded(this);
	}

	private void SetTileMatrixIndex(int row, int column)
	{
		matrixIndex = (row, column);
	}

	private void SetMovementRestrictions()
	{
		var movementRestrictions = new MovementRestrictions();

		// X PLAIN

		// If the tile is on the left-hand side of the board, it can only move to the right
		if (matrixIndex.Item2 == 0)
		{
			movementRestrictions.xMin = tileRestingPosition.x;
		}
		// Otherwise the tile can move left one tile size plus padding		
		else
		{
			movementRestrictions.xMin = tileRestingPosition.x - SizeManager.Instance.TileSize.x - SizeManager.Instance.InteriorPaddingSizes.x;
		}


		// if the tile is on the right-hand side of the board, it can only move to the left
		if (matrixIndex.Item2 == SettingsScriptable.Columns - 1)
		{
			movementRestrictions.xMax = tileRestingPosition.x;
		}
		// Otherwise the tile can move right one tile size plus padding
		else
		{
			movementRestrictions.xMax = tileRestingPosition.x + SizeManager.Instance.TileSize.x + SizeManager.Instance.InteriorPaddingSizes.x;
		}

		// Y PLAIN

		// if the tile is on the top of the board, it can only move down
		if (matrixIndex.Item1 == 0)
		{
			movementRestrictions.yMax = tileRestingPosition.y;
		}
		// Otherwise the tile can move up one tile size plus padding
		else
		{
			movementRestrictions.yMax = tileRestingPosition.y + SizeManager.Instance.TileSize.y + SizeManager.Instance.InteriorPaddingSizes.y;
		}


		// if the tile is on the bottom of the board, it can only move up
		if (matrixIndex.Item1 == SettingsScriptable.Rows - 1)
		{
			movementRestrictions.yMin = tileRestingPosition.y;
		}
		// Otherwise the tile can move down one tile size plus padding
		else
		{
			movementRestrictions.yMin = tileRestingPosition.y - SizeManager.Instance.TileSize.y - SizeManager.Instance.InteriorPaddingSizes.y;
		}

		singleTileMover.SetMovementRestrictions(movementRestrictions);
	}
}
