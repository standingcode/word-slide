using System;
using UnityEngine;

namespace Pooling
{
	public class PoolObject : MonoBehaviour
	{
		[SerializeField]
		protected Type poolObjectIdentifier;
		public Type PoolObjectIdentifier { get => poolObjectIdentifier; set => poolObjectIdentifier = value; }
	}
}
