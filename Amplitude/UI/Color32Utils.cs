using UnityEngine;

namespace Amplitude.UI
{
	public static class Color32Utils
	{
		public static readonly Color32 White = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public static bool Equals(Color32 left, Color32 right)
		{
			if (left.r == right.r && left.g == right.g && left.b == right.b)
			{
				return left.a == right.a;
			}
			return false;
		}
	}
}
