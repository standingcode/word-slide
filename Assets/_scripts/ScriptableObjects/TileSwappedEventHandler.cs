using System;
using System.Collections.Generic;
using UnityEngine;
using WordSlide;

[CreateAssetMenu(fileName = "TileSwappedEvent", menuName = "Scriptable WordSlide/TileSwappedEvent")]
public class TileSwappedEventHandler : ScriptableObject
{
	public Action<List<SingleTileManagerSequence>> tileSwapped;

	public void RaiseTileSwapped(List<SingleTileManagerSequence> rowsAndColumnsAffected)
	{
		tileSwapped?.Invoke(rowsAndColumnsAffected);
	}

	public void AddTileSwappedListener(Action<List<SingleTileManagerSequence>> listener)
	{
		tileSwapped += listener;
	}

	public void RemoveTileSwappedListener(Action<List<SingleTileManagerSequence>> listener)
	{
		tileSwapped -= listener;
	}

	private void OnDestroy()
	{
		tileSwapped = null;
	}
}
