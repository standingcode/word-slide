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

		public static Settings Instance { get; private set; }

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
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
