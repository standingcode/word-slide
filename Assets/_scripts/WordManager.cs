using UnityEngine;

namespace WordSlide
{
	public class WordManager : IWordManager
	{
		public WordManager()
		{
			Debug.Log("WordManager: Constructor called");
		}

		public bool CheckWord(string word)
		{
			Debug.Log("WordManager: Checking word: " + word);
			// Check if the word is in the dictionary
			return true;
		}
	}
}
