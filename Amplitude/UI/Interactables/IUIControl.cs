using System;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public interface IUIControl : IUIInteractable
	{
		UIEventBlockingMask BlockedEvents { get; set; }

		bool Focusable { get; set; }

		bool Focused { get; }

		event Action<IUIControl, Vector2> Tick;

		event Action<IUIControl, Vector2> MouseEnter;

		event Action<IUIControl, Vector2> MouseLeave;

		event Action<IUIControl, Vector2> LeftMouseDown;

		event Action<IUIControl, Vector2> LeftMouseUp;

		event Action<IUIControl, Vector2> MiddleMouseDown;

		event Action<IUIControl, Vector2> MiddleMouseUp;

		event Action<IUIControl, Vector2> RightMouseDown;

		event Action<IUIControl, Vector2> RightMouseUp;

		event Action<IUIControl> FocusGain;

		event Action<IUIControl> FocusLoss;

		event Action<IUIControl, float> MouseScroll;

		event Action<IUIControl, Vector2> AxisUpdate2D;

		event Action<IUIControl, KeyCode> KeyDown;

		event Action<IUIControl, KeyCode> KeyUp;

		void TryTriggerAudioEvent(StaticString interactivityEvent);

		void ResetState();
	}
}
