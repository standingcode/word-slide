using UnityEngine;
using UnityEngine.InputSystem;

namespace WordSlide
{
	public static class PointerMethods
	{
		public static Vector2 GetMouseOrPointerPosition()
		{

#if UNITY_EDITOR || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
			return Mouse.current.position.ReadValue();
#else
			return Touchscreen.current.primaryTouch.position.ReadValue();
#endif
		}
	}
}

