using TMPro;
using UnityEngine;

public class SingleTile : MonoBehaviour
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

	public void SetShownCharacter(char character)
	{
		textMesh.text = character.ToString().ToUpper();
	}

	public void Awake()
	{
		DeactivateTile();
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
