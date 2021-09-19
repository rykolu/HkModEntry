using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationPivotYOffsetItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitPivotYAnchor>
	{
		public override string GetShortName()
		{
			return "PivotY Offset";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.PivotYAnchor.Offset;
			interpolator.Max = target.PivotYAnchor.Offset;
		}

		protected override void Apply(float value)
		{
			target.PivotYAnchor = target.PivotYAnchor.SetOffset(value);
		}
	}
}
