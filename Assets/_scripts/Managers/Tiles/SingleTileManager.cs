using Pooling;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using WordSlide;

public class SingleTileManager : MonoBehaviour
{
	[SerializeField]
	private char tileCharacter;
	public char TileCharacter => tileCharacter;

	[SerializeField]
	private int _row, _column;
	public int Row => _row;
	public int Column => _column;

	[SerializeField]
	private TextMeshProUGUI textMesh;

	[SerializeField]
	private GameObject visualCube;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private float gravitySpeed => SettingsScriptable.GravitySpeed;

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

	[SerializeField]
	private TileEventHandler tileEventHandler;

	private void Awake()
	{
		tileRestingRotation = transform.rotation;
		DeactivateTile();
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}


	// Initialization, activation and deactivation methods

	/// <summary>
	/// Initialize the tile, setting character, matrix location, scale, resting position and the movement restrictions
	/// </summary>
	/// <param name="character"></param>
	/// <param name="row"></param>
	/// <param name="column"></param>
	/// <param name="overrideStartPosition">Override the start position, used for spawning above the board</param>
	public void InitializeTile(char character, int row, int column, Vector3? overrideStartPosition = null)
	{
		SetTileCharacter(character);
		SetTileMatrixIndex(row, column);
		SetTileScale();
		SetTileRestingPosition();

		if (overrideStartPosition != null)
		{
			transform.position = overrideStartPosition.Value;
			// TODO: Need to sort this little mess out
			//StartTileMovingTileToRestingPosition();
		}
		else
		{
			transform.position = tileRestingPosition;
		}
	}

	/// <summary>
	/// Activate the tile to make it visible and interactable
	/// </summary>
	public void ActivateTile()
	{
		tileIsActive = true;
		boxCollider.enabled = true;
		visualCube.SetActive(true);
		textMesh.enabled = true;
	}

	/// <summary>
	/// Deactivate the tile, will be invisible and not interactable
	/// </summary>
	public void DeactivateTile()
	{
		tileIsActive = false;
		boxCollider.enabled = false;
		visualCube.SetActive(false);
		textMesh.enabled = false;
		transform.rotation = tileRestingRotation;

		StopAllCoroutines();
	}


	// Moving the tile

	/// <summary>
	/// Use when the tile needs to return to its resting position
	/// </summary>
	public void AnimateToRestingPositionInGrid()
	{
		singleTileMover.StopMoving();
		StartTileMovingTileToRestingPosition();
	}

	/// <summary>
	/// Start the moving of the tile towards the resting position
	/// </summary>
	private void StartTileMovingTileToRestingPosition()
	{
		if (transform.position != TileRestingPosition)
		{
			StartCoroutine(AnimateTileMovingToNewPositionCoroutine());
		}
		else
		{
			tileEventHandler.RaiseSingleTileFinishedAnimation(this);
		}
	}

	/// <summary>
	/// Coroutine for animating the tile to its resting position
	/// </summary>
	/// <returns></returns>
	private IEnumerator AnimateTileMovingToNewPositionCoroutine()
	{
		while (transform.position != TileRestingPosition)
		{
			transform.position = Vector3.MoveTowards(
			transform.position,
			TileRestingPosition,
			Time.deltaTime * gravitySpeed);
			yield return null;
		}

		tileEventHandler.RaiseSingleTileFinishedAnimation(this);
	}


	// Setting the tile properties

	/// <summary>
	/// Sets the grid matrix and the resting position of the tile
	/// </summary>
	/// <param name="row"></param>
	/// <param name="column"></param>
	public void SetNewGridPosition(int row, int column)
	{
		SetTileMatrixIndex(row, column);
		SetTileRestingPosition();
	}

	/// <summary>
	/// Set the character of the tile, needs to be public to allow the generation of board without existing words.
	/// </summary>
	/// <param name="character"></param>
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


	// Control of the tile

	/// <summary>
	/// When the tile is first selected, this can be called from the tile manager which should get a reference via a ray
	/// </summary>
	/// <param name="mousePosition"></param>
	public void TileWasClickedOn(Vector2 mousePosition)
	{
		singleTileMover.StartMoving(mousePosition);
	}


	// Destroying the tile

	/// <summary>
	/// Called from the PlayManager when this tile is part of a detected word
	/// The animation is triggered, which will then call HighlightAnimationFinished
	/// </summary>
	public void StartDestroySequence()
	{
		animator.SetTrigger(destroyMovementAnimationString);
	}

	/// <summary>
	/// Called once the destory animation is complete
	/// </summary>
	public void DestroySequenceIsComplete()
	{
		tileEventHandler.RaiseSingleTileFinishedAnimation(this);
	}


	// Helper methods

	private void SetTileMatrixIndex(int row, int column)
	{
		_row = row;
		_column = column;
	}

	private void SetMovementRestrictions()
	{
		var movementRestrictions = new MovementRestrictions();

		// X PLAIN

		// If the tile is on the left-hand side of the board, it can only move to the right
		if (_column == 0)
		{
			movementRestrictions.xMin = tileRestingPosition.x;
		}
		// Otherwise the tile can move left one tile size plus padding		
		else
		{
			movementRestrictions.xMin = tileRestingPosition.x - SizeManager.Instance.TileSize.x - SizeManager.Instance.InteriorPaddingSizes.x;
		}


		// if the tile is on the right-hand side of the board, it can only move to the left
		if (_column == SettingsScriptable.Columns - 1)
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
		if (_row == 0)
		{
			movementRestrictions.yMax = tileRestingPosition.y;
		}
		// Otherwise the tile can move up one tile size plus padding
		else
		{
			movementRestrictions.yMax = tileRestingPosition.y + SizeManager.Instance.TileSize.y + SizeManager.Instance.InteriorPaddingSizes.y;
		}

		// if the tile is on the bottom of the board, it can only move up
		if (_row == SettingsScriptable.Rows - 1)
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

	private void SetTileRestingPosition()
	{
		tileRestingPosition = SizeManager.Instance.TileSpawnPositions[_row, _column];
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
}
