using System;
using System.Collections.Generic;
using UnityEngine;
using WordSlide;

[CreateAssetMenu(fileName = "GameStateEventHandler", menuName = "Scriptable WordSlide/GameStateEvent")]
public class GameStateEventHandler : ScriptableObject
{
	#region A new board has been generated

	public Action<List<SingleTileManagerSequence>> NewBoardGenerated;

	public void RaiseNewBoardGenerated(List<SingleTileManagerSequence> generatedBoard)
	{
		NewBoardGenerated?.Invoke(generatedBoard);
	}

	public void AddNewBoardGeneratedListener(Action<List<SingleTileManagerSequence>> listener)
	{
		NewBoardGenerated += listener;
	}

	public void RemoveNewBoardGeneratedListener(Action<List<SingleTileManagerSequence>> listener)
	{
		NewBoardGenerated -= listener;
	}

	#endregion


	#region Requested tile swaps have occourred (This is part of the process of generating a new board)

	public Action<List<SingleTileManagerSequence>> ChangeTilesRequestedDueToContainingWords;

	public void RaiseChangeTilesRequestedDueToContainingWords(List<SingleTileManagerSequence> rowsAndColumnsAffected)
	{
		ChangeTilesRequestedDueToContainingWords?.Invoke(rowsAndColumnsAffected);
	}

	public void AddChangeTilesRequestedDueToContainingWordsListener(Action<List<SingleTileManagerSequence>> listener)
	{
		ChangeTilesRequestedDueToContainingWords += listener;
	}

	public void RemoveChangeTilesRequestedDueToContainingWordsListener(Action<List<SingleTileManagerSequence>> listener)
	{
		ChangeTilesRequestedDueToContainingWords -= listener;
	}

	#endregion


	#region Tile swap requested by user play

	public Action<List<SingleTileManagerSequence>> TileSwapped;

	public void RaiseTileSwapped(List<SingleTileManagerSequence> rowsAndColumnsAffected)
	{
		TileSwapped?.Invoke(rowsAndColumnsAffected);
	}

	public void AddTileSwappedListener(Action<List<SingleTileManagerSequence>> listener)
	{
		TileSwapped += listener;
	}

	public void RemoveTileSwappedListener(Action<List<SingleTileManagerSequence>> listener)
	{
		TileSwapped -= listener;
	}

	#endregion


	#region A new game has started

	public Action NewGameStarted;

	public void RaiseNewGameStarted()
	{
		NewGameStarted?.Invoke();
	}

	public void AddNewGameStartedListener(Action listener)
	{
		NewGameStarted += listener;
	}

	public void RemoveNewGameStartedListener(Action listener)
	{
		NewGameStarted -= listener;
	}

	#endregion


	private void OnDestroy()
	{
		TileSwapped = null;
		NewGameStarted = null;
		ChangeTilesRequestedDueToContainingWords = null;
		NewBoardGenerated = null;
		NewGameStarted = null;
	}
}
