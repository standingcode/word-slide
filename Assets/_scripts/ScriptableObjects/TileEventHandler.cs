using System;
using System.Collections.Generic;
using UnityEngine;
using WordSlide;

[CreateAssetMenu(fileName = "TileEvent", menuName = "Scriptable WordSlide/TileEvent")]
public class TileEventHandler : ScriptableObject
{
	// New board has been generated

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


	// Word check needed after tile swap or drop

	public Action WordCheckNeeded;

	public void RaiseWordCheckNeeded()
	{
		WordCheckNeeded?.Invoke();
	}

	public void AddWordCheckNeededListener(Action listener)
	{
		WordCheckNeeded += listener;
	}

	public void RemoveWordCheckNeededListener(Action listener)
	{
		WordCheckNeeded -= listener;
	}


	// Tile animation has completed

	public Action<SingleTileManager> TileAnimationComplete;

	public void RaiseTileAnimationComplete(SingleTileManager singleTileManager)
	{
		TileAnimationComplete?.Invoke(singleTileManager);
	}

	public void AddTileAnimationCompleteListener(Action<SingleTileManager> listener)
	{
		TileAnimationComplete += listener;
	}

	public void RemoveTileAnimationCompleteListener(Action<SingleTileManager> listener)
	{
		TileAnimationComplete -= listener;
	}

	// Destroy sequence is completed

	public Action<SingleTileManager> DestroySequenceComplete;

	public void RaiseDestroySequenceComplete(SingleTileManager destroyedTile)
	{
		DestroySequenceComplete?.Invoke(destroyedTile);
	}

	public void AddDestroySequenceCompleteListener(Action<SingleTileManager> listener)
	{
		DestroySequenceComplete += listener;
	}

	public void RemoveDestroySequenceCompleteListener(Action<SingleTileManager> listener)
	{
		DestroySequenceComplete -= listener;
	}


	// Need to nullify everything on destruction

	private void OnDestroy()
	{
		TileAnimationComplete = null;
		NewBoardGenerated = null;
		WordCheckNeeded = null;
		DestroySequenceComplete = null;
	}
}
