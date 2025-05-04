using System.Collections.Generic;

namespace WordSlide
{
	public interface IWordFinderService
	{
		public List<SingleTileManagerSequence> GetListOfValidWordsFromGivenRowsAndOrColumns(
		IDictionaryService _dictionaryService, List<SingleTileManagerSequence> rowsAndColumnsToCheck);
	}
}