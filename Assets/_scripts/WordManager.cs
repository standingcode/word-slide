using System.Collections.Generic;
using System.Threading.Tasks;

namespace WordSlide
{
	public class WordManager : IWordManager
	{
		private IDictionaryImporter _dictionaryImporter;

		private Dictionary<string, Word> _currentDictionary = new();

		public bool DictionaryLoaded => _currentDictionary.Count > 0;

		public WordManager(IDictionaryImporter dictionaryImporter)
		{
			_dictionaryImporter = dictionaryImporter;
		}

		public async Task LoadDictionary(string language)
		{
			_currentDictionary = await _dictionaryImporter.GetDictionary(language);
		}

		public bool CheckWord(string word)
		{
			if (!DictionaryLoaded)
			{
				throw new System.Exception("Dictionary not loaded. Please load a dictionary before checking words.");
			}

			if (string.IsNullOrEmpty(word))
			{
				return false;
			}

			return _currentDictionary.ContainsKey(word);
		}
	}
}
