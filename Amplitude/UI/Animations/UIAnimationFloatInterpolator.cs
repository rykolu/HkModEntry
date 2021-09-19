using System;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationFloatInterpolator : UIAnimationInterpolator<float>
	{
		public override float Interpolate(float t)
		{
			return Mathf.LerpUnclamped(Min, Max, t);
		}
	}
}
