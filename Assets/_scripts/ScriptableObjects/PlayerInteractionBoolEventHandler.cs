using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnablePlayerInteraction", menuName = "Scriptable WordSlide/EnablePlayerInteractionEvent")]
public class PlayerInteractionBoolEventHandler : ScriptableObject
{
	public Action<bool> PlayerInteractionEnabled;

	public void RaisePlayerInteractionBoolChange(bool value)
	{
		PlayerInteractionEnabled?.Invoke(value);
	}

	public void AddPlayerInteractionBoolListener(Action<bool> listener)
	{
		PlayerInteractionEnabled += listener;
	}

	public void RemovePlayerInteractionBoolListener(Action<bool> listener)
	{
		PlayerInteractionEnabled -= listener;
	}

	private void OnDestroy()
	{
		PlayerInteractionEnabled = null;
	}
}
