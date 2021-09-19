using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationPivotYMinMarginItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitPivotYAnchor>
	{
		public override string GetShortName()
		{
			return "PivotY MinMargin";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.PivotYAnchor.MinMargin;
			interpolator.Max = target.PivotYAnchor.MinMargin;
		}

		protected override void Apply(float value)
		{
			target.PivotYAnchor = target.PivotYAnchor.SetMinMargin(value);
		}
	}
}
