using NativeSerializableDictionary;
using UnityEngine;

namespace Pooling
{
	public class PoolManager : MonoBehaviour
	{
		[SerializeField]
		SerializableDictionary<string, PoolableObjectRoot> poolableObjectRoots;

		public static PoolManager Instance { get; private set; }

		private void Awake()
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

		public PoolObject GetObjectFromPool(string objectIdentifier, Transform parentForPoolObjectTransform = null)
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

			PoolObject poolObject = poolableObjectRoot.transform.GetChild(0).GetComponent<PoolObject>();

			if (poolObject.PoolObjectIdentifier == null)
			{
				poolObject.PoolObjectIdentifier = objectIdentifier;
			}

			if (parentForPoolObjectTransform != null)
			{
				poolObject.transform.SetParent(parentForPoolObjectTransform);
			}

			return poolObject;
		}

		public void ReturnObjectToPool(PoolObject poolObject)
		{
			poolObject.transform.SetParent(poolableObjectRoots[poolObject.PoolObjectIdentifier].transform);
			poolObject.gameObject.SetActive(false);
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