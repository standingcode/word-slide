using System.Collections.Generic;

namespace WordSlide
{
	public interface IDictionaryImporter
	{
		public Dictionary<string, Word> GetDictionary(string language);
	}
}
