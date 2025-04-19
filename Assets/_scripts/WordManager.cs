using System.Collections.Generic;
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
			var sw = System.Diagnostics.Stopwatch.StartNew();

			bool wordWasFound = _currentDictionary.ContainsKey(word);

			sw.Stop();

			Debug.Log($"_currentDictionary.ContainsKey(word) took {sw.ElapsedMilliseconds} ms");

			return wordWasFound;

		}
	}
}
