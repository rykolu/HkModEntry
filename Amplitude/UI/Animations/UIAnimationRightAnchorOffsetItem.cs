using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationRightAnchorOffsetItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitRightBorderAnchor>
	{
		public override string GetShortName()
		{
			return "Right Offset";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.RightAnchor.Offset;
			interpolator.Max = target.RightAnchor.Offset;
		}

		protected override void Apply(float value)
		{
			target.RightAnchor = target.RightAnchor.SetOffset(value);
		}
	}
}
