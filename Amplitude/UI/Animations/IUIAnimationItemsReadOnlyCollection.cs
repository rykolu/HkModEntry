namespace Amplitude.UI.Animations
{
	public interface IUIAnimationItemsReadOnlyCollection
	{
		int Length { get; }

		IUIAnimationItem this[int index] { get; }
	}
}
