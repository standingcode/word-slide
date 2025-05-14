using System;
using UnityEngine;
using WordSlide;

[CreateAssetMenu(fileName = "GameStateEventHandler", menuName = "Scriptable WordSlide/GameStateEvent")]
public class GameStateEventHandler : ScriptableObject
{
	#region game state changed

	public Action<GameState> GameStateChanged;

	public void RaiseGameStateChanged(GameState gameState)
	{
		Debug.Log($"GameStateEventHandler changing to: {gameState}");

		GameStateChanged?.Invoke(gameState);
	}

	public void AddGameStateChangedListener(Action<GameState> listener)
	{
		GameStateChanged += listener;
	}
	public void RemoveGameStateChangedListener(Action<GameState> listener)
	{
		GameStateChanged -= listener;
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
		NewGameStarted = null;
		NewGameStarted = null;
	}
}
