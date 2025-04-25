using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MouseClickEvent", menuName = "Scriptable WordSlide/MouseClickEvent")]
public class MouseClickEvent : ScriptableObject
{
	public Action<Vector2> MouseClicked;
	public Action MouseReleased;

	public void RaiseMouseClicked(Vector2 position)
	{
		MouseClicked?.Invoke(position);
	}

	public void RaiseMouseReleased()
	{
		MouseReleased?.Invoke();
	}

	public void AddMouseDownListener(Action<Vector2> listener)
	{
		MouseClicked += listener;
	}

	public void RemoveMouseDownListener(Action<Vector2> listener)
	{
		MouseClicked -= listener;
	}

	public void AddMouseUpListener(Action listener)
	{
		MouseReleased += listener;
	}

	public void RemoveMouseUpListener(Action listener)
	{
		MouseReleased -= listener;
	}

	private void OnDestroy()
	{
		MouseClicked = null;
		MouseReleased = null;
	}

}
