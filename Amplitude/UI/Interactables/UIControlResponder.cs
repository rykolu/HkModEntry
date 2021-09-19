using System;
using Amplitude.Framework.Input;
using Amplitude.UI.Audio;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public class UIControlResponder : UIResponder
	{
		[NonSerialized]
		private UIReactivityState reactivityState;

		[NonSerialized]
		private UIReactivityState previousReactivityState;

		public ref UIReactivityState ReactivityState => ref reactivityState;

		public bool Focused => UIInteractivityManager.Instance.FocusedResponder == this;

		private IUIControl Control => base.Interactable as IUIControl;

		public event Action<IUIControl, Vector2> Tick;

		public event Action<IUIControl, Vector2> MouseEnter;

		public event Action<IUIControl, Vector2> MouseLeave;

		public event Action<IUIControl, Vector2> LeftMouseDown;

		public event Action<IUIControl, Vector2> LeftMouseUp;

		public event Action<IUIControl, Vector2> MiddleMouseDown;

		public event Action<IUIControl, Vector2> MiddleMouseUp;

		public event Action<IUIControl, Vector2> RightMouseDown;

		public event Action<IUIControl, Vector2> RightMouseUp;

		public event Action<IUIControl> FocusGain;

		public event Action<IUIControl> FocusLoss;

		public event Action<IUIControl, float> MouseScroll;

		public event Action<IUIControl, Vector2> AxisUpdate2D;

		public event Action<IUIControl, KeyCode> KeyDown;

		public event Action<IUIControl, KeyCode> KeyUp;

		public event Action<IUIControl, char> NewChar;

		public UIControlResponder(IUIControl control)
			: base(control)
		{
		}

		public void UpdateReactivity(bool instant = false)
		{
			previousReactivityState.Clear();
			previousReactivityState.AddTagsFrom(ref reactivityState);
			reactivityState.Clear();
			DoUpdateReactivity(ref reactivityState);
			if (!reactivityState.IsSameAs(ref previousReactivityState))
			{
				Control.GetUITransform().OnReactivityChanged(reactivityState, Control.GetUITransform(), instant);
			}
		}

		public virtual void ResetState()
		{
			if (hovered && !base.IsInteractive)
			{
				Vector2 mousePosition = UIInteractivityManager.Instance.GetMousePosition(UIHierarchyManager.Instance.MainFullscreenView);
				OnMouseLeave(mousePosition);
			}
		}

		public override bool TryCatchEvent(ref InputEvent inputEvent)
		{
			if ((inputEvent.Type & InputEvent.EventType.Tick & eventSensitivity) != 0 && base.IsInteractive)
			{
				OnTick(ref inputEvent);
			}
			bool isInteractive = base.IsInteractive;
			if (inputEvent.Type == InputEvent.EventType.KeyDown)
			{
				return CatchKeyDownEvent(ref inputEvent, isInteractive);
			}
			if (inputEvent.Type == InputEvent.EventType.KeyUp)
			{
				return CatchKeyUpEvent(ref inputEvent, isInteractive);
			}
			if (inputEvent.Type == InputEvent.EventType.NewChar)
			{
				return CatchNewCharEvent(ref inputEvent, isInteractive);
			}
			bool isMouseInside = Contains(inputEvent.MousePosition) && UIHierarchyManager.Instance.MainFullscreenView.RenderedRect.Contains(inputEvent.MousePosition);
			if ((inputEvent.Type & InputEvent.EventType.MouseHover & eventSensitivity) != 0)
			{
				return CatchMouseHoverEvent(ref inputEvent, isMouseInside, isInteractive);
			}
			if (inputEvent.Type == InputEvent.EventType.CastFocus && Control.Focusable)
			{
				return CatchMouseFocusEvent(ref inputEvent, isMouseInside, isInteractive);
			}
			if (inputEvent.Type == InputEvent.EventType.MouseDown)
			{
				return CatchMouseDownEvent(ref inputEvent, isMouseInside, isInteractive);
			}
			if (inputEvent.Type == InputEvent.EventType.MouseUp)
			{
				return CatchMouseUpEvent(ref inputEvent, isMouseInside, isInteractive);
			}
			if (inputEvent.Type == InputEvent.EventType.MouseScroll)
			{
				return CatchMouseScrollEvent(ref inputEvent, isMouseInside, isInteractive);
			}
			if (inputEvent.Type == InputEvent.EventType.AxisUpdate2D)
			{
				return CatchAxisUpdate2DEvent(ref inputEvent, isMouseInside, isInteractive);
			}
			return false;
		}

		internal virtual void OnFocusGain()
		{
			this.FocusGain?.Invoke(Control);
			UpdateReactivity();
		}

		internal virtual void OnFocusLoss()
		{
			this.FocusLoss?.Invoke(Control);
			UpdateReactivity();
		}

		protected virtual void OnTick(ref InputEvent tickEvent)
		{
			this.Tick?.Invoke(Control, tickEvent.MousePosition);
		}

		protected bool CatchMouseHoverEvent(ref InputEvent mouseEvent, bool isMouseInside, bool isInteractive)
		{
			if (isMouseInside && !mouseEvent.Catched)
			{
				if (isInteractive && !hovered)
				{
					OnMouseEnter(mouseEvent.MousePosition, isMouseInside);
				}
				return (Control.BlockedEvents & UIEventBlockingMask.MouseHover) != 0;
			}
			if (isInteractive && hovered)
			{
				OnMouseLeave(mouseEvent.MousePosition);
			}
			return false;
		}

		protected virtual void OnMouseEnter(Vector2 mousePosition, bool isMouseInside)
		{
			hovered = true;
			this.MouseEnter?.Invoke(Control, mousePosition);
			UpdateReactivity();
			Control.TryTriggerAudioEvent(AudioEvent.Interactivity.MouseEnter);
		}

		protected virtual void OnMouseLeave(Vector2 mousePosition)
		{
			hovered = false;
			this.MouseLeave?.Invoke(Control, mousePosition);
			UpdateReactivity();
			Control.TryTriggerAudioEvent(AudioEvent.Interactivity.MouseLeave);
		}

		protected bool CatchMouseFocusEvent(ref InputEvent focusEvent, bool isMouseInside, bool isInteractive)
		{
			if (isMouseInside && !focusEvent.Catched)
			{
				if (!Focused)
				{
					UIInteractivityManager.Instance.SetFocus(this);
				}
				return true;
			}
			if (Focused && (!isMouseInside || focusEvent.Catched))
			{
				UIInteractivityManager.Instance.SetFocus();
			}
			return false;
		}

		protected bool CatchMouseDownEvent(ref InputEvent mouseEvent, bool isMouseInside, bool isInteractive)
		{
			if (!isMouseInside || mouseEvent.Catched)
			{
				return false;
			}
			if (isInteractive)
			{
				OnMouseDown(ref mouseEvent, isMouseInside);
			}
			if (mouseEvent.Button == MouseButton.Left)
			{
				return (Control.BlockedEvents & UIEventBlockingMask.LeftMouseDown) != 0;
			}
			if (mouseEvent.Button == MouseButton.Middle)
			{
				return (Control.BlockedEvents & UIEventBlockingMask.MiddleMouseDown) != 0;
			}
			if (mouseEvent.Button == MouseButton.Right)
			{
				return (Control.BlockedEvents & UIEventBlockingMask.RightMouseDown) != 0;
			}
			return false;
		}

		protected virtual void OnMouseDown(ref InputEvent mouseEvent, bool isMouseInside)
		{
			if (mouseEvent.Button == MouseButton.Left)
			{
				this.LeftMouseDown?.Invoke(Control, mouseEvent.MousePosition);
			}
			else if (mouseEvent.Button == MouseButton.Middle)
			{
				this.MiddleMouseDown?.Invoke(Control, mouseEvent.MousePosition);
			}
			else if (mouseEvent.Button == MouseButton.Right)
			{
				this.RightMouseDown?.Invoke(Control, mouseEvent.MousePosition);
			}
		}

		protected bool CatchMouseUpEvent(ref InputEvent mouseEvent, bool isMouseInside, bool isInteractive)
		{
			if (isInteractive)
			{
				OnMouseUp(ref mouseEvent, isMouseInside);
			}
			if (!isMouseInside)
			{
				return false;
			}
			if (mouseEvent.Button == MouseButton.Left)
			{
				return (Control.BlockedEvents & UIEventBlockingMask.LeftMouseUp) != 0;
			}
			if (mouseEvent.Button == MouseButton.Middle)
			{
				return (Control.BlockedEvents & UIEventBlockingMask.MiddleMouseUp) != 0;
			}
			if (mouseEvent.Button == MouseButton.Right)
			{
				return (Control.BlockedEvents & UIEventBlockingMask.RightMouseUp) != 0;
			}
			return false;
		}

		protected virtual void OnMouseUp(ref InputEvent mouseEvent, bool isMouseInside)
		{
			if (isMouseInside)
			{
				if (mouseEvent.Button == MouseButton.Left)
				{
					this.LeftMouseUp?.Invoke(Control, mouseEvent.MousePosition);
				}
				else if (mouseEvent.Button == MouseButton.Middle)
				{
					this.MiddleMouseUp?.Invoke(Control, mouseEvent.MousePosition);
				}
				else if (mouseEvent.Button == MouseButton.Right)
				{
					this.RightMouseUp?.Invoke(Control, mouseEvent.MousePosition);
				}
			}
		}

		protected bool CatchMouseScrollEvent(ref InputEvent mouseEvent, bool isMouseInside, bool isInteractive)
		{
			if (!isMouseInside || mouseEvent.Catched)
			{
				return false;
			}
			if (isInteractive)
			{
				OnMouseScroll(ref mouseEvent, isMouseInside);
			}
			return (Control.BlockedEvents & UIEventBlockingMask.MouseScroll) != 0;
		}

		protected bool CatchAxisUpdate2DEvent(ref InputEvent axisUpdate2DEvent, bool isMouseInside, bool isInteractive)
		{
			if (!isMouseInside || axisUpdate2DEvent.Catched)
			{
				return false;
			}
			if (isInteractive)
			{
				OnAxisUpdate2D(ref axisUpdate2DEvent, isMouseInside);
			}
			return (Control.BlockedEvents & UIEventBlockingMask.AxisUpdate2D) != 0;
		}

		protected virtual void OnMouseScroll(ref InputEvent mouseScrollEvent, bool isMouseInside)
		{
			this.MouseScroll?.Invoke(Control, mouseScrollEvent.ScrollIncrement);
		}

		protected virtual void OnAxisUpdate2D(ref InputEvent axisUpdate2DEvent, bool isMouseInside)
		{
			this.AxisUpdate2D?.Invoke(Control, axisUpdate2DEvent.AxisUpdate2D);
		}

		protected bool CatchKeyDownEvent(ref InputEvent keyboardEvent, bool isInteractive)
		{
			if (isInteractive)
			{
				OnKeyDown(ref keyboardEvent);
			}
			return (Control.BlockedEvents & UIEventBlockingMask.KeyDown) != 0;
		}

		protected virtual void OnKeyDown(ref InputEvent keyboardEvent)
		{
			this.KeyDown?.Invoke(Control, (KeyCode)keyboardEvent.Key);
		}

		protected bool CatchKeyUpEvent(ref InputEvent keyboardEvent, bool isInteractive)
		{
			if (isInteractive)
			{
				OnKeyUp(ref keyboardEvent);
			}
			return (Control.BlockedEvents & UIEventBlockingMask.KeyUp) != 0;
		}

		protected virtual void OnKeyUp(ref InputEvent keyboardEvent)
		{
			this.KeyUp?.Invoke(Control, (KeyCode)keyboardEvent.Key);
		}

		protected bool CatchNewCharEvent(ref InputEvent keyboardEvent, bool isInteractive)
		{
			if (isInteractive)
			{
				OnNewChar(ref keyboardEvent);
			}
			return (Control.BlockedEvents & UIEventBlockingMask.NewChar) != 0;
		}

		protected virtual void OnNewChar(ref InputEvent keyboardEvent)
		{
			this.NewChar?.Invoke(Control, (char)keyboardEvent.Key);
		}

		protected virtual void DoUpdateReactivity(ref UIReactivityState reactivityState)
		{
			if (!Control.IsInteractive)
			{
				reactivityState.Add(UIReactivityState.Key.Disabled);
			}
			else if (hovered)
			{
				reactivityState.Add(UIReactivityState.Key.Hover);
			}
		}
	}
}
