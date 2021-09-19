using System;
using Amplitude.UI.Traits;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationRotationXItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitRotation>
	{
		public override string GetShortName()
		{
			return "Rotation X";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.Rotation.x;
			interpolator.Max = target.Rotation.x;
		}

		protected override void Apply(float value)
		{
			Vector3 rotation = target.Rotation;
			rotation.x = value;
			target.Rotation = rotation;
		}
	}
}
