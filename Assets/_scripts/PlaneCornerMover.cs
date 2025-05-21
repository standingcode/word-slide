using UnityEngine;
using WordSlide;

public class PlaneCornerMover : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;

	public Transform leftBottom, rightTop;

	// Update is called once per frame
	void Update()
	{
		//spriteRenderer.size = new Vector2(rightTop.position.x - leftBottom.position.x, rightTop.position.y - leftBottom.position.y);
		//transform.position = new Vector3(leftBottom.position.x + (spriteRenderer.size.x / 2), leftBottom.position.y + spriteRenderer.size.y / 2, transform.position.z);

		transform.position = new Vector3(
		0f,
		Camera.main.orthographicSize - (0.5f * SizeManager.Instance.BoardSize.y) - SettingsScriptable.MinimumMarginFromBoardAsRatio * Camera.main.orthographicSize,
		transform.position.z);

		spriteRenderer.size = new Vector2(
		SizeManager.Instance.BoardSize.x,
		Camera.main.orthographicSize - (0.5f * SizeManager.Instance.BoardSize.y));

		//spriteRenderer.size = new Vector2(
		//1f,
		//1f);

	}
}
