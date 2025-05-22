using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
	public static (HashSet<int> rows, HashSet<int> columns) GetAffectedRowsAndColumns(HashSet<SingleTileManager> singleTileManagers)
	{
		(HashSet<int> rows, HashSet<int> columns) affectedRowsAndColumns = new();

		affectedRowsAndColumns.rows = new();
		affectedRowsAndColumns.columns = new();

		foreach (var tile in singleTileManagers)
		{
			affectedRowsAndColumns.rows.Add(tile.Row);
			affectedRowsAndColumns.columns.Add(tile.Column);
		}

		return affectedRowsAndColumns;
	}
}
