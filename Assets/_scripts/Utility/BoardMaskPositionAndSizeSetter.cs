using UnityEngine;
using WordSlide;

public class BoardMaskPositionAndSizeSetter : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;

	void Start()
	{
		var yTopPosition = 0.5f * SizeManager.Instance.WorldUnitsInCamera.y;

		transform.position = new Vector3(
		0f,
		(yTopPosition) - (0.5f * (yTopPosition - (0.5f * SizeManager.Instance.BoardSize.y))),
		transform.position.z);

		spriteRenderer.size = new Vector2(
		SizeManager.Instance.WorldUnitsInCamera.x,
		(0.5f * SizeManager.Instance.WorldUnitsInCamera.y) - (0.5f * SizeManager.Instance.BoardSize.y));
	}
}
