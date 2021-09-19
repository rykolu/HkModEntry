using UnityEngine;

namespace Amplitude.UI
{
	public class FloatInterpolator : IInterpolator<float>
	{
		public void Interpolate(float origin, float target, float ratio, ref float result)
		{
			result = Mathf.LerpUnclamped(origin, target, ratio);
		}
	}
}
