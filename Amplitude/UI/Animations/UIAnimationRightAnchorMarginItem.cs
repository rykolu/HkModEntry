using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationRightAnchorMarginItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitRightBorderAnchor>
	{
		public override string GetShortName()
		{
			return "Right Margin";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.RightAnchor.Margin;
			interpolator.Max = target.RightAnchor.Margin;
		}

		protected override void Apply(float value)
		{
			target.RightAnchor = target.RightAnchor.SetMargin(value);
		}
	}
}
