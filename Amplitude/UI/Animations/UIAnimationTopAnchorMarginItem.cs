using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationTopAnchorMarginItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitTopBorderAnchor>
	{
		public override string GetShortName()
		{
			return "Top Margin";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.TopAnchor.Margin;
			interpolator.Max = target.TopAnchor.Margin;
		}

		protected override void Apply(float value)
		{
			target.TopAnchor = target.TopAnchor.SetMargin(value);
		}
	}
}
