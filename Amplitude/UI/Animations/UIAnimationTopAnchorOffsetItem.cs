using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationTopAnchorOffsetItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitTopBorderAnchor>
	{
		public override string GetShortName()
		{
			return "Top Offset";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.TopAnchor.Offset;
			interpolator.Max = target.TopAnchor.Offset;
		}

		protected override void Apply(float value)
		{
			target.TopAnchor = target.TopAnchor.SetOffset(value);
		}
	}
}
