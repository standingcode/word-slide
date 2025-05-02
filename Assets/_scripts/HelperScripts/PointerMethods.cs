using UnityEngine;
using UnityEngine.InputSystem;

namespace WordSlide
{
	public static class PointerMethods
	{
		public static Vector2 GetMouseOrPointerPosition()
		{
#if UNITY_EDITOR
			return Mouse.current.position.ReadValue();
#else
			return Touchscreen.current.primaryTouch.position.ReadValue();
#endif
		}
	}
}

