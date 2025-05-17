using System;
using UnityEngine;
using WordSlide;

public enum GameState
{
	None = 0,
	WaitingForPlayer,
	NewGameStarted,
	BoardGeneratedInProgress,
	TilesAreBeingSwapped,
	TilesAreBeingSwappedBack,
	SingleTileIsBeingAnimatedBackToOriginalPosition,
	TilesAreBeingDestroyed,
	BoardIsBeingReconfigured
}



[CreateAssetMenu(fileName = "GameStateEventHandler", menuName = "Scriptable WordSlide/GameStateEvent")]
public class GameStateEventHandler : ScriptableObject
{
	public static GameState GameState { get; private set; } = GameState.None;

	private Action<GameState> GameStateChanged;
	private Action NewGameStarted;

	public void RaiseChangeGameState(GameState gameState)
	{
		//Debug.Log($"GameStateEventHandler changing to: {gameState}");
		GameState = gameState;
		GameStateChanged?.Invoke(gameState);
	}

	// GENERAL GAME STATE EVENT

	// Subscribe methods who want to know about any state change
	public void AddGameStateChangedListener(Action<GameState> listener)
	{
		GameStateChanged += listener;
	}

	// Unsubscribe methods who want to know about any state change
	public void RemoveGameStateChangedListener(Action<GameState> listener)
	{
		GameStateChanged -= listener;
	}

	// NEW GAME EVENT
	public void RaiseNewGame()
	{
		RaiseChangeGameState(GameState.NewGameStarted);
		NewGameStarted?.Invoke();
	}

	// Add a new game started listener
	public void AddNewGameStartedListener(Action listener)
	{
		NewGameStarted += listener;
	}

	// Remove a new game started listener
	public void RemoveNewGameStartedListener(Action listener)
	{
		NewGameStarted -= listener;
	}

	private void OnDestroy()
	{
		GameStateChanged = null;
		NewGameStarted = null;
	}
}