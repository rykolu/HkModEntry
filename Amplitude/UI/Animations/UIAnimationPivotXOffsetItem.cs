using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationPivotXOffsetItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitPivotXAnchor>
	{
		public override string GetShortName()
		{
			return "PivotX Offset";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.PivotXAnchor.Offset;
			interpolator.Max = target.PivotXAnchor.Offset;
		}

		protected override void Apply(float value)
		{
			target.PivotXAnchor = target.PivotXAnchor.SetOffset(value);
		}
	}
}
