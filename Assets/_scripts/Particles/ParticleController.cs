using Pooling;
using System;
using System.Collections;
using UnityEngine;

public abstract class ParticleController : MonoBehaviour
{
	[SerializeField]
	protected ParticleSystem particleSystem;

	[SerializeField]
	float secondsUntilCallback = 1f;
	float countdown = 0f;

	public void PlayParticleAnimationAndCallBack(Action<Transform> callBack)
	{
		particleSystem.Play();
		countdown = secondsUntilCallback;
		StartCoroutine(WaitForSecondsToElapseAndCallBack(callBack));
	}

	protected IEnumerator WaitForSecondsToElapseAndCallBack(Action<Transform> callBack)
	{
		while (countdown > 0)
		{
			countdown -= Time.deltaTime;
			yield return null;
		}

		// Calls back to report the particle sequence is complete
		callBack?.Invoke(transform);
	}

	protected void OnDestroy()
	{
		StopAllCoroutines();
	}
}
