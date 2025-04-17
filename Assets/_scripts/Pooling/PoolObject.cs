using UnityEngine;

namespace Pooling
{
	public class PoolObject : MonoBehaviour
	{
		[SerializeField]
		protected string poolObjectIdentifier;
		public string PoolObjectIdentifier { get => poolObjectIdentifier; set => poolObjectIdentifier = value; }
	}
}
