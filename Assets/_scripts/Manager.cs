using UnityEngine;

public class Manager : MonoBehaviour
{
	public static Manager Instance { get; private set; }
	//public static Settings Settings { get; private set; }
	//public static TimeManager TimeManager { get; private set; }

	public static PoolManager PoolManager { get; private set; }
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}

		Instance = this;

		//Settings = GetComponent<Settings>();
		//TimeManager = GetComponent<TimeManager>();
		PoolManager = GetComponent<PoolManager>();
	}

	private void OnDestroy()
	{
		Instance = null;
	}
}
