using UnityEngine;

namespace Amplitude.UI
{
	public class Vector3Interpolator : IInterpolator<Vector3>
	{
		public void Interpolate(Vector3 origin, Vector3 target, float ratio, ref Vector3 result)
		{
			result = Vector3.LerpUnclamped(origin, target, ratio);
		}
	}
}
