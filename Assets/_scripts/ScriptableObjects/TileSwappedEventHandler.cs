using System;
using System.Collections.Generic;
using UnityEngine;
using WordSlide;

[CreateAssetMenu(fileName = "TileSwappedEvent", menuName = "Scriptable WordSlide/TileSwappedEvent")]
public class TileSwappedEventHandler : ScriptableObject
{
	public Action<List<SingleTileManagersRepresentingAString>> tileSwapped;

	public void RaiseTileSwapped(List<SingleTileManagersRepresentingAString> rowsAndColumnsAffected)
	{
		tileSwapped?.Invoke(rowsAndColumnsAffected);
	}

	public void AddClickDownListener(Action<List<SingleTileManagersRepresentingAString>> listener)
	{
		tileSwapped += listener;
	}

	public void RemoveClickDownListener(Action<List<SingleTileManagersRepresentingAString>> listener)
	{
		tileSwapped -= listener;
	}

	private void OnDestroy()
	{
		tileSwapped = null;
	}

}
