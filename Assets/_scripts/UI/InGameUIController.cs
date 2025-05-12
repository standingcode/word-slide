using UnityEngine;

public class InGameUIController : MonoBehaviour
{
	[SerializeField]
	private GameObject loadingCanvas;

	public static InGameUIController Instance { get; private set; }

	public void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}
		Instance = this;
	}

	public void ShowLoadingCanvas()
	{
		loadingCanvas.SetActive(true);
	}

	public void HideLoadingCanvas()
	{
		loadingCanvas?.SetActive(false);
	}
}
