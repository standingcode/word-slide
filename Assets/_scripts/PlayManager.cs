using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace WordSlide
{
	/// <summary>
	/// Could potentially use later to add defintions etc for the word
	/// </summary>
	public struct Word
	{
		public string word;
	}

	public class PlayManager : MonoBehaviour
	{
		private Dictionary<string, Word> englishDictionary = new();

		private IWordManager _wordManager;

		[Inject]
		public void Construct(IWordManager wordManager)
		{
			_wordManager = wordManager;
		}

		void Start()
		{

		}
	}
}