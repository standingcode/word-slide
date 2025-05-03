using System.Collections.Generic;

namespace WordSlide
{
	public interface IWordFinderService
	{
		public List<SingleTileManagersRepresentingAString> GetListOfValidWordsFromGivenRowsAndOrColumns(
		IDictionaryService _dictionaryManager, List<SingleTileManagersRepresentingAString> rowsAndColumnsToCheck);
	}
}