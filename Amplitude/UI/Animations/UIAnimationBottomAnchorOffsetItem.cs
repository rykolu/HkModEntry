using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationBottomAnchorOffsetItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitBottomBorderAnchor>
	{
		public override string GetShortName()
		{
			return "Bottom Offset";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.BottomAnchor.Offset;
			interpolator.Max = target.BottomAnchor.Offset;
		}

		protected override void Apply(float value)
		{
			target.BottomAnchor = target.BottomAnchor.SetOffset(value);
		}
	}
}
