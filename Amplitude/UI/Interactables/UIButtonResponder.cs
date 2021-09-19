using System;
using Amplitude.Framework.Input;
using Amplitude.UI.Audio;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public class UIButtonResponder : UIControlResponder
	{
		private bool leftMousePressed;

		private bool middleMousePressed;

		private bool rightMousePressed;

		private float timeOfPreviousLeftClick = float.MinValue;

		private bool ignoreNextLeftUp;

		private float timeOfPreviousMiddleClick = float.MinValue;

		private bool ignoreNextMiddleUp;

		private float timeOfPreviousRightClick = float.MinValue;

		private bool ignoreNextRightUp;

		private IUIButton Button => base.Interactable as IUIButton;

		public event Action<IUIButton> LeftClick;

		public event Action<IUIButton> MiddleClick;

		public event Action<IUIButton> RightClick;

		public event Action<IUIButton> DoubleLeftClick;

		public event Action<IUIButton> DoubleMiddleClick;

		public event Action<IUIButton> DoubleRightClick;

		public UIButtonResponder(IUIButton button)
			: base(button)
		{
		}

		public override void ResetState()
		{
			base.ResetState();
			leftMousePressed = false;
			middleMousePressed = false;
			rightMousePressed = false;
			timeOfPreviousLeftClick = float.MinValue;
			ignoreNextLeftUp = false;
			timeOfPreviousMiddleClick = float.MinValue;
			ignoreNextMiddleUp = false;
			timeOfPreviousRightClick = float.MinValue;
			ignoreNextRightUp = false;
		}

		protected override void OnMouseDown(ref InputEvent mouseEvent, bool isMouseInside)
		{
			base.OnMouseDown(ref mouseEvent, isMouseInside);
			leftMousePressed |= mouseEvent.Button == MouseButton.Left;
			middleMousePressed |= mouseEvent.Button == MouseButton.Middle;
			rightMousePressed |= mouseEvent.Button == MouseButton.Right;
			if (Button.HandleDoubleClicks)
			{
				TryHandleDoubleClick(ref mouseEvent);
			}
			if (Button.HandleOnMouseDown)
			{
				if (mouseEvent.Button == MouseButton.Left)
				{
					OnLeftClick();
				}
				else if (mouseEvent.Button == MouseButton.Middle)
				{
					OnMiddleClick();
				}
				else if (mouseEvent.Button == MouseButton.Right)
				{
					OnRightClick();
				}
			}
			UpdateReactivity();
		}

		protected override void OnMouseUp(ref InputEvent mouseEvent, bool isMouseInside)
		{
			base.OnMouseUp(ref mouseEvent, isMouseInside);
			if (isMouseInside && !mouseEvent.Catched && !Button.HandleOnMouseDown)
			{
				if (mouseEvent.Button == MouseButton.Left)
				{
					if (leftMousePressed && !ignoreNextLeftUp)
					{
						OnLeftClick();
					}
					ignoreNextLeftUp = false;
				}
				else if (mouseEvent.Button == MouseButton.Middle)
				{
					if (middleMousePressed && !ignoreNextMiddleUp)
					{
						OnMiddleClick();
					}
					ignoreNextMiddleUp = false;
				}
				else if (mouseEvent.Button == MouseButton.Right)
				{
					if (rightMousePressed && !ignoreNextRightUp)
					{
						OnRightClick();
					}
					ignoreNextRightUp = false;
				}
			}
			leftMousePressed &= mouseEvent.Button != MouseButton.Left;
			middleMousePressed &= mouseEvent.Button != MouseButton.Middle;
			rightMousePressed &= mouseEvent.Button != MouseButton.Right;
			UpdateReactivity();
		}

		protected virtual void OnLeftClick()
		{
			if (base.IsInteractive)
			{
				this.LeftClick?.Invoke(Button);
				Button.TryTriggerAudioEvent(AudioEvent.Interactivity.LeftClick);
			}
		}

		protected virtual void OnMiddleClick()
		{
			if (base.IsInteractive)
			{
				this.MiddleClick?.Invoke(Button);
				Button.TryTriggerAudioEvent(AudioEvent.Interactivity.MiddleClick);
			}
		}

		protected virtual void OnRightClick()
		{
			if (base.IsInteractive)
			{
				this.RightClick?.Invoke(Button);
				Button.TryTriggerAudioEvent(AudioEvent.Interactivity.RightClick);
			}
		}

		protected override void DoUpdateReactivity(ref UIReactivityState reactivityState)
		{
			if (!base.IsInteractive)
			{
				reactivityState.Add(UIReactivityState.Key.Disabled);
				return;
			}
			if (hovered)
			{
				reactivityState.Add(UIReactivityState.Key.Hover);
			}
			if (leftMousePressed)
			{
				reactivityState.Add(UIReactivityState.Key.Pressed);
			}
		}

		private bool TryHandleDoubleClick(ref InputEvent mouseEvent)
		{
			if (!base.IsInteractive)
			{
				return false;
			}
			if (mouseEvent.Button == MouseButton.Left && IsDoubleClickSince(ref timeOfPreviousLeftClick))
			{
				ignoreNextLeftUp = true;
				this.DoubleLeftClick?.Invoke(Button);
				Button.TryTriggerAudioEvent(AudioEvent.Interactivity.DoubleLeftClick);
				return true;
			}
			if (mouseEvent.Button == MouseButton.Middle && IsDoubleClickSince(ref timeOfPreviousMiddleClick))
			{
				ignoreNextMiddleUp = true;
				this.DoubleMiddleClick?.Invoke(Button);
				Button.TryTriggerAudioEvent(AudioEvent.Interactivity.DoubleMiddleClick);
				return true;
			}
			if (mouseEvent.Button == MouseButton.Right && IsDoubleClickSince(ref timeOfPreviousRightClick))
			{
				ignoreNextRightUp = true;
				this.DoubleRightClick?.Invoke(Button);
				Button.TryTriggerAudioEvent(AudioEvent.Interactivity.DoubleMiddleClick);
				return true;
			}
			return false;
		}

		private bool IsDoubleClickSince(ref float timeSinceLastClick)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (realtimeSinceStartup - timeSinceLastClick < 0.3f)
			{
				timeSinceLastClick = float.MinValue;
				return true;
			}
			timeSinceLastClick = realtimeSinceStartup;
			return false;
		}
	}
}
