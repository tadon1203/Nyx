using System;
using UnityEngine;

namespace Nyx.Core.Utils
{
	public static class MathUtils
	{
		public static float Lerp(float a, float b, float t)
		{
			return a + (b - a) * Math.Clamp(t, 0, 1);
		}

		public static float EaseInExpo(float t)
		{
			if (t == 0) return 0;
			return Mathf.Pow(2, 10 * (t - 1));
		}

		public static float EaseOutExpo(float t)
		{
			if (t == 1) return 1;
			return 1 - Mathf.Pow(2, -10 * t);
		}
	}
}
