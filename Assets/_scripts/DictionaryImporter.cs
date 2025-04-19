using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace WordSlide
{
	public class DictionaryImporter : IDictionaryImporter
	{
		public async Task<List<char>> GetCharacterSetForDictionary(string language)
		{
			List<char> characterSet = new List<char>();
			var fileAsset = Resources.LoadAsync<TextAsset>("CharacterSets/" + language);
			await fileAsset;

			var fileContents = fileAsset.asset as TextAsset;

			foreach (var line in fileContents.text.Split('\n'))
			{
				var word = line.Trim();

				if (string.IsNullOrEmpty(word)) { continue; }

				var character = (char)word[0];
				int characterCount = 0;
				Int32.TryParse(word.Split(' ')[1], out characterCount);


				for (int i = 0; i < characterCount; i++)
				{
					characterSet.Add(character);
				}
			}
			return characterSet;
		}

		public async Task<Dictionary<string, Word>> GetDictionary(string language)
		{
			Dictionary<string, Word> dictionary = new Dictionary<string, Word>();

			var fileAsset = Resources.LoadAsync<TextAsset>("Dictionaries/" + language);
			await fileAsset;

			var fileContents = fileAsset.asset as TextAsset;

			foreach (var line in fileContents.text.Split('\n'))
			{
				var word = line.Trim();

				if (string.IsNullOrEmpty(word)) { continue; }

				var wordObj = new Word();
				wordObj.word = word;

				dictionary.Add(word, wordObj);
			}

			return dictionary;
		}
	}
}
