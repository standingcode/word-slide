using System.Collections.Generic;
using System.Threading.Tasks;

namespace WordSlide
{
	public class WordManager : IWordManager
	{
		private IDictionaryImporter _dictionaryImporter;

		private Dictionary<string, Word> _currentDictionary = new();

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
			if (string.IsNullOrEmpty(word))
			{
				return false;
			}

			return _currentDictionary.ContainsKey(word);
		}
	}
}
