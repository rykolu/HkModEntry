using System;
using Amplitude.UI.Traits;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationRotationYItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitRotation>
	{
		public override string GetShortName()
		{
			return "Rotation Y";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.Rotation.y;
			interpolator.Max = target.Rotation.y;
		}

		protected override void Apply(float value)
		{
			Vector3 rotation = target.Rotation;
			rotation.y = value;
			target.Rotation = rotation;
		}
	}
}
