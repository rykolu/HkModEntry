using System;
using Amplitude.UI.Traits;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationScale : UIAnimationItem<Vector3, UIAnimationVector3Interpolator, IUITraitScale>
	{
		public override string GetShortName()
		{
			return "Scale";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.Scale;
			interpolator.Max = target.Scale;
		}

		protected override void Apply(Vector3 value)
		{
			target.Scale = value;
		}
	}
}
