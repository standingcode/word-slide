using System;
using UnityEngine;

public class DetectScreenSizeChange : MonoBehaviour
{
	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private Vector2 referenceResolution;

	// This event triggers on the event of a screen change, and passes the pixel difference
	public static Action<Vector2> ScreenSizeChanged;

	private Vector2 previousFrameScreenSize;

	void Start()
	{
		previousFrameScreenSize = rectTransform.sizeDelta;
	}


	void Update()
	{
		if (previousFrameScreenSize != rectTransform.sizeDelta)
		{
			ScreenSizeChanged?.Invoke(referenceResolution / rectTransform.sizeDelta);
			previousFrameScreenSize = rectTransform.sizeDelta;
		}
	}
}
