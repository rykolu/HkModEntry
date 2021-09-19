using System;
using Amplitude.UI.Traits;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationColorItem : UIAnimationItem<Color, UIAnimationColorInterpolator, IUITraitColor>
	{
		public override string GetShortName()
		{
			return "Color";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.Color;
			interpolator.Max = target.Color;
		}

		protected override void Apply(Color value)
		{
			target.Color = value;
		}
	}
}
