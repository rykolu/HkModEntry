using System;
using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationLeftAnchorOffsetItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitLeftBorderAnchor>
	{
		public override string GetShortName()
		{
			return "Left Offset";
		}

		protected override void InitValues()
		{
			interpolator.Min = target.LeftAnchor.Offset;
			interpolator.Max = target.LeftAnchor.Offset;
		}

		protected override void Apply(float value)
		{
			target.LeftAnchor = target.LeftAnchor.SetOffset(value);
		}
	}
}
