using System;
using UnityEngine;
using WordSlide;

[CreateAssetMenu(fileName = "GameStateEventHandler", menuName = "Scriptable WordSlide/GameStateEvent")]
public class GameStateEventHandler : ScriptableObject
{
	private Action<GameState> GameStateChanged;
	private Action NewGameStarted;

	public void RaiseGameStateChanged(GameState gameState)
	{
		Debug.Log($"GameStateEventHandler changing to: {gameState}");

		GameStateChanged?.Invoke(gameState);

		if (gameState == GameState.NewGameStarted)
		{
			NewGameStarted?.Invoke();
		}
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