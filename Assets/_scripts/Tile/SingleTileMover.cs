using System.Collections;
using UnityEngine;

namespace WordSlide
{
	public enum PlaneOfMovement
	{
		XAxis,
		YAxis
	}

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

		PlaneOfMovement? planeOfMovement = null;

		[SerializeField]
		private float distanceToConsiderCloseToCentre = 0.1f;

		Coroutine moveCoroutine;

		private MovementRestrictions _movementRestrictions;
		public MovementRestrictions MovementRestrictions => _movementRestrictions;

		private bool tileIsInMotion = false;

		private void Awake()
		{
			newPosition.z = zMovementWhenSliding;
		}

		public void SetMovementRestrictions(MovementRestrictions movementRestrictions)
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

			tileStartingPosition = transform.position;

			_pointerPositionPreviousFrame = Camera.main.ScreenToWorldPoint(mouseStartingposition);

			moveCoroutine = StartCoroutine(MoveTileCoroutine());
		}

		public void StopMoving()
		{
			if (!tileIsInMotion)
			{
				return;
			}

			StopCoroutine(moveCoroutine);

			moveCoroutine = null;
			tileIsInMotion = false;
			planeOfMovement = null;
		}

		private IEnumerator MoveTileCoroutine()
		{
			while (true)
			{
				yield return null;

				MoveTile();
			}
		}

		private Vector2 tileStartingPosition;
		private Vector2 _pointerPositionPreviousFrame;
		private Vector2 pointerWorldPositionThisFrame;
		private float xDistanceFromStartingPosition;
		private float yDistanceFromStartingPosition;
		private Vector3 newPosition;
		private Vector2 vectorToMove;
		private float xMinToEdgeOfTile => _movementRestrictions.xMin - (SizeManager.Instance.TileSize.x / 2f);
		private float xMaxToEdgeOfTile => _movementRestrictions.xMax + (SizeManager.Instance.TileSize.x / 2f);
		private float yMinToEdgeOfTile => _movementRestrictions.yMin - (SizeManager.Instance.TileSize.y / 2f);
		private float yMaxToEdgeOfTile => _movementRestrictions.yMax + (SizeManager.Instance.TileSize.y / 2f);

		private void MoveTile()
		{
			pointerWorldPositionThisFrame = Camera.main.ScreenToWorldPoint(PointerMethods.GetMouseOrPointerPosition());

			// No point in doing anything if the pointer hasn't moved
			if (pointerWorldPositionThisFrame == _pointerPositionPreviousFrame)
			{
				return;
			}

			// Get the vector that the point has moved
			vectorToMove = pointerWorldPositionThisFrame - _pointerPositionPreviousFrame;

			// If we are close to centre, use direction as intent
			if (closeToCenter())
			{
				planeOfMovement = Mathf.Abs(vectorToMove.x) >= Mathf.Abs(vectorToMove.y) ? PlaneOfMovement.XAxis : PlaneOfMovement.YAxis;
			}
			// Otherwise the plane should be selected based on tile position compared to original position
			else
			{
				xDistanceFromStartingPosition = Mathf.Abs(transform.position.x - tileStartingPosition.x);
				yDistanceFromStartingPosition = Mathf.Abs(transform.position.y - tileStartingPosition.y);

				planeOfMovement = xDistanceFromStartingPosition >= yDistanceFromStartingPosition ? PlaneOfMovement.XAxis : PlaneOfMovement.YAxis;
			}


			// If we are moving in the x axis, restrict the movement.
			// (It's not enough to just clamp the position as if the pointer moves quickly the tile can get stuck not at the max position)			
			if (planeOfMovement == PlaneOfMovement.XAxis)
			{
				// Clamp to xMin
				if (pointerWorldPositionThisFrame.x < xMinToEdgeOfTile)
				{
					newPosition.x = _movementRestrictions.xMin;
				}
				// Clamp to xMax
				else if (pointerWorldPositionThisFrame.x > xMaxToEdgeOfTile)
				{
					newPosition.x = _movementRestrictions.xMax;
				}
				// Set the position normally
				else
				{
					newPosition.x = Mathf.Clamp(transform.position.x + vectorToMove.x, _movementRestrictions.xMin, _movementRestrictions.xMax);
				}

				// Always set y to default when moving in the x plane
				newPosition.y = tileStartingPosition.y;
			}
			// And if we're moving in the y axis, restrict the movement.
			else
			{
				// Clamp to yMin
				if (pointerWorldPositionThisFrame.y < yMinToEdgeOfTile)
				{
					newPosition.y = _movementRestrictions.yMin;
				}
				// Clamp to yMax
				else if (pointerWorldPositionThisFrame.y > yMaxToEdgeOfTile)
				{
					newPosition.y = _movementRestrictions.yMax;
				}
				// Set the position normally
				else
				{
					newPosition.y = Mathf.Clamp(transform.position.y + vectorToMove.y, _movementRestrictions.yMin, _movementRestrictions.yMax);
				}

				// Always set x to default position when moving in the y plane
				newPosition.x = tileStartingPosition.x;
			}

			// Move the tile to the new position
			transform.position = newPosition;

			// Save the pointer location this frame
			_pointerPositionPreviousFrame = pointerWorldPositionThisFrame;
		}

		private bool closeToCenter()
		{
			return Vector2.Distance(transform.position, tileStartingPosition) < distanceToConsiderCloseToCentre;
		}

		private void OnDisable()
		{
			StopAllCoroutines();
		}
	}
}
