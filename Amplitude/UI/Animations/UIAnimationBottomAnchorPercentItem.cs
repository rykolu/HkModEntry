using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationBottomAnchorPercentItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitBottomBorderAnchor>
	{
		public override string GetShortName()
		{
			return "Bottom Percent";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.BottomAnchor.Percent;
			interpolator.Max = target.BottomAnchor.Percent;
		}

		protected override void Apply(float value)
		{
			target.BottomAnchor = target.BottomAnchor.SetPercent(value);
		}
	}
}
