using NativeSerializableDictionary;
using System;
using UnityEditor.Build.Content;
using UnityEngine;

namespace Pooling
{
	public class PoolManager : MonoBehaviour
	{
		[SerializeField]
		SerializableDictionary<string, PoolableObjectRoot> poolableObjectRoots;

		public static PoolManager Instance { get; private set; }

		public void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this);
				return;
			}

			Instance = this;
		}

		private void OnDestroy()
		{
			Instance = null;
		}

		public Transform GetObjectFromPool(string objectIdentifier, Transform parentForPoolObjectTransform = null)
		{
			if (!poolableObjectRoots.ContainsKey(objectIdentifier))
			{
				return null;
			}

			PoolableObjectRoot poolableObjectRoot = poolableObjectRoots[objectIdentifier];

			if (poolableObjectRoot.transform.childCount == 0)
			{
				GameObject instantiatedObject = Instantiate(poolableObjectRoot.PoolableObjectPrefab);
				instantiatedObject.transform.SetParent(poolableObjectRoot.transform);
			}

			var poolableObjectTransform = poolableObjectRoot.transform.GetChild(0);

			if (parentForPoolObjectTransform != null)
			{
				poolableObjectTransform.transform.SetParent(parentForPoolObjectTransform);
			}

			poolableObjectTransform.gameObject.SetActive(true);

			return poolableObjectTransform;
		}

		public void ReturnObjectToPool(Transform objectTransform, string objectIdentifier)
		{
			objectTransform.transform.SetParent(poolableObjectRoots[objectIdentifier].transform);
			objectTransform.gameObject.SetActive(false);
		}

		public void InstantiateObjectsEnabled(string objectIdentifier, int amountToInstantiate)
		=> InstantiateObjects(objectIdentifier, amountToInstantiate, true);

		public void InstantiateObjectsDisabled(string objectIdentifier, int amountToInstantiate)
		=> InstantiateObjects(objectIdentifier, amountToInstantiate, false);

		public void InstantiateObjects(string objectIdentifier, int amountToInstantiate, bool objectsShouldBeEnabled)
		{
			PoolableObjectRoot poolableObjectRoot = poolableObjectRoots[objectIdentifier];

			for (int i = 0; i < amountToInstantiate; i++)
			{
				GameObject instantiatedObject = Instantiate(poolableObjectRoot.PoolableObjectPrefab);
				instantiatedObject.transform.parent = poolableObjectRoot.transform;
				instantiatedObject.SetActive(objectsShouldBeEnabled);
			}
		}
	}
}