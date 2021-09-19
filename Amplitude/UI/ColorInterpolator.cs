using UnityEngine;

namespace Amplitude.UI
{
	public class ColorInterpolator : IInterpolator<Color>
	{
		public void Interpolate(Color origin, Color target, float ratio, ref Color result)
		{
			result = Color.Lerp(origin.linear, target.linear, ratio).gamma;
		}
	}
}
