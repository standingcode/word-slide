using UnityEngine;
using UnityEngine.InputSystem;

namespace WordSlide
{
	public static class PointerMethods
	{
		public static Vector2 GetMouseOrPointerPosition()
		{

#if UNITY_EDITOR || UNITY_EDITOR_WIN
			return Mouse.current.position.ReadValue();
#else
			return Touchscreen.current.primaryTouch.position.ReadValue();
#endif
		}
	}
}

