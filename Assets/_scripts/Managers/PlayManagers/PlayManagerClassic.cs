using System.Collections.Generic;
using UnityEngine;

namespace WordSlide
{
	public class PlayManagerClassic : PlayManagerAbstract
	{
		protected override void TilesSwappedByUser(List<SingleTileManagerSequence> rowsAndColumnsToCheck)
		{
			_gameStateEventHandler.RaisePlayerCanInteractWithTilesChanged(false);

			if (rowsAndColumnsToCheck.Count == 0)
			{
				_gameStateEventHandler.RaisePlayerCanInteractWithTilesChanged(true);
				return;
			}

			var validWords = FindWords(rowsAndColumnsToCheck);

			validWords.ForEach(x => Debug.Log($"{x.ToString()}"));

			if (validWords.Count == 0)
			{
				_gameStateEventHandler.RaisePlayerCanInteractWithTilesChanged(true);
				return;
			}

			List<SingleTileManager> tilesToDestroy = new();

			foreach (var word in validWords)
			{
				foreach (var singleTileManager in word.SingleTileManagers)
				{
					// TODO: Need to make sure that we still count this when working out points,
					// we just don't want to try and double destroy the same tile.
					if (tilesToDestroy.Contains(singleTileManager))
					{
						continue;
					}

					tilesToDestroy.Add(singleTileManager);
				}
			}

			TilesManager.Instance.DestroyTiles(tilesToDestroy);
		}
	}
}
