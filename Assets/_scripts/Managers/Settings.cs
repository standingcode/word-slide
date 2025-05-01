using Pooling;
using UnityEngine;

namespace WordSlide
{
	public class Settings : MonoBehaviour
	{
		[SerializeField]
		private int columns = 10;
		public int Columns => columns;

		[SerializeField]
		private int rows = 10;
		public int Rows => rows;

		[SerializeField]
		private int minimumWordLength = 3;
		public int MinimumWordLength => minimumWordLength;

		public static Settings Instance { get; private set; }

		public void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
				Debug.LogError("Settings instance already exists. Destroying the new instance.");
				return;
			}

			Instance = this;
		}

		private void OnDestroy()
		{
			Instance = null;
		}
	}
}
