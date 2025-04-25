using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WordSlide
{
	public struct MoveRestriction
	{
		public float xMin;
		public float xMax;
		public float yMin;
		public float yMax;
	}

	public class UserInputHandler : MonoBehaviour
	{
		Coroutine moveCoroutine;

		public void MainSelectOrDeselectPerformed(InputAction.CallbackContext callbackContext)
		{
			if (callbackContext.ReadValue<float>() == 1)
			{
				// Mouse was pressed
				MainSelectOn();
			}
			else
			{
				// Mouse was released
				MainSelectOff();
			}
		}

		private void MainSelectOn()
		{
			if (moveCoroutine != null)
			{
				return;
			}

			moveCoroutine = StartCoroutine(TileIsBeingMoved());
		}

		private void MainSelectOff()
		{
			StopCoroutine(moveCoroutine);
			moveCoroutine = null;
		}

		private IEnumerator TileIsBeingMoved()
		{
			while (true)
			{
				MoveTile();
				yield return null;
			}
		}

		private void MoveTile()
		{
			Debug.Log($"I'm being moved: {Mouse.current.position.x.value}, {Mouse.current.position.y.value}");
		}


		private void OnDisable()
		{
			StopAllCoroutines();
		}
	}
}
