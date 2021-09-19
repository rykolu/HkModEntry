namespace Amplitude.UI.Animations.Scene
{
	internal abstract class UIAnimationEditionEventArg
	{
		internal enum Type
		{
			Invalid,
			PropertiesChanged,
			ItemChanged
		}

		internal readonly Type EventType;

		protected UIAnimationEditionEventArg(Type eventType)
		{
			EventType = eventType;
		}

		protected static void Raise(UIAnimationEditionEventArg eventArgs)
		{
			UIAnimatorManager.Instance?.OnAssetChanged(eventArgs);
			eventArgs.Reset();
		}

		protected abstract void Reset();
	}
}
