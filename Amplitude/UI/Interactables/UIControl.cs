using System;
using Amplitude.UI.Audio;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UITransform))]
	public class UIControl : UIInteractable, IUIControl, IUIInteractable
	{
		[SerializeField]
		[HideInInspector]
		private UIEventBlockingMask blockedEvents = UIEventBlockingMask.All;

		[SerializeField]
		[HideInInspector]
		private bool focusable;

		public virtual UIEventBlockingMask BlockedEvents
		{
			get
			{
				return blockedEvents;
			}
			set
			{
				blockedEvents = value;
			}
		}

		public bool Focusable
		{
			get
			{
				return focusable;
			}
			set
			{
				focusable = value;
			}
		}

		public bool Focused => ControlResponder.Focused;

		public ref UIReactivityState ReactivityState => ref ControlResponder.ReactivityState;

		private UIControlResponder ControlResponder => (UIControlResponder)base.Responder;

		public event Action<IUIControl, Vector2> Tick
		{
			add
			{
				ControlResponder.Tick += value;
			}
			remove
			{
				ControlResponder.Tick -= value;
			}
		}

		public event Action<IUIControl, Vector2> MouseEnter
		{
			add
			{
				ControlResponder.MouseEnter += value;
			}
			remove
			{
				ControlResponder.MouseEnter -= value;
			}
		}

		public event Action<IUIControl, Vector2> MouseLeave
		{
			add
			{
				ControlResponder.MouseLeave += value;
			}
			remove
			{
				ControlResponder.MouseLeave -= value;
			}
		}

		public event Action<IUIControl, Vector2> LeftMouseDown
		{
			add
			{
				ControlResponder.LeftMouseDown += value;
			}
			remove
			{
				ControlResponder.LeftMouseDown -= value;
			}
		}

		public event Action<IUIControl, Vector2> LeftMouseUp
		{
			add
			{
				ControlResponder.LeftMouseUp += value;
			}
			remove
			{
				ControlResponder.LeftMouseUp -= value;
			}
		}

		public event Action<IUIControl, Vector2> MiddleMouseDown
		{
			add
			{
				ControlResponder.MiddleMouseDown += value;
			}
			remove
			{
				ControlResponder.MiddleMouseDown -= value;
			}
		}

		public event Action<IUIControl, Vector2> MiddleMouseUp
		{
			add
			{
				ControlResponder.MiddleMouseUp += value;
			}
			remove
			{
				ControlResponder.MiddleMouseUp -= value;
			}
		}

		public event Action<IUIControl, Vector2> RightMouseDown
		{
			add
			{
				ControlResponder.RightMouseDown += value;
			}
			remove
			{
				ControlResponder.RightMouseDown -= value;
			}
		}

		public event Action<IUIControl, Vector2> RightMouseUp
		{
			add
			{
				ControlResponder.RightMouseUp += value;
			}
			remove
			{
				ControlResponder.RightMouseUp -= value;
			}
		}

		public event Action<IUIControl> FocusGain
		{
			add
			{
				ControlResponder.FocusGain += value;
			}
			remove
			{
				ControlResponder.FocusGain -= value;
			}
		}

		public event Action<IUIControl> FocusLoss
		{
			add
			{
				ControlResponder.FocusLoss += value;
			}
			remove
			{
				ControlResponder.FocusLoss -= value;
			}
		}

		public event Action<IUIControl, float> MouseScroll
		{
			add
			{
				ControlResponder.MouseScroll += value;
			}
			remove
			{
				ControlResponder.MouseScroll -= value;
			}
		}

		public event Action<IUIControl, Vector2> AxisUpdate2D
		{
			add
			{
				ControlResponder.AxisUpdate2D += value;
			}
			remove
			{
				ControlResponder.AxisUpdate2D -= value;
			}
		}

		public event Action<IUIControl, KeyCode> KeyDown
		{
			add
			{
				ControlResponder.KeyDown += value;
			}
			remove
			{
				ControlResponder.KeyDown -= value;
			}
		}

		public event Action<IUIControl, KeyCode> KeyUp
		{
			add
			{
				ControlResponder.KeyUp += value;
			}
			remove
			{
				ControlResponder.KeyUp -= value;
			}
		}

		public void Focus(bool focus)
		{
			if (focus)
			{
				if (focusable && UIInteractivityManager.Instance.FocusedResponder != ControlResponder)
				{
					UIInteractivityManager.Instance.SetFocus(ControlResponder);
				}
			}
			else if (UIInteractivityManager.Instance.FocusedResponder == ControlResponder)
			{
				UIInteractivityManager.Instance.SetFocus();
			}
		}

		public void TryTriggerAudioEvent(StaticString interactivityEvent)
		{
			(this as IAudioControllerOwner)?.AudioController.TryTriggerEvent(interactivityEvent);
		}

		public void ResetState()
		{
			ControlResponder.ResetState();
			if (base.IsVisible)
			{
				ControlResponder.UpdateReactivity();
			}
		}

		protected override IUIResponder InstantiateResponder()
		{
			return new UIControlResponder(this);
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (base.Loaded)
			{
				ControlResponder.UpdateReactivity(instant: true);
			}
		}

		protected override void OnIsVisibleChanged()
		{
			base.OnIsVisibleChanged();
			if (base.IsVisible)
			{
				ControlResponder.UpdateReactivity(instant: true);
			}
			else
			{
				ControlResponder.ResetState();
			}
		}

		protected override void OnIsInteractiveChanged()
		{
			base.OnIsInteractiveChanged();
			if (!base.IsInteractive)
			{
				ControlResponder.ResetState();
			}
			if (base.IsVisible)
			{
				ControlResponder.UpdateReactivity();
			}
		}
	}
}
