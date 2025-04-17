using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WordSlide
{
	public class DictionaryImporter : IDictionaryImporter
	{
		public Dictionary<string, Word> GetDictionary(string language)
		{
			Dictionary<string, Word> dictionary = new Dictionary<string, Word>();

			var thingy = Resources.Load<TextAsset>("Dictionaries/" + language);

			foreach (var line in thingy.text.Split('\n'))
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
