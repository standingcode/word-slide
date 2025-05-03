using System.Collections.Generic;
using System.Threading.Tasks;

namespace WordSlide
{
	public interface IDictionaryImporterService
	{
		public Task<Dictionary<string, Word>> GetDictionary(string language);

		public Task<List<char>> GetCharacterSetForDictionary(string language);

	}
}
