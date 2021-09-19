using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationPivotXPercentItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitPivotXAnchor>
	{
		public override string GetShortName()
		{
			return "PivotX Percent";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.PivotXAnchor.Percent;
			interpolator.Max = target.PivotXAnchor.Percent;
		}

		protected override void Apply(float value)
		{
			target.PivotXAnchor = target.PivotXAnchor.SetPercent(value);
		}
	}
}
