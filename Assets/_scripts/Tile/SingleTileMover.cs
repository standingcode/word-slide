using System.Collections;
using UnityEngine;

namespace WordSlide
{
	public struct MovementRestrictions
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

		private MovementRestrictions _movementRestrictions;

		private bool tileIsmovingOnXPlain = false;
		private bool tileIsMovingOnYPlain = false;

		private bool tileIsInMotion = false;

		private Vector2 _tileStartingPosition;
		private Vector2 _mousePositionPreviousFrame;

		public void InitializeMovementRestrictions(MovementRestrictions movementRestrictions)
		{
			_movementRestrictions = movementRestrictions;
		}

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

			tileIsmovingOnXPlain = tileIsMovingOnYPlain = false;

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

			if (tileIsmovingOnXPlain == false && tileIsMovingOnYPlain == false)
			{
				if (Mathf.Abs(_mousePositionPreviousFrame.x - mouseWorldPositionThisFrame.x)
					>= Mathf.Abs(_mousePositionPreviousFrame.y - mouseWorldPositionThisFrame.y))
				{
					tileIsmovingOnXPlain = true;
				}
				else
				{
					tileIsMovingOnYPlain = true;
				}
			}

			if (tileIsmovingOnXPlain)
			{
				float xToMove = mouseWorldPositionThisFrame.x - _mousePositionPreviousFrame.x;

				transform.position = new Vector3(
				Mathf.Clamp(transform.position.x + xToMove, _movementRestrictions.xMin, _movementRestrictions.xMax),
				transform.position.y,
				transform.position.z
				);
			}
			else if (tileIsMovingOnYPlain)
			{
				float yToMove = mouseWorldPositionThisFrame.y - _mousePositionPreviousFrame.y;

				transform.position = new Vector3(
				transform.position.x,
				Mathf.Clamp(transform.position.y + yToMove, _movementRestrictions.yMin, _movementRestrictions.yMax),
				transform.position.z
				);
			}

			_mousePositionPreviousFrame = mouseWorldPositionThisFrame;
		}

		private void OnDisable()
		{
			StopAllCoroutines();
		}
	}
}
