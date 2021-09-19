using Amplitude.UI.Animations.Data;

namespace Amplitude.UI.Animations.Scene
{
	internal class UIAnimationEditionItemChangedEventArg : UIAnimationEditionEventArg
	{
		internal UIAnimationAsset Asset;

		internal int AnimationItemIndex;

		private static UIAnimationEditionItemChangedEventArg self = new UIAnimationEditionItemChangedEventArg();

		private UIAnimationEditionItemChangedEventArg()
			: base(Type.ItemChanged)
		{
		}

		internal static void Raise(UIAnimationAsset asset, int animationItemIndex)
		{
		}

		protected override void Reset()
		{
			Asset = null;
			AnimationItemIndex = -1;
		}
	}
}
