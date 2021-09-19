using System;
using Amplitude.UI.Animations;
using Amplitude.UI.Animations.Data;
using Amplitude.UI.Animations.Scene;
using Amplitude.UI.Audio;
using UnityEngine;

namespace Amplitude.UI
{
	public abstract class UIAbstractShowable : UIIndexedComponent, IUIAnimationControllerPragmaticOwner, IUIAnimationControllerOwner, IAudioControllerOwner
	{
		public enum VisibilityState
		{
			None,
			PreShowing,
			Showing,
			Visible,
			Hiding,
			Invisible
		}

		[SerializeField]
		[HideInInspector]
		protected UIAnimationMultiController animationController;

		[SerializeField]
		[HideInInspector]
		protected bool hasAnimationController;

		[SerializeField]
		[HideInInspector]
		private UIAnimatorComponent showAnimator;

		[SerializeField]
		[HideInInspector]
		private UIAnimatorComponent hideAnimator;

		[NonSerialized]
		private VisibilityState visibility;

		[NonSerialized]
		protected bool lastInstant;

		[SerializeField]
		[HideInInspector]
		private AudioController audioController;

		[NonSerialized]
		private bool isChangingTransformVisibleSelf;

		public VisibilityState Visibility
		{
			get
			{
				return visibility;
			}
			protected set
			{
				if (visibility != value)
				{
					VisibilityState oldState = visibility;
					visibility = value;
					OnVisibilityChanged(oldState, visibility);
				}
			}
		}

		public IUIAnimationController AnimationController => animationController;

		public bool HasAnimationController => hasAnimationController;

		public UIAnimatorComponent ShowAnimator => showAnimator;

		public UIAnimatorComponent HideAnimator => hideAnimator;

		public bool Shown
		{
			get
			{
				if (Visibility != VisibilityState.Visible && Visibility != VisibilityState.Showing)
				{
					return Visibility == VisibilityState.PreShowing;
				}
				return true;
			}
		}

		public bool Hidden
		{
			get
			{
				if (Visibility != VisibilityState.Invisible)
				{
					return Visibility == VisibilityState.Hiding;
				}
				return true;
			}
		}

		public bool HasFadeAnimation
		{
			get
			{
				if (showAnimator != null)
				{
					return true;
				}
				return ((IUIAnimationControllerPragmaticOwner)this).HasAnimationController;
			}
		}

		public AudioController AudioController => audioController;

		protected virtual bool InitialVisibility => false;

		public event Action<UIAbstractShowable, VisibilityState, VisibilityState> VisibilityChange;

		void IUIAnimationControllerPragmaticOwner.ClearAnimationController(bool onlyIfNecessary)
		{
			if (!onlyIfNecessary || !hasAnimationController)
			{
				if (animationController != null)
				{
					animationController.ActiveChanged -= OnAnimationActiveChanged;
					animationController.StopAnimations();
				}
				animationController = null;
				hasAnimationController = false;
			}
		}

		void IUIAnimationControllerPragmaticOwner.CreateAnimationController()
		{
			animationController = new UIAnimationMultiController();
			animationController.ActiveChanged += OnAnimationActiveChanged;
			hasAnimationController = true;
		}

		protected virtual void OnVisibilityChanged(VisibilityState oldState, VisibilityState newState)
		{
			this.VisibilityChange?.Invoke(this, oldState, newState);
		}

		protected void StartAnimation(bool show)
		{
			if (HasFadeAnimation)
			{
				if (showAnimator != null && hideAnimator != null)
				{
					if (show)
					{
						if (hideAnimator.InProgress)
						{
							hideAnimator.IsReversed = true;
							return;
						}
						if (showAnimator.InProgress)
						{
							showAnimator.IsReversed = false;
							return;
						}
						showAnimator.IsReversed = false;
						showAnimator.ResetToBeginning(apply: true);
						showAnimator.Play();
					}
					else if (showAnimator.InProgress)
					{
						showAnimator.IsReversed = true;
					}
					else if (hideAnimator.InProgress)
					{
						hideAnimator.IsReversed = false;
					}
					else
					{
						hideAnimator.IsReversed = false;
						hideAnimator.ResetToBeginning(apply: true);
						hideAnimator.Play();
					}
				}
				else if (showAnimator != null)
				{
					showAnimator.IsReversed = !show;
					if (!showAnimator.InProgress)
					{
						showAnimator.Play();
					}
				}
				else if (animationController != null)
				{
					animationController.ReverseAnimations(!show);
				}
			}
			else
			{
				OnAnimationActiveChanged(active: false);
			}
		}

		protected void OnAnimationActiveChanged(bool active)
		{
			if (!active)
			{
				switch (visibility)
				{
				case VisibilityState.Showing:
					OnShowProcessFinished(instant: false);
					break;
				case VisibilityState.Hiding:
					OnHideProcessFinished(instant: false);
					break;
				}
			}
		}

		protected void OnShowProcessFinished(bool instant)
		{
			OnEndShow(instant);
			Visibility = VisibilityState.Visible;
			audioController.TryTriggerEvent(instant ? AudioEvent.Panels.InstantShow : AudioEvent.Panels.EndShow);
		}

		protected void OnHideProcessFinished(bool instant)
		{
			SetTransformVisibleSelf(value: false);
			OnEndHide(instant);
			if (Visibility == VisibilityState.Hiding)
			{
				audioController.TryTriggerEvent(instant ? AudioEvent.Panels.InstantHide : AudioEvent.Panels.EndHide);
				Visibility = VisibilityState.Invisible;
			}
		}

		private void ShowAnimator_InProgressChange(UIAnimatorComponent animationComponent, bool value)
		{
			if (value)
			{
				return;
			}
			bool isReversed = animationComponent.IsReversed;
			switch (visibility)
			{
			case VisibilityState.Showing:
				if (!isReversed && animationComponent.IsAtEnd())
				{
					OnShowProcessFinished(instant: false);
				}
				break;
			case VisibilityState.Hiding:
				if (isReversed && animationComponent.IsAtBeginning())
				{
					OnHideProcessFinished(instant: false);
				}
				break;
			}
		}

		private bool IsShowAnimatorProgressChangeExpected(UIAnimatorComponent animationComponent, bool inProgress)
		{
			switch (visibility)
			{
			case VisibilityState.Showing:
			case VisibilityState.Hiding:
				return true;
			case VisibilityState.PreShowing:
				if (!inProgress && animationComponent.IsReversed && animationComponent.IsAtBeginning())
				{
					return true;
				}
				Diagnostics.LogError($"Showable's animator progress changed to {inProgress} while Visibility = PreShowing - InProgress={inProgress}/IsReversed={animationComponent.IsReversed}/Ratio={animationComponent.ComputeCurrentRatio()} ({UITransform.ToString()})");
				return false;
			default:
				return false;
			}
		}

		private void HideAnimator_InProgressChange(UIAnimatorComponent animationComponent, bool value)
		{
			if (value)
			{
				return;
			}
			bool isReversed = animationComponent.IsReversed;
			switch (visibility)
			{
			case VisibilityState.Showing:
				if (isReversed && animationComponent.IsAtBeginning())
				{
					OnShowProcessFinished(instant: false);
				}
				break;
			case VisibilityState.Hiding:
				if (!isReversed && animationComponent.IsAtEnd())
				{
					OnHideProcessFinished(instant: false);
				}
				break;
			}
		}

		public void SetAudioProfile(AudioProfile audioProfile)
		{
			audioController.Profile = audioProfile;
		}

		void IAudioControllerOwner.InitializeAudioProfile(AudioProfile audioProfile)
		{
			audioProfile.Initialize(AudioEvent.Panels.BeginShow, AudioEvent.Panels.EndShow, AudioEvent.Panels.InstantShow, AudioEvent.Panels.BeginHide, AudioEvent.Panels.EndHide, AudioEvent.Panels.InstantHide);
		}

		protected override void Load()
		{
			base.Load();
			((IUIAnimationControllerPragmaticOwner)this).ClearAnimationController(onlyIfNecessary: true);
			if (showAnimator != null)
			{
				showAnimator.LoadIfNecessary();
				CheckAnimatorProperties(showAnimator);
				showAnimator.InProgressChange += ShowAnimator_InProgressChange;
				if (hideAnimator != null)
				{
					hideAnimator.LoadIfNecessary();
					CheckAnimatorProperties(hideAnimator);
					hideAnimator.InProgressChange += HideAnimator_InProgressChange;
				}
			}
			else if (animationController != null)
			{
				animationController.LoadTargetsIfNecessary();
				animationController.ActiveChanged += OnAnimationActiveChanged;
			}
			ApplyInitialVisibility();
		}

		protected override void Unload()
		{
			if (showAnimator != null)
			{
				showAnimator.InProgressChange -= ShowAnimator_InProgressChange;
			}
			if (hideAnimator != null)
			{
				hideAnimator.InProgressChange -= HideAnimator_InProgressChange;
			}
			if (animationController != null)
			{
				animationController.ActiveChanged -= OnAnimationActiveChanged;
			}
			Visibility = VisibilityState.None;
			base.Unload();
		}

		protected override void OnTransformVisibleGloballyChanged(bool previouslyVisible, bool currentlyVisible)
		{
			if (isChangingTransformVisibleSelf || !currentlyVisible)
			{
				return;
			}
			if (visibility == VisibilityState.Showing)
			{
				if (HasFadeAnimation)
				{
					ResetAnimation(show: true);
				}
				OnShowProcessFinished(instant: true);
			}
			else if (visibility == VisibilityState.Hiding)
			{
				if (HasFadeAnimation)
				{
					ResetAnimation(show: false);
				}
				OnHideProcessFinished(instant: true);
			}
		}

		protected virtual void OnPreShow()
		{
		}

		protected virtual void OnBeginShow(bool instant)
		{
		}

		protected virtual void OnEndShow(bool instant)
		{
		}

		protected virtual void OnBeginHide(bool instant)
		{
		}

		protected virtual void OnEndHide(bool instant)
		{
		}

		protected virtual bool IsReadyForShowing()
		{
			return true;
		}

		protected virtual void RequestShow(bool instant)
		{
			lastInstant = instant;
			Visibility = VisibilityState.PreShowing;
			OnPreShow();
			if (IsReadyForShowing())
			{
				InternalRequestShow();
			}
		}

		protected virtual void RequestHide(bool instant)
		{
			if (Shown)
			{
				Visibility = VisibilityState.Hiding;
				OnBeginHide(instant);
			}
			if (Shown)
			{
				return;
			}
			if (!instant && base.TransformVisibleGlobally)
			{
				StartAnimation(show: false);
				audioController.TryTriggerEvent(AudioEvent.Panels.BeginHide);
				return;
			}
			if (HasFadeAnimation)
			{
				ResetAnimation(show: false);
			}
			OnHideProcessFinished(instant: true);
		}

		protected virtual void InternalRequestShow()
		{
			Visibility = VisibilityState.Showing;
			SetTransformVisibleSelf(value: true);
			OnBeginShow(lastInstant);
			if (Visibility == VisibilityState.Hiding || Visibility == VisibilityState.Invisible)
			{
				return;
			}
			if (!lastInstant && base.TransformVisibleGlobally)
			{
				StartAnimation(show: true);
				audioController.TryTriggerEvent(AudioEvent.Panels.BeginShow);
				return;
			}
			if (HasFadeAnimation)
			{
				ResetAnimation(show: true);
			}
			OnShowProcessFinished(instant: true);
		}

		protected virtual void ApplyInitialVisibility()
		{
			bool flag = false;
			if (InitialVisibility || flag)
			{
				visibility = (UITransform.VisibleSelf ? VisibilityState.Visible : VisibilityState.Invisible);
			}
			else
			{
				SetTransformVisibleSelf(value: false);
				visibility = VisibilityState.Invisible;
			}
			if (HasFadeAnimation)
			{
				ResetAnimation(UITransform.VisibleSelf);
			}
		}

		private static void CheckAnimatorProperties(UIAnimatorComponent animatorComponent)
		{
			if (animatorComponent.AutoTrigger)
			{
				Diagnostics.LogWarning("Showable's Animator has AutoTrigger set to true. This is forbidden. Setting it to false. ('" + animatorComponent.UITransform.ToString() + "')");
				animatorComponent.AutoTrigger = false;
			}
			if (animatorComponent.LoopMode != 0)
			{
				Diagnostics.LogWarning($"Showable's Animator has LoopMode set to {animatorComponent.LoopMode}. This is forbidden. Setting it to None. ('{animatorComponent.UITransform.ToString()}')");
				animatorComponent.LoopMode = UIAnimationLoopMode.None;
			}
		}

		private void ResetAnimation(bool show)
		{
			if (showAnimator != null && hideAnimator != null)
			{
				UIAnimatorComponent uIAnimatorComponent = (show ? showAnimator : hideAnimator);
				UIAnimatorComponent uIAnimatorComponent2 = (show ? hideAnimator : showAnimator);
				if (uIAnimatorComponent2.InProgress)
				{
					uIAnimatorComponent2.Pause();
					uIAnimatorComponent2.ResetToBeginning(apply: true);
					return;
				}
				if (uIAnimatorComponent.InProgress)
				{
					uIAnimatorComponent.Pause();
				}
				uIAnimatorComponent.ResetToEnd(apply: true);
			}
			else if (showAnimator != null)
			{
				if (showAnimator.InProgress)
				{
					showAnimator.Pause();
				}
				showAnimator.ResetAtRatio(show ? 1f : 0f, apply: true);
			}
			else if (animationController != null)
			{
				animationController.ReverseAnimations(!show);
				animationController.ResetAnimations(toStart: false);
			}
		}

		private void SetTransformVisibleSelf(bool value)
		{
			isChangingTransformVisibleSelf = true;
			UITransform.VisibleSelf = value;
			isChangingTransformVisibleSelf = false;
		}
	}
}
