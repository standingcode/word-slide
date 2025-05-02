using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ClickEvent", menuName = "Scriptable WordSlide/ClickEvent")]
public class ClickEventHandler : ScriptableObject
{
	public Action<Vector2> InputClicked;
	public Action InputClickReleased;

	public void RaiseInputClicked(Vector2 position)
	{
		InputClicked?.Invoke(position);
	}

	public void RaiseInputClickReleased()
	{
		InputClickReleased?.Invoke();
	}

	public void AddClickDownListener(Action<Vector2> listener)
	{
		InputClicked += listener;
	}

	public void RemoveClickDownListener(Action<Vector2> listener)
	{
		InputClicked -= listener;
	}

	public void AddClickUpListener(Action listener)
	{
		InputClickReleased += listener;
	}

	public void RemoveClickUpListener(Action listener)
	{
		InputClickReleased -= listener;
	}

	private void OnDestroy()
	{
		InputClicked = null;
		InputClickReleased = null;
	}

}
