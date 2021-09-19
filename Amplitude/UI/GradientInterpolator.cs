using System;
using UnityEngine;

namespace Amplitude.UI
{
	public class GradientInterpolator : IInterpolator<UIGradient>
	{
		public void Interpolate(UIGradient origin, UIGradient target, float ratio, ref UIGradient result)
		{
			if (result == null)
			{
				result = new UIGradient();
			}
			FixOriginTargetIfNecessary(origin, target);
			int num = ((origin.ColorKeys != null) ? origin.ColorKeys.Length : 0);
			GradientColorKey[] array = new GradientColorKey[num];
			for (int i = 0; i < num; i++)
			{
				GradientColorKey gradientColorKey = (array[i] = new GradientColorKey(Color.Lerp(origin.ColorKeys[i].color.linear, target.ColorKeys[i].color.linear, ratio).gamma, Mathf.Lerp(origin.ColorKeys[i].time, target.ColorKeys[i].time, ratio)));
			}
			result.ColorKeys = array;
			int num2 = ((origin.AlphaKeys != null) ? origin.AlphaKeys.Length : 0);
			GradientAlphaKey[] array2 = new GradientAlphaKey[num2];
			for (int j = 0; j < num2; j++)
			{
				GradientAlphaKey gradientAlphaKey = (array2[j] = new GradientAlphaKey(Mathf.Lerp(origin.AlphaKeys[j].alpha, target.AlphaKeys[j].alpha, ratio), Mathf.Lerp(origin.AlphaKeys[j].time, target.AlphaKeys[j].time, ratio)));
			}
			result.AlphaKeys = array2;
			result.Orientation = target.Orientation;
			if (origin.Orientation == target.Orientation)
			{
				result.LineAngle = Mathf.Lerp(origin.LineAngle, target.LineAngle, ratio);
				result.RadialPosition = Vector2.Lerp(origin.RadialPosition, target.RadialPosition, ratio);
			}
			else
			{
				result.LineAngle = target.LineAngle;
				result.RadialPosition = target.RadialPosition;
			}
		}

		private void FixOriginTargetIfNecessary(UIGradient origin, UIGradient target)
		{
			int num = ((origin.ColorKeys != null) ? origin.ColorKeys.Length : 0);
			int num2 = ((target.ColorKeys != null) ? target.ColorKeys.Length : 0);
			int num3 = ((origin.AlphaKeys != null) ? origin.AlphaKeys.Length : 0);
			int num4 = ((target.AlphaKeys != null) ? target.AlphaKeys.Length : 0);
			if (num != num2)
			{
				int num5 = Mathf.Min(num, num2);
				int num6 = Mathf.Max(num, num2);
				GradientColorKey[] sourceArray = ((num > num2) ? target.ColorKeys : origin.ColorKeys);
				GradientColorKey[] array = new GradientColorKey[num6];
				Array.Copy(sourceArray, array, num5);
				float time = array[num6 - 1].time;
				Color color = array[num6 - 1].color;
				for (int i = num5; i < num6; i++)
				{
					array[i] = new GradientColorKey(color, time);
				}
				if (num > num2)
				{
					target.ColorKeys = array;
				}
				else
				{
					origin.ColorKeys = array;
				}
			}
			if (num3 != num4)
			{
				int num7 = Mathf.Min(num3, num4);
				int num8 = Mathf.Max(num3, num4);
				GradientAlphaKey[] sourceArray2 = ((num3 > num4) ? target.AlphaKeys : origin.AlphaKeys);
				GradientAlphaKey[] array2 = new GradientAlphaKey[num8];
				Array.Copy(sourceArray2, array2, num7);
				float time2 = array2[num8 - 1].time;
				float alpha = array2[num8 - 1].alpha;
				for (int j = num7; j < num8; j++)
				{
					array2[j] = new GradientAlphaKey(alpha, time2);
				}
				if (num > num2)
				{
					target.AlphaKeys = array2;
				}
				else
				{
					origin.AlphaKeys = array2;
				}
			}
		}
	}
}
