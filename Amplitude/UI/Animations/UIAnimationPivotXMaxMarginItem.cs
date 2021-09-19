using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationPivotXMaxMarginItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitPivotXAnchor>
	{
		public override string GetShortName()
		{
			return "PivotX MaxMargin";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.PivotXAnchor.MaxMargin;
			interpolator.Max = target.PivotXAnchor.MaxMargin;
		}

		protected override void Apply(float value)
		{
			target.PivotXAnchor = target.PivotXAnchor.SetMaxMargin(value);
		}
	}
}
