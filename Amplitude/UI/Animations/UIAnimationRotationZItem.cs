using System;
using Amplitude.UI.Traits;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationRotationZItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitRotation>
	{
		public override string GetShortName()
		{
			return "Rotation Z";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.Rotation.z;
			interpolator.Max = target.Rotation.z;
		}

		protected override void Apply(float value)
		{
			Vector3 rotation = target.Rotation;
			rotation.z = value;
			target.Rotation = rotation;
		}
	}
}
