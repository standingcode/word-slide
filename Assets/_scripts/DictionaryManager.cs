using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WordSlide
{
	public class DictionaryManager : IDictionaryManager
	{
		private IDictionaryImporter _dictionaryImporter;

		private Dictionary<string, Word> currentDictionary = new();

		private List<char> characterSet = new();

		public bool DictionaryLoaded => currentDictionary.Count > 0;
		public bool CharacterSetLoaded => characterSet.Count > 0;

		public DictionaryManager(IDictionaryImporter dictionaryImporter)
		{
			_dictionaryImporter = dictionaryImporter;
		}

		public async Task LoadDictionary(string language)
		{
			currentDictionary = await _dictionaryImporter.GetDictionary(language);
		}

		public async Task LoadCharacterSet(string language)
		{
			characterSet = await _dictionaryImporter.GetCharacterSetForDictionary(language);
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

			return currentDictionary.ContainsKey(word);
		}

		public char GetRandomChar()
		{
			if (!CharacterSetLoaded)
			{
				throw new System.Exception("Character set not loaded. Please load a character set before getting random chars.");
			}
			int randomIndex = UnityEngine.Random.Range(0, characterSet.Count);
			return characterSet[randomIndex];
		}
	}
}
