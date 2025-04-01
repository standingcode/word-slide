using NativeSerializableDictionary;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
	[SerializeField]
	SerializableDictionary<string, PoolableObjectRoot> poolableObjectRoots;

	public PoolObject GetObjectFromPool(string objectIdentifier)
	{
		if (!poolableObjectRoots.ContainsKey(objectIdentifier))
		{
			return null;
		}

		PoolableObjectRoot poolableObjectRoot = poolableObjectRoots[objectIdentifier];

		if (poolableObjectRoot.transform.childCount == 0)
		{
			Debug.Log("Instantiating");

			GameObject instantiatedObject = Instantiate(poolableObjectRoot.PoolableObjectPrefab);

			instantiatedObject.transform.parent = poolableObjectRoot.transform;
		}

		PoolObject poolObject = poolableObjectRoot.transform.GetChild(0).GetComponent<PoolObject>();

		if (poolObject.PoolObjectIdentifier == null)
		{
			poolObject.PoolObjectIdentifier = objectIdentifier;
		}

		return poolObject;
	}

	public void ReturnObjectToPool(PoolObject poolObject)
	{
		poolObject.transform.parent = poolableObjectRoots[poolObject.PoolObjectIdentifier].transform;
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