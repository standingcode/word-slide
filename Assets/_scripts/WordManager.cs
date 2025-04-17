using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

namespace WordSlide
{
	public class WordManager : IWordManager
	{
		public bool CheckWord(string word)
		{
			Debug.Log("WordManager: Checking word: " + word);
			// Check if the word is in the dictionary
			return true;
		}

	}
}
