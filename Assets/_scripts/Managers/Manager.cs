using Pooling;
using UnityEngine;

namespace WordSlide
{
	public class Manager : MonoBehaviour
	{
		[SerializeField]
		private int columns = 10;
		public int Columns => columns;

		[SerializeField]
		private int rows = 10;
		public int Rows => rows;

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
}
