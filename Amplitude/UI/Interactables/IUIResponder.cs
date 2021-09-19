namespace Amplitude.UI.Interactables
{
	public interface IUIResponder
	{
		int ResponderIndex { get; set; }

		bool IsInteractive { get; }

		bool IsMask { get; }

		bool Hovered { get; }

		SortedResponder GetSortedResponder(UIView view, int viewGroupCullingMask);

		bool TryCatchEvent(ref InputEvent inputEvent);
	}
}
