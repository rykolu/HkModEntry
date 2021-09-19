using System;
using Amplitude.UI.Audio;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UITransform))]
	public class UIToggle : UIControl, IUIToggle, IUIControl, IUIInteractable, IAudioControllerOwner
	{
		[SerializeField]
		[Tooltip("The state (checked or not) of the toggle.")]
		private bool state;

		[SerializeField]
		[Tooltip("Check to trigger DoubleLeftClick event. Each DoubleLeftClick follows a Switch event on the next LeftButtonDown event. The following LeftButtonUp will be ignored in all cases.")]
		private bool handleDoubleClicks;

		[SerializeField]
		[Tooltip("Check to use the toggle as a radio button.\nIt can be switched off programmatically, but not by clicking on it.")]
		private bool clickDoesntSwitchOff;

		[SerializeField]
		[Tooltip("Check to have the switch event triggered when the mouse is up.")]
		private bool handleOnMouseUp;

		[SerializeField]
		[Tooltip("Check to have the click events triggered on mouse up or down depending on its state.\nIt's like when clicking on files with Ctrl pressed in the Windows Explorer.")]
		private bool windowsExplorerLike;

		[SerializeField]
		[HideInInspector]
		private AudioController audioController;

		public virtual bool State
		{
			get
			{
				return state;
			}
			set
			{
				if (state != value)
				{
					state = value;
					ToggleResponder.UpdateReactivity();
				}
			}
		}

		public bool ClickDoesntSwitchOff
		{
			get
			{
				return clickDoesntSwitchOff;
			}
			set
			{
				clickDoesntSwitchOff = value;
			}
		}

		public bool HandleOnMouseUp
		{
			get
			{
				return handleOnMouseUp;
			}
			set
			{
				handleOnMouseUp = value;
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

		public bool WindowsExplorerLike
		{
			get
			{
				return windowsExplorerLike;
			}
			set
			{
				windowsExplorerLike = value;
			}
		}

		public AudioController AudioController => audioController;

		private UIToggleResponder ToggleResponder => (UIToggleResponder)base.Responder;

		public event Action<IUIToggle, bool> Switch
		{
			add
			{
				ToggleResponder.Switch += value;
			}
			remove
			{
				ToggleResponder.Switch -= value;
			}
		}

		public event Action<IUIToggle, bool> DoubleLeftClick
		{
			add
			{
				ToggleResponder.DoubleLeftClick += value;
			}
			remove
			{
				ToggleResponder.DoubleLeftClick -= value;
			}
		}

		public void SetAudioProfile(AudioProfile audioProfile)
		{
			audioController.Profile = audioProfile;
		}

		void IAudioControllerOwner.InitializeAudioProfile(AudioProfile audioProfile)
		{
			audioProfile.Initialize(AudioEvent.Interactivity.MouseEnter, AudioEvent.Interactivity.MouseLeave, AudioEvent.Interactivity.SwitchOn, AudioEvent.Interactivity.SwitchOff);
		}

		protected override IUIResponder InstantiateResponder()
		{
			return new UIToggleResponder(this);
		}

		protected override void Load()
		{
			base.Load();
		}

		private void OnHandleOnMouseUpChanged()
		{
			if (handleOnMouseUp)
			{
				windowsExplorerLike = false;
			}
		}

		private void OnWindowsExplorerLikeChanged()
		{
			if (windowsExplorerLike)
			{
				handleOnMouseUp = false;
			}
		}
	}
}
