namespace Amplitude.UI.Animations
{
	public interface IUIAnimationItemInspectable : IUIAnimationItem
	{
		UIComponent Target { get; }

		float CurrentTime { get; }
	}
}
