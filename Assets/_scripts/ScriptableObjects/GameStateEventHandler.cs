using System;
using System.Collections.Generic;
using UnityEngine;
using WordSlide;

[CreateAssetMenu(fileName = "GameStateEventHandler", menuName = "Scriptable WordSlide/GameStateEvent")]
public class GameStateEventHandler : ScriptableObject
{
	#region Player can interact with tiles

	public Action<bool> PlayerCanInteractWithTilesChanged;

	public void RaisePlayerCanInteractWithTilesChanged(bool canInteract)
	{
		PlayerCanInteractWithTilesChanged?.Invoke(canInteract);
	}

	public void AddPlayerCanInteractWithTilesChangedListener(Action<bool> listener)
	{
		PlayerCanInteractWithTilesChanged += listener;
	}

	public void RemovePlayerCanInteractWithTilesChangedListener(Action<bool> listener)
	{
		PlayerCanInteractWithTilesChanged -= listener;
	}

	#endregion


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
		NewBoardGenerated = null;
		NewGameStarted = null;
	}
}
