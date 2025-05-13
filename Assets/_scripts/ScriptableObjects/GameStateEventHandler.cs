using System;
using UnityEngine;

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
