using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace Nyx.Core.Utils;

public class UnityMainThreadDispatcher : MonoBehaviour
{
	private static readonly Queue<Action> ExecutionQueue = new();
	private static UnityMainThreadDispatcher _instance;

	private void Awake()
	{
		_instance = this;
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	private void Update()
	{
		lock (ExecutionQueue)
		{
			while (ExecutionQueue.Count > 0)
			{
				ExecutionQueue.Dequeue().Invoke();
			}
		}
	}

	public static void Enqueue(IEnumerator action)
	{
		lock (ExecutionQueue)
		{
			ExecutionQueue.Enqueue(() =>
			{
				_instance.StartCoroutine(action.WrapToIl2Cpp());
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