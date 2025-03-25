using System;

namespace Nyx.Core.Utils;

public static class MathUtils
{
	public static float Lerp(float a, float b, float t)
	{
		return a + (b - a) * Math.Clamp(t, 0, 1);
	}
}