using Amplitude.UI.Interactables;
using Amplitude.UI.Styles.Data;
using UnityEngine;

namespace Amplitude.UI.Styles.Scene
{
	internal abstract class UIRuntimeStyleItem
	{
		public struct LocalState
		{
			public readonly UIStyleItem StyleItem;

			public readonly int ItemsSetIndex;

			public readonly int RuntimeItemIndex;

			public int PreviousReactivityIndex;

			public int CurrentReactivityIndex;

			public float AnimationTimer;

			internal UIGradient GradientCache;

			public bool IsAnimationInProgress => AnimationTimer > float.Epsilon;

			public LocalState(UIStyleItem styleItem, int itemsSetIndex, int runtimeItemIndex)
			{
				StyleItem = styleItem;
				ItemsSetIndex = itemsSetIndex;
				RuntimeItemIndex = runtimeItemIndex;
				PreviousReactivityIndex = -1;
				CurrentReactivityIndex = -1;
				AnimationTimer = -1f;
				GradientCache = null;
				if (styleItem.ReactivityKeysCount > 0 && styleItem.Duration > float.Epsilon && styleItem is UIStyleItemImpl<UIGradient>)
				{
					GradientCache = new UIGradient();
				}
			}

			public float ComputeAnimationRatio()
			{
				if (StyleItem.Duration < float.Epsilon)
				{
					return 1f;
				}
				float num = Mathf.Clamp(AnimationTimer, 0f, StyleItem.Duration);
				float time = (StyleItem.Duration - num) / StyleItem.Duration;
				return StyleItem.TransitionCurve.Evaluate(time);
			}
		}

		internal readonly StaticString Identifier = StaticString.Empty;

		protected abstract bool CanInterpolate { get; }

		internal UIRuntimeStyleItem(StaticString identifier)
		{
			Identifier = identifier;
		}

		internal abstract bool IsDataValid(UIStyleItem item);

		internal void ApplyReactivityState(IUIStyleTarget rawTarget, ref LocalState localData, ref UIReactivityState reactivityState, bool instant)
		{
			int num = FindNextReactivityStateIndex(localData.StyleItem, ref reactivityState);
			if (instant || num != localData.CurrentReactivityIndex)
			{
				Apply(rawTarget, ref localData, num, instant);
			}
		}

		internal void UpdateAnimation(IUIStyleTarget target, ref LocalState localState, float deltaTime)
		{
			localState.AnimationTimer = Mathf.Max(localState.AnimationTimer - deltaTime, 0f);
			SetValue(target, ref localState);
		}

		protected abstract void SetValue(IUIStyleTarget target, ref LocalState localState);

		private void Apply(IUIStyleTarget rawTarget, ref LocalState localState, int nextReactivityKeyIndex, bool instant)
		{
			int previousReactivityIndex = localState.PreviousReactivityIndex;
			localState.PreviousReactivityIndex = localState.CurrentReactivityIndex;
			localState.CurrentReactivityIndex = nextReactivityKeyIndex;
			if (!instant && CanInterpolate)
			{
				bool flag = localState.IsAnimationInProgress && nextReactivityKeyIndex == previousReactivityIndex;
				localState.AnimationTimer = (flag ? (localState.StyleItem.Duration - localState.AnimationTimer) : localState.StyleItem.Duration);
			}
			else
			{
				localState.AnimationTimer = 0f;
			}
			SetValue(rawTarget, ref localState);
		}

		private int FindNextReactivityStateIndex(UIStyleItem dataItem, ref UIReactivityState reactivityState)
		{
			if (reactivityState.IsNormal)
			{
				return -1;
			}
			int reactivityKeysCount = dataItem.ReactivityKeysCount;
			if (reactivityKeysCount == 0)
			{
				return -1;
			}
			UIStyleReactivityKey[] reactivityKeys = dataItem.ReactivityKeys;
			int result = -1;
			int num = 0;
			int tagsCount = reactivityState.TagsCount;
			for (int i = 0; i < reactivityKeysCount; i++)
			{
				int score = 0;
				if (reactivityState.TryKey(ref reactivityKeys[i], out score))
				{
					if (score >= tagsCount)
					{
						result = i;
						break;
					}
					if (num < score)
					{
						num = score;
						result = i;
					}
				}
			}
			return result;
		}
	}
}
