using System;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationColorInterpolator : UIAnimationInterpolator<Color>
	{
		public override Color Interpolate(float t)
		{
			return Color.LerpUnclamped(Min.linear, Max.linear, t).gamma;
		}
	}
}
