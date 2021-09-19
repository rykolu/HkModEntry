using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationYItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitPosition>
	{
		public override string GetShortName()
		{
			return "Y";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.Y;
			interpolator.Max = target.Y;
		}

		protected override void Apply(float value)
		{
			target.Y = value;
		}
	}
}
