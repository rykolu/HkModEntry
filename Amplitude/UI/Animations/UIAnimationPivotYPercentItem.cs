using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationPivotYPercentItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitPivotYAnchor>
	{
		public override string GetShortName()
		{
			return "PivotY Percent";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.PivotYAnchor.Percent;
			interpolator.Max = target.PivotYAnchor.Percent;
		}

		protected override void Apply(float value)
		{
			target.PivotYAnchor = target.PivotYAnchor.SetPercent(value);
		}
	}
}
