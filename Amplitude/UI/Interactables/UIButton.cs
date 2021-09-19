using System;
using Amplitude.UI.Audio;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UITransform))]
	public class UIButton : UIControl, IUIButton, IUIControl, IUIInteractable, IAudioControllerOwner
	{
		[SerializeField]
		[Tooltip("To have the click events triggered when the mouse is down.")]
		private bool handleOnMouseDown;

		[SerializeField]
		[Tooltip("To handle double-clicks as well.")]
		private bool handleDoubleClicks;

		[SerializeField]
		[HideInInspector]
		private AudioController audioController;

		public bool HandleOnMouseDown
		{
			get
			{
				return handleOnMouseDown;
			}
			set
			{
				handleOnMouseDown = value;
			}
		}

		public bool HandleDoubleClicks
		{
			get
			{
				return handleDoubleClicks;
			}
			set
			{
				handleDoubleClicks = value;
			}
		}

		public AudioController AudioController => audioController;

		private UIButtonResponder ButtonResponder => (UIButtonResponder)base.Responder;

		public event Action<IUIButton> LeftClick
		{
			add
			{
				ButtonResponder.LeftClick += value;
			}
			remove
			{
				ButtonResponder.LeftClick -= value;
			}
		}

		public event Action<IUIButton> MiddleClick
		{
			add
			{
				ButtonResponder.MiddleClick += value;
			}
			remove
			{
				ButtonResponder.MiddleClick -= value;
			}
		}

		public event Action<IUIButton> RightClick
		{
			add
			{
				ButtonResponder.RightClick += value;
			}
			remove
			{
				ButtonResponder.RightClick -= value;
			}
		}

		public event Action<IUIButton> DoubleLeftClick
		{
			add
			{
				ButtonResponder.DoubleLeftClick += value;
			}
			remove
			{
				ButtonResponder.DoubleLeftClick -= value;
			}
		}

		public event Action<IUIButton> DoubleMiddleClick
		{
			add
			{
				ButtonResponder.DoubleMiddleClick += value;
			}
			remove
			{
				ButtonResponder.DoubleMiddleClick -= value;
			}
		}

		public event Action<IUIButton> DoubleRightClick
		{
			add
			{
				ButtonResponder.DoubleRightClick += value;
			}
			remove
			{
				ButtonResponder.DoubleRightClick -= value;
			}
		}

		void IAudioControllerOwner.InitializeAudioProfile(AudioProfile audioProfile)
		{
			audioProfile.Initialize(AudioEvent.Interactivity.MouseEnter, AudioEvent.Interactivity.MouseLeave, AudioEvent.Interactivity.LeftClick, AudioEvent.Interactivity.DoubleLeftClick, AudioEvent.Interactivity.MiddleClick, AudioEvent.Interactivity.DoubleMiddleClick, AudioEvent.Interactivity.RightClick, AudioEvent.Interactivity.DoubleRightClick);
		}

		public void SetAudioProfile(AudioProfile audioProfile)
		{
			audioController.Profile = audioProfile;
		}

		protected override IUIResponder InstantiateResponder()
		{
			return new UIButtonResponder(this);
		}
	}
}
