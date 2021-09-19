using System;

namespace Amplitude.UI.Animations
{
	public interface IUIAnimationController
	{
		IUIAnimationItemsReadOnlyCollection Items { get; }

		float DurationFactor { get; set; }

		event Action<bool> ActiveChanged;

		float ComputeFullDuration(bool forward = true);

		void ComputeAutoReverse();

		bool IsAnyItemInProgress();

		bool IsAnyItemPaused();

		ItemType FindItem<ItemType>(UIComponent target = null) where ItemType : class, IUIAnimationItem;

		IUIAnimationItem FindItem(Type type, UIComponent target = null);

		void StartAnimations(bool forward = true, bool autoTriggerOnly = false);

		void StopAnimations();

		void PauseAnimations(bool pause);

		void ResetAnimations(bool toStart = true, bool applyValue = true);

		void ReverseAnimations(bool allReversed);
	}
}
