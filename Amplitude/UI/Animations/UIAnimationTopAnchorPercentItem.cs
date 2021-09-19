using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationTopAnchorPercentItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitTopBorderAnchor>
	{
		public override string GetShortName()
		{
			return "Top Percent";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.TopAnchor.Percent;
			interpolator.Max = target.TopAnchor.Percent;
		}

		protected override void Apply(float value)
		{
			target.TopAnchor = target.TopAnchor.SetPercent(value);
		}
	}
}
