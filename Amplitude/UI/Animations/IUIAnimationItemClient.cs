namespace Amplitude.UI.Animations
{
	public interface IUIAnimationItemClient
	{
		float DurationFactor { get; }

		void OnItemStart(IUIAnimationItem item);

		void OnItemStop(IUIAnimationItem item);
	}
}
