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
			Debug.Log("WordManager: Constructor called");
			_dictionaryImporter = dictionaryImporter;
			_currentDictionary = _dictionaryImporter.GetDictionary("english");
		}

		public bool CheckWord(string word)
		{
			var timer = new System.Diagnostics.Stopwatch();
			timer.Start();

			bool wordWasFound = _currentDictionary.ContainsKey(word);

			Debug.Log($"_currentDictionary.ContainsKey(word) took {timer.ElapsedMilliseconds} ms");

			return wordWasFound;

		}
	}
}
