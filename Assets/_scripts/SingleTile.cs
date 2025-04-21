using TMPro;
using UnityEngine;

public class SingleTile : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI textMesh;

	public void SetShownCharacter(char character)
	{
		textMesh.text = character.ToString().ToUpper();
	}
}
