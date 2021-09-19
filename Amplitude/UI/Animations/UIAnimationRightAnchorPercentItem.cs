using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationRightAnchorPercentItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitRightBorderAnchor>
	{
		public override string GetShortName()
		{
			return "Right Percent";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.RightAnchor.Percent;
			interpolator.Max = target.RightAnchor.Percent;
		}

		protected override void Apply(float value)
		{
			target.RightAnchor = target.RightAnchor.SetPercent(value);
		}
	}
}
