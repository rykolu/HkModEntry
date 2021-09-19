using System;
using Amplitude.Framework.Input;
using Amplitude.UI.Audio;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public class UIToggleResponder : UIControlResponder
	{
		private bool ignoreNextMouseUp;

		private float timeOfPreviousLeftClick = float.MinValue;

		private bool leftMousePressed;

		public IUIToggle Toggle => base.Interactable as IUIToggle;

		public event Action<IUIToggle, bool> Switch;

		public event Action<IUIToggle, bool> DoubleLeftClick;

		public UIToggleResponder(IUIToggle toggle)
			: base(toggle)
		{
		}

		public override void ResetState()
		{
			base.ResetState();
			leftMousePressed = false;
			ignoreNextMouseUp = false;
		}

		protected override void OnMouseDown(ref InputEvent mouseEvent, bool isMouseInside)
		{
			base.OnMouseDown(ref mouseEvent, isMouseInside);
			if (mouseEvent.Button != 0 || !base.IsInteractive)
			{
				return;
			}
			leftMousePressed = true;
			if (!Toggle.HandleDoubleClicks || !TryHandleDoubleClick(ref mouseEvent))
			{
				if (Toggle.WindowsExplorerLike)
				{
					if (!Toggle.State)
					{
						ignoreNextMouseUp = true;
						TrySwitchState();
					}
				}
				else if (!Toggle.HandleOnMouseUp)
				{
					TrySwitchState();
				}
			}
			UpdateReactivity();
		}

		protected override void OnMouseUp(ref InputEvent mouseEvent, bool isMouseInside)
		{
			base.OnMouseUp(ref mouseEvent, isMouseInside);
			if (mouseEvent.Button != 0)
			{
				return;
			}
			if (!mouseEvent.Catched && leftMousePressed && !ignoreNextMouseUp && isMouseInside)
			{
				if (Toggle.WindowsExplorerLike && Toggle.State)
				{
					TrySwitchState();
				}
				else if (Toggle.HandleOnMouseUp)
				{
					TrySwitchState();
				}
			}
			ignoreNextMouseUp = false;
			leftMousePressed = false;
			UpdateReactivity();
		}

		protected override void DoUpdateReactivity(ref UIReactivityState reactivityState)
		{
			if (!base.IsInteractive)
			{
				reactivityState.Add(UIReactivityState.Key.Disabled);
			}
			if (Toggle.State)
			{
				reactivityState.Add(UIReactivityState.Key.On);
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

		protected void TrySwitchState()
		{
			if (!Toggle.State || !Toggle.ClickDoesntSwitchOff)
			{
				Toggle.State = !Toggle.State;
				OnStateSwitch();
			}
		}

		protected void OnStateSwitch()
		{
			this.Switch?.Invoke(Toggle, Toggle.State);
			Toggle.TryTriggerAudioEvent(Toggle.State ? AudioEvent.Interactivity.SwitchOn : AudioEvent.Interactivity.SwitchOff);
		}

		private bool TryHandleDoubleClick(ref InputEvent mouseEvent)
		{
			if (!base.IsInteractive)
			{
				return false;
			}
			if (mouseEvent.Button == MouseButton.Left && IsDoubleClickSince(ref timeOfPreviousLeftClick))
			{
				ignoreNextMouseUp = true;
				this.DoubleLeftClick?.Invoke(Toggle, Toggle.State);
				Toggle.TryTriggerAudioEvent(AudioEvent.Interactivity.DoubleLeftClick);
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
