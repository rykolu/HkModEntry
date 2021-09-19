using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationWidthItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitPosition>
	{
		public override string GetShortName()
		{
			return "Width";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.Width;
			interpolator.Max = target.Width;
		}

		protected override void Apply(float value)
		{
			target.Width = value;
		}
	}
}
