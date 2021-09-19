using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationLeftAnchorPercentItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitLeftBorderAnchor>
	{
		public override string GetShortName()
		{
			return "Left Percent";
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
