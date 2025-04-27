using UnityEngine;
using UnityEngine.InputSystem;

namespace WordSlide
{
	public class GameContextUserInputHandler : MonoBehaviour
	{
		[SerializeField]
		private ClickEventHandler ClickEventHandler;

		public void MainSelectOrDeselectPerformed(InputAction.CallbackContext callbackContext)
		{
			if (callbackContext.ReadValue<float>() == 1)
			{
				// Mouse was pressed
				ClickEventHandler.RaiseInputClicked(PointerMethods.GetMouseOrPointerPosition());
			}
			else
			{
				// Mouse was released
				ClickEventHandler.RaiseMouseReleased();
			}
		}
	}
}
