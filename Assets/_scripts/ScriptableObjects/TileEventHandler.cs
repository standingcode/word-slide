using System;
using System.Collections.Generic;
using UnityEngine;
using WordSlide;

[CreateAssetMenu(fileName = "TileEvent", menuName = "Scriptable WordSlide/TileEvent")]
public class TileEventHandler : ScriptableObject
{
	[SerializeField] private GameStateEventHandler gameStateEventHandler;

	private Action<SingleTileManager, Vector2> TileWasClickedOn;
	private Action CheckIfTileWasClickedOff;

	private Action<HashSet<SingleTileManagerSequence>> BoardGenerated;
	private Action BoardRequiresReconfiguring;


	private Action<HashSet<SingleTileManager>> ChangeCharactersForTiles;

	private Action<SingleTileManager, SingleTileManager> TilesNeedToBeSwappedBack;
	private Action<HashSet<SingleTileManager>> TilesNeedToBeDestroyed;
	private Action<SingleTileManager> SingleTileFinishedAnimation;
	private Action<HashSet<SingleTileManager>> AllTilesFinishedAnimating;

	// TILE WAS CLICKED ON

	public void RaiseTileWasClickedOn(SingleTileManager tile, Vector2 position)
	{
		TileWasClickedOn?.Invoke(tile, position);
	}

	public void AddTileWasClickedOnListener(Action<SingleTileManager, Vector2> listener)
	{
		TileWasClickedOn += listener;
	}

	public void RemoveTileWasClickedOnListener(Action<SingleTileManager, Vector2> listener)
	{
		TileWasClickedOn -= listener;
	}

	// CHECK IF TILE WAS CLICKED OFF

	public void RaiseCheckIfTileWasClickedOff()
	{
		CheckIfTileWasClickedOff?.Invoke();
	}

	public void AddCheckIfTileWasClickedOffListener(Action listener)
	{
		CheckIfTileWasClickedOff += listener;
	}

	public void RemoveCheckIfTileWasClickedOffListener(Action listener)
	{
		CheckIfTileWasClickedOff -= listener;
	}


	// NEW BOARD GENERATED

	public void RaiseBoardGenerated(HashSet<SingleTileManagerSequence> boardGenerated)
	{
		gameStateEventHandler.RaiseChangeGameState(GameState.BoardGenerationInProgress);
		BoardGenerated?.Invoke(boardGenerated);
	}

	public void AddNewBoardGeneratedListener(Action<HashSet<SingleTileManagerSequence>> listener)
	{
		BoardGenerated += listener;
	}

	public void RemoveNewBoardGeneratedListener(Action<HashSet<SingleTileManagerSequence>> listener)
	{
		BoardGenerated -= listener;
	}

	// BOARD REQUIRES RECONFIGURING

	public void RaiseBoardRequiresReconfiguring()
	{
		gameStateEventHandler.RaiseChangeGameState(GameState.BoardIsBeingReconfigured);
		BoardRequiresReconfiguring?.Invoke();
	}

	public void AddBoardRequiresReconfiguringListener(Action listener)
	{
		BoardRequiresReconfiguring += listener;
	}

	public void RemoveBoardRequiresReconfiguringListener(Action listener)
	{
		BoardRequiresReconfiguring -= listener;
	}

	// CHANGE CHARACTERS FOR TILES

	public void RaiseChangeCharactersForTiles(HashSet<SingleTileManager> tiles)
	{
		ChangeCharactersForTiles?.Invoke(tiles);
	}

	public void AddChangeCharactersForTilesListener(Action<HashSet<SingleTileManager>> listener)
	{
		ChangeCharactersForTiles += listener;
	}

	public void RemoveChangeCharactersForTilesListener(Action<HashSet<SingleTileManager>> listener)
	{
		ChangeCharactersForTiles -= listener;
	}

	// TILES NEED TO BE SWAPPED BACK

	public void RaiseTilesNeedSwappingBack(SingleTileManager tile1, SingleTileManager tile2)
	{
		gameStateEventHandler.RaiseChangeGameState(GameState.TilesAreBeingSwappedBack);
		TilesNeedToBeSwappedBack?.Invoke(tile1, tile2);
	}

	public void AddTilesNeedSwappingBackListener(Action<SingleTileManager, SingleTileManager> listener)
	{
		TilesNeedToBeSwappedBack += listener;
	}

	public void RemoveTilesNeedSwappingBackListener(Action<SingleTileManager, SingleTileManager> listener)
	{
		TilesNeedToBeSwappedBack -= listener;
	}


	// TILES NEED TO BE DESTROYED

	public void RaiseTilesNeedsToBeDestroyed(HashSet<SingleTileManager> tiles)
	{
		gameStateEventHandler.RaiseChangeGameState(GameState.TilesAreBeingDestroyed);
		TilesNeedToBeDestroyed?.Invoke(tiles);
	}

	public void AddTilesNeedsToBeDestroyedListener(Action<HashSet<SingleTileManager>> listener)
	{
		TilesNeedToBeDestroyed += listener;
	}

	public void RemoveTilesNeedsToBeDestroyedListener(Action<HashSet<SingleTileManager>> listener)
	{
		TilesNeedToBeDestroyed -= listener;
	}

	// SINGLE TILE FINISHED ANIMATION

	public void RaiseSingleTileFinishedAnimation(SingleTileManager tile)
	{
		SingleTileFinishedAnimation?.Invoke(tile);
	}

	public void AddSingleTileFinishedAnimationListener(Action<SingleTileManager> listener)
	{
		SingleTileFinishedAnimation += listener;
	}

	public void RemoveSingleTileFinishedAnimationListener(Action<SingleTileManager> listener)
	{
		SingleTileFinishedAnimation -= listener;
	}


	// ALL TILES FINISHED ANIMATING
	public void RaiseAllTilesFinishedAnimating(HashSet<SingleTileManager> tiles)
	{
		AllTilesFinishedAnimating?.Invoke(tiles);
	}

	public void AddAllTilesFinishedAnimatingListener(Action<HashSet<SingleTileManager>> listener)
	{
		AllTilesFinishedAnimating += listener;
	}

	public void RemoveAllTilesFinishedAnimatingListener(Action<HashSet<SingleTileManager>> listener)
	{
		AllTilesFinishedAnimating -= listener;
	}


	private void OnDestroy()
	{
		TileWasClickedOn = null;
		CheckIfTileWasClickedOff = null;

		BoardGenerated = null;

		ChangeCharactersForTiles = null;

		TilesNeedToBeSwappedBack = null;
		TilesNeedToBeDestroyed = null;
		SingleTileFinishedAnimation = null;
		AllTilesFinishedAnimating = null;
	}
}
