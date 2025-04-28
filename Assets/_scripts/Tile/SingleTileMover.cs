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


		private float positionToMoveToX;
		private float positionToMoveToY;
		private void MoveTile()
		{
			Debug.Log("Moving tile");

			var mouseWorldPositionThisFrame = Camera.main.ScreenToWorldPoint(PointerMethods.GetMouseOrPointerPosition());

			positionToMoveToX = Mathf.Clamp(mouseWorldPositionThisFrame.x, _movementRestrictions.xMin, _movementRestrictions.xMax);
			positionToMoveToY = Mathf.Clamp(mouseWorldPositionThisFrame.y, _movementRestrictions.yMin, _movementRestrictions.yMax);

			float xDistanceFromStartingPosition = Mathf.Abs(positionToMoveToX - _tileStartingPosition.x);
			float yDistanceFromStartingPosition = Mathf.Abs(positionToMoveToY - _tileStartingPosition.y);

			if (xDistanceFromStartingPosition >= yDistanceFromStartingPosition)
			{
				transform.position = new Vector3(
				Mathf.Clamp(mouseWorldPositionThisFrame.x, _movementRestrictions.xMin, _movementRestrictions.xMax),
				_tileStartingPosition.y,
				transform.position.z
				);
			}
			else
			{
				transform.position = new Vector3(
				_tileStartingPosition.x,
				Mathf.Clamp(mouseWorldPositionThisFrame.y, _movementRestrictions.yMin, _movementRestrictions.yMax),
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
