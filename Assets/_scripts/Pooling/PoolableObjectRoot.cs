using UnityEngine;

public class PoolableObjectRoot : MonoBehaviour
{
	[SerializeField]
	protected GameObject poolableObjectPrefab;
	public GameObject PoolableObjectPrefab { get => poolableObjectPrefab; set => poolableObjectPrefab = value; }
}
