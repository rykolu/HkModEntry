using System;

namespace Amplitude.UI.Interactables
{
	public interface IUIButton : IUIControl, IUIInteractable
	{
		bool HandleOnMouseDown { get; set; }

		bool HandleDoubleClicks { get; set; }

		event Action<IUIButton> LeftClick;

		event Action<IUIButton> MiddleClick;

		event Action<IUIButton> RightClick;

		event Action<IUIButton> DoubleLeftClick;

		event Action<IUIButton> DoubleMiddleClick;

		event Action<IUIButton> DoubleRightClick;
	}
}
