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

	public class SingleTileMover : MonoBehaviour
	{
		[SerializeField]
		private float zMovementWhenSliding = -1f;

		Coroutine moveCoroutine;

		private bool moveX = false;
		private bool moveY = false;

		private bool tileIsInMotion = false;

		private Vector2 _tileStartingPosition;
		private Vector2 _mousePositionPreviousFrame;

		public void StartMoving(Vector2 mouseStartingposition)
		{
			if (tileIsInMotion)
			{
				return;
			}

			tileIsInMotion = true;

			_tileStartingPosition = transform.position;

			_mousePositionPreviousFrame = Camera.main.ScreenToWorldPoint(mouseStartingposition);

			transform.position = new Vector3(_tileStartingPosition.x, _tileStartingPosition.y, zMovementWhenSliding);

			moveCoroutine = StartCoroutine(MoveTileCoroutine());
		}

		public void StopMoving()
		{
			if (!tileIsInMotion)
			{
				return;
			}

			StopCoroutine(moveCoroutine);

			moveX = moveY = false;

			moveCoroutine = null;

			tileIsInMotion = false;

			transform.position = _tileStartingPosition;
		}

		private IEnumerator MoveTileCoroutine()
		{
			while (true)
			{
				yield return null;

				MoveTile();
			}
		}

		private void MoveTile()
		{
			var mouseWorldPositionThisFrame = Camera.main.ScreenToWorldPoint(PointerMethods.GetMouseOrPointerPosition());

			if (mouseWorldPositionThisFrame.x == _mousePositionPreviousFrame.x
				&& mouseWorldPositionThisFrame.y == _mousePositionPreviousFrame.y)
			{
				return;
			}

			if (moveX == false && moveY == false)
			{
				if (Mathf.Abs(_mousePositionPreviousFrame.x - mouseWorldPositionThisFrame.x)
					>= Mathf.Abs(_mousePositionPreviousFrame.y - mouseWorldPositionThisFrame.y))
				{
					moveX = true;
				}
				else
				{
					moveY = true;
				}
			}

			if (moveX)
			{
				float xToMove = mouseWorldPositionThisFrame.x - _mousePositionPreviousFrame.x;
				transform.position = new Vector3(transform.position.x + xToMove, transform.position.y, transform.position.z);
			}
			else
			{
				float yToMove = mouseWorldPositionThisFrame.y - _mousePositionPreviousFrame.y;
				transform.position = new Vector3(transform.position.x, transform.position.y + yToMove, transform.position.z);
			}

			_mousePositionPreviousFrame = mouseWorldPositionThisFrame;
		}

		private void OnDisable()
		{
			StopAllCoroutines();
		}
	}
}
