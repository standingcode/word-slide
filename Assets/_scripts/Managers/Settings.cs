using Pooling;
using UnityEngine;

namespace WordSlide
{
	public class Settings : MonoBehaviour
	{
		[SerializeField]
		private SettingsScriptable settingsScriptable;

		public int Columns => settingsScriptable.Columns;
		public int Rows => settingsScriptable.Rows;
		public int MinimumWordLength => settingsScriptable.MinimumWordLength;

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
