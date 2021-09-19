using System;

namespace Amplitude.UI.Interactables
{
	public interface IUIToggle : IUIControl, IUIInteractable
	{
		bool State { get; set; }

		bool ClickDoesntSwitchOff { get; set; }

		bool HandleOnMouseUp { get; set; }

		bool HandleDoubleClicks { get; set; }

		bool WindowsExplorerLike { get; set; }

		event Action<IUIToggle, bool> Switch;

		event Action<IUIToggle, bool> DoubleLeftClick;
	}
}
