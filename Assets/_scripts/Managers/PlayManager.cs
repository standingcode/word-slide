using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace WordSlide
{
	/// <summary>
	/// Could potentially use later to add defintions etc for the word
	/// </summary>
	public struct Word
	{
		public string word;
	}

	public class PlayManager : MonoBehaviour
	{
		public static PlayManager Instance { get; private set; }

		private IDictionaryManager _dictionaryManager;

		[Inject]
		public async Task Construct(IDictionaryManager dictionaryManager)
		{
			if (Instance != null && Instance != this)
			{
				Debug.LogError("PlayManager instance already exists. Destroying the new instance.");
				Destroy(this);
				return;
			}

			_dictionaryManager = dictionaryManager;

			await _dictionaryManager.LoadDictionary("english");
			await _dictionaryManager.LoadCharacterSet("english");
		}

		public List<string> CheckWordString(string stringToCheck)
		{
			return WordFinder.CheckStringForWords(_dictionaryManager, stringToCheck);
		}
	}
}