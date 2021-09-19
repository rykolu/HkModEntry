using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationBottomAnchorMarginItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitBottomBorderAnchor>
	{
		public override string GetShortName()
		{
			return "Bottom Margin";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.BottomAnchor.Margin;
			interpolator.Max = target.BottomAnchor.Margin;
		}

		protected override void Apply(float value)
		{
			target.BottomAnchor = target.BottomAnchor.SetMargin(value);
		}
	}
}
