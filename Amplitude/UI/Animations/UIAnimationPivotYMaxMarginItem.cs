using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationPivotYMaxMarginItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitPivotYAnchor>
	{
		public override string GetShortName()
		{
			return "PivotY MaxMargin";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.PivotYAnchor.MaxMargin;
			interpolator.Max = target.PivotYAnchor.MaxMargin;
		}

		protected override void Apply(float value)
		{
			target.PivotYAnchor = target.PivotYAnchor.SetMaxMargin(value);
		}
	}
}
