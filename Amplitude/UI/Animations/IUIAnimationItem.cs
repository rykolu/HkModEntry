using UnityEngine;

namespace Amplitude.UI.Animations
{
	public interface IUIAnimationItem
	{
		bool IsInitialized { get; }

		bool OncePerController { get; }

		IUIAnimationInterpolator Interpolator { get; }

		AnimationCurve Curve { get; set; }

		float Duration { get; set; }

		bool IsPaused { get; set; }

		bool Repeat { get; set; }

		float Delay { get; set; }

		float ReverseDelay { get; set; }

		bool AutoTrigger { get; set; }

		UIAnimationEventsSet Events { get; }

		bool InProgress { get; }

		bool IsReversed { get; }

		bool IsValidTarget(UIComponent potentialTarget);

		void Initialize(IUIAnimationItemClient client, object target, bool setValuesToDefault = false);

		T GetMin<T>();

		void SetMin<T>(T value);

		T GetMax<T>();

		void SetMax<T>(T value);

		void StartAnimation(bool forward = true, bool ignoreDelays = false);

		void ResetAnimation(bool toStart = true, bool applyValue = false, bool ignoreDelays = false);

		void StopAnimation();

		bool UpdateAnimation();

		void ReverseAnimation();

		string GetTargetName();

		string GetShortName();
	}
	public interface IUIAnimationItem<TValue>
	{
		TValue Min { get; set; }

		TValue Max { get; set; }

		float Duration { get; set; }

		float StartTime { get; set; }

		AnimationCurve Curve { get; set; }
	}
}
