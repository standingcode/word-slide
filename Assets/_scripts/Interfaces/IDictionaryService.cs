using System.Threading.Tasks;

namespace WordSlide
{
	public interface IDictionaryService
	{
		public bool DictionaryLoaded { get; }

		public bool CharacterSetLoaded { get; }

		public Task LoadDictionary(string language);

		public Task LoadCharacterSet(string language);

		public bool CheckWord(string word);

		public char GetRandomChar();
	}
}
