using UnityEngine;
using UnityEngine.InputSystem;

namespace WordSlide
{
	public static class PointerMethods
	{
		public static Vector2 GetMouseOrPointerPosition()
		{
#if ANDROID || IOS
			return Touchscreen.current.primaryTouch.position.ReadValue();
#else
			return Mouse.current.position.ReadValue();
#endif
		}
	}
}

