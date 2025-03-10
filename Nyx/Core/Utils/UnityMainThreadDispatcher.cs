using BepInEx.Unity.IL2CPP.Utils.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nyx.Core.Utils
{
	public class UnityMainThreadDispatcher : MonoBehaviour
	{
		private static readonly Queue<Action> executionQueue = new();
		private static UnityMainThreadDispatcher instance;

		private void Awake()
		{
			instance = this;
		}

		private void OnDestroy()
		{
			instance = null;
		}

		private void Update()
		{
			lock (executionQueue)
			{
				while (executionQueue.Count > 0)
				{
					executionQueue.Dequeue().Invoke();
				}
			}
		}

		public static void Enqueue(IEnumerator action)
		{
			lock (executionQueue)
			{
				executionQueue.Enqueue(() =>
				{
					instance.StartCoroutine(action.WrapToIl2Cpp());
				});
			}
		}

		public static void Enqueue(Action action)
		{
			Enqueue(ActionWrapper(action));
		}
		
		private static IEnumerator ActionWrapper(Action action)
		{
			action();
			yield return null;
		}
	}
}
