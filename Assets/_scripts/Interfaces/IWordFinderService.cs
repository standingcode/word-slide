using System.Collections.Generic;

namespace WordSlide
{
	public interface IWordFinderService
	{
		public HashSet<SingleTileManagerSequence> GetListOfValidWordsFromGivenRowsAndOrColumns(
		IDictionaryService _dictionaryService, HashSet<SingleTileManagerSequence> rowsAndColumnsToCheck);
	}
}