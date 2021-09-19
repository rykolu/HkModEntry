using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationHeightItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitPosition>
	{
		public override string GetShortName()
		{
			return "Height";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.Height;
			interpolator.Max = target.Height;
		}

		protected override void Apply(float value)
		{
			target.Height = value;
		}
	}
}
