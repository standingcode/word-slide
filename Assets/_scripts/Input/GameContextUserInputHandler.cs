using UnityEngine;
using UnityEngine.InputSystem;

namespace WordSlide
{
	public class GameContextUserInputHandler : MonoBehaviour
	{
		[SerializeField]
		private MouseClickEvent mouseClickEvent;

		public void MainSelectOrDeselectPerformed(InputAction.CallbackContext callbackContext)
		{
			if (callbackContext.ReadValue<float>() == 1)
			{
				// Mouse was pressed
				mouseClickEvent.RaiseMouseClicked(Input.mousePosition);
			}
			else
			{
				// Mouse was released
				mouseClickEvent.RaiseMouseReleased();
			}
		}
	}
}
