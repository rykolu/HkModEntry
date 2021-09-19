namespace Amplitude.UI.Interactables
{
	public interface IUIScrollView : IUIControl, IUIInteractable
	{
		UITransform Content { get; }

		UITransform Viewport { get; }

		bool ScrollHorizontally { get; }

		bool ScrollVertically { get; }

		IUIScrollBar HorizontalScrollBar { get; }

		IUIScrollBar VerticalScrollBar { get; }

		HorizontalDirection HorizontalScrollingDirection { get; }

		VerticalDirection VerticalScrollingDirection { get; }

		float HorizontalScrollingSpeed { get; }

		float VerticalScrollingSpeed { get; }

		bool AutoAdjustWidth { get; }

		float MinWidth { get; }

		float MaxWidth { get; set; }

		bool AutoAdjustHeight { get; }

		float MinHeight { get; }

		float MaxHeight { get; set; }

		float LeftMargin { get; set; }

		float RightMargin { get; set; }

		float TopMargin { get; set; }

		float BottomMargin { get; set; }

		bool OnlyBlockMouseScrollIfActive { get; set; }

		bool IgnoreMouseScrollEvent { get; set; }

		void ResetHorizontally(bool toEnd = false);

		void ResetVertically(bool toEnd = false);
	}
}
