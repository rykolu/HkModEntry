using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationPivotXMinMarginItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitPivotXAnchor>
	{
		public override string GetShortName()
		{
			return "PivotX MinMargin";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.PivotXAnchor.MinMargin;
			interpolator.Max = target.PivotXAnchor.MinMargin;
		}

		protected override void Apply(float value)
		{
			target.PivotXAnchor = target.PivotXAnchor.SetMinMargin(value);
		}
	}
}
