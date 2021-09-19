using System;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationVector3Interpolator : UIAnimationInterpolator<Vector3>
	{
		public override Vector3 Interpolate(float t)
		{
			return Vector3.LerpUnclamped(Min, Max, t);
		}
	}
}
