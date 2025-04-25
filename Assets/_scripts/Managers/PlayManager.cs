using System.Collections.Generic;
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
		private IDictionaryManager _dictionaryManager;

		[Inject]
		public async Task Construct(IDictionaryManager dictionaryManager)
		{
			_dictionaryManager = dictionaryManager;

			await _dictionaryManager.LoadDictionary("english");
			await _dictionaryManager.LoadCharacterSet("english");
		}
	}
}