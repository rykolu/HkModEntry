using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationXItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitPosition>
	{
		public override string GetShortName()
		{
			return "X";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.X;
			interpolator.Max = target.X;
		}

		protected override void Apply(float value)
		{
			target.X = value;
		}
	}
}
