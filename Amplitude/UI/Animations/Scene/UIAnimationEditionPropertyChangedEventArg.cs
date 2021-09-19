using Amplitude.UI.Animations.Data;

namespace Amplitude.UI.Animations.Scene
{
	internal class UIAnimationEditionPropertyChangedEventArg : UIAnimationEditionEventArg
	{
		internal UIAnimationAsset Asset;

		private static UIAnimationEditionPropertyChangedEventArg self = new UIAnimationEditionPropertyChangedEventArg();

		private UIAnimationEditionPropertyChangedEventArg()
			: base(Type.PropertiesChanged)
		{
		}

		internal static void Raise(UIAnimationAsset asset)
		{
		}

		protected override void Reset()
		{
			Asset = null;
		}
	}
}
