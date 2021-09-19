using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationLeftAnchorMarginItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitLeftBorderAnchor>
	{
		public override string GetShortName()
		{
			return "Left Margin";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.LeftAnchor.Percent;
			interpolator.Max = target.LeftAnchor.Percent;
		}

		protected override void Apply(float value)
		{
			target.LeftAnchor = target.LeftAnchor.SetPercent(value);
		}
	}
}
