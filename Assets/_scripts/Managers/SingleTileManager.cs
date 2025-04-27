using TMPro;
using UnityEngine;
using WordSlide;

public class SingleTileManager : MonoBehaviour
{
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

	public void SetShownCharacter(char character)
	{
		textMesh.text = character.ToString().ToUpper();
	}

	public void Awake()
	{
		DeactivateTile();
	}

	public void InitializeMovementRestrictions(MovementRestrictions movementRestrictions)
	{
		singleTileMover.InitializeMovementRestrictions(movementRestrictions);
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
