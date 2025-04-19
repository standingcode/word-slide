using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace WordSlide
{
	public class WordManager : IWordManager
	{
		private IDictionaryImporter _dictionaryImporter;

		private Dictionary<string, Word> _currentDictionary = new();

		public WordManager(IDictionaryImporter dictionaryImporter)
		{
			_dictionaryImporter = dictionaryImporter;
			_currentDictionary = _dictionaryImporter.GetDictionary("english");
		}

		public bool CheckWord(string word)
		{
			if (string.IsNullOrEmpty(word))
			{
				return false;
			}

			return _currentDictionary.ContainsKey(word);
		}
	}
}
