using System;
using System.Collections.Generic;
using Amplitude.UI.Animations.Data;
using UnityEngine;

namespace Amplitude.UI.Animations.Scene
{
	[ExecuteInEditMode]
	public class UIAnimatorComponent : UIIndexedComponent, IUIAnimationItemsCollection
	{
		private struct AnimationItemOrder
		{
			public int ItemIndex;

			public StaticString Name;

			public UIAnimationItem AnimationItem;
		}

		private struct PendingEventsData
		{
			public static readonly PendingEventsData Clear = new PendingEventsData
			{
				InProgressChanged = false,
				Tick = false,
				PreviousRatio = -1f,
				CurrentRatio = -1f
			};

			public bool InProgressChanged;

			public bool Tick;

			public float PreviousRatio;

			public float CurrentRatio;
		}

		[SerializeField]
		private UIAnimationAsset animationAsset;

		[SerializeField]
		private UIBehaviour[] animationTargets;

		private UIAnimationItem[] items;

		private List<AnimationItemOrder> workingAnimationItemOrders;

		private bool inProgress;

		private bool isReversed;

		private UIAnimationLoopMode loopMode;

		private bool autoTrigger;

		private float durationFactor = 1f;

		private float currentTime;

		private PendingEventsData pendingEvents;

		public bool InProgress => inProgress;

		public bool IsReversed
		{
			get
			{
				return isReversed;
			}
			set
			{
				isReversed = value;
			}
		}

		public UIAnimationLoopMode LoopMode
		{
			get
			{
				return loopMode;
			}
			set
			{
				loopMode = value;
			}
		}

		public bool AutoTrigger
		{
			get
			{
				return autoTrigger;
			}
			set
			{
				autoTrigger = value;
			}
		}

		public float DurationFactor
		{
			get
			{
				return durationFactor;
			}
			set
			{
				durationFactor = value;
			}
		}

		internal UIAnimationAsset AnimationAsset => animationAsset;

		public event Action<UIAnimatorComponent, float> Tick;

		public event Action<UIAnimatorComponent, bool> InProgressChange;

		public event Action<UIAnimatorComponent, StaticString> EventTrigger;

		public bool IsAtBeginning()
		{
			return currentTime <= float.Epsilon;
		}

		public bool IsAtEnd()
		{
			return currentTime >= ComputeDuration() - float.Epsilon;
		}

		public bool ContainsEvent(StaticString eventName)
		{
			int num = animationAsset?.Events?.Length ?? 0;
			for (int i = 0; i < num; i++)
			{
				if (animationAsset.Events[i].Name == eventName)
				{
					return true;
				}
			}
			return false;
		}

		public void Play()
		{
			if (!inProgress)
			{
				if (!UITransform.VisibleGlobally)
				{
					Diagnostics.LogWarning("AnimatorComponent cannot start while UITransform.VisibleGlobally = false (" + ToString() + ")");
					return;
				}
				UIAnimatorManager.Instance.StartAnimation(this);
				inProgress = true;
				this.InProgressChange?.Invoke(this, inProgress);
			}
		}

		public void Pause()
		{
			if (inProgress)
			{
				inProgress = false;
				this.InProgressChange?.Invoke(this, inProgress);
				UIAnimatorManager.Instance?.StopAnimation(this);
			}
		}

		public void ResetToBeginning(bool apply)
		{
			currentTime = 0f;
			if (apply)
			{
				ApplyCurrentTime();
			}
		}

		public void ResetToEnd(bool apply)
		{
			currentTime = ComputeDuration();
			if (apply)
			{
				ApplyCurrentTime();
			}
		}

		public void ResetAtRatio(float ratio, bool apply)
		{
			currentTime = ComputeDuration() * Mathf.Clamp01(ratio);
			if (apply)
			{
				ApplyCurrentTime();
			}
		}

		public void ResetAtTime(float time, bool apply)
		{
			currentTime = time;
			if (apply)
			{
				ApplyCurrentTime();
			}
		}

		public IUIAnimationItem<TValue> FindItem<TValue>(StaticString name, IUIAnimationTarget target = null)
		{
			int num = ((items != null) ? items.Length : 0);
			for (int i = 0; i < num; i++)
			{
				UIAnimationItem uIAnimationItem = items[i];
				if (uIAnimationItem == null || uIAnimationItem.Name != name)
				{
					continue;
				}
				IUIAnimationTarget iUIAnimationTarget = animationTargets[i] as IUIAnimationTarget;
				if (target == null || target == iUIAnimationTarget)
				{
					IUIAnimationItem<TValue> iUIAnimationItem = uIAnimationItem as IUIAnimationItem<TValue>;
					if (iUIAnimationItem != null)
					{
						return iUIAnimationItem;
					}
				}
			}
			return null;
		}

		public float ComputeDuration()
		{
			float num = 0f;
			int num2 = items.Length;
			for (int i = 0; i < num2; i++)
			{
				UIAnimationItem uIAnimationItem = items[i];
				if (uIAnimationItem != null)
				{
					num = Mathf.Max(num, uIAnimationItem.StartTime + uIAnimationItem.Duration);
				}
			}
			return num;
		}

		public float ComputeCurrentRatio()
		{
			float num = ComputeDuration();
			if (num > float.Epsilon)
			{
				return currentTime / num;
			}
			return 0f;
		}

		public void Add<TTarget, TValue>(StaticString name, Func<TTarget, TValue> getFunc, Action<TTarget, TValue> setAction) where TTarget : class, IUIAnimationTarget
		{
			int count = workingAnimationItemOrders.Count;
			for (int i = 0; i < count; i++)
			{
				AnimationItemOrder value = workingAnimationItemOrders[i];
				if (!(value.Name != name))
				{
					object value2 = null;
					if (!Interpolators.InstancePerValueType.TryGetValue(typeof(TValue), out value2))
					{
						Diagnostics.LogError($"No Interpolator available for Value type '{typeof(TValue).Name}': {name} could not be created.");
						break;
					}
					value.AnimationItem = new UIAnimationItemImpl<TTarget, TValue>(name, getFunc, setAction, (IInterpolator<TValue>)value2);
					workingAnimationItemOrders[i] = value;
					break;
				}
			}
		}

		public void Add<TTarget, TValue>(MutatorSet<TTarget, TValue> mutatorSet) where TTarget : class, IUIAnimationTarget
		{
			Add(mutatorSet.Name, mutatorSet.Get, mutatorSet.Set);
		}

		internal UIAnimationItem GetAnimationItemAt(int index)
		{
			if (index < 0 || items == null || index >= items.Length)
			{
				return null;
			}
			return items[index];
		}

		internal bool UpdateAnimation()
		{
			float num = ComputeDuration();
			if (num < float.Epsilon || durationFactor < float.Epsilon)
			{
				inProgress = false;
				pendingEvents.InProgressChanged = true;
				return false;
			}
			float previousRatio = currentTime / num;
			currentTime += ((!isReversed) ? 1f : (-1f)) * (1f / durationFactor) * Time.deltaTime;
			currentTime = Mathf.Clamp(currentTime, 0f, num);
			ApplyCurrentTime();
			bool flag = currentTime > 0f && currentTime < num;
			if (!flag)
			{
				switch (loopMode)
				{
				case UIAnimationLoopMode.BackAndForth:
					isReversed = !isReversed;
					flag = true;
					break;
				case UIAnimationLoopMode.Repeat:
					currentTime = ((!isReversed) ? 0f : num);
					flag = true;
					break;
				}
			}
			pendingEvents.PreviousRatio = previousRatio;
			pendingEvents.CurrentRatio = currentTime / num;
			pendingEvents.Tick = flag;
			pendingEvents.InProgressChanged = inProgress != flag;
			inProgress = flag;
			return inProgress;
		}

		internal void TriggerPendingEvents()
		{
			if (pendingEvents.PreviousRatio >= 0f && pendingEvents.CurrentRatio >= 0f)
			{
				TryTriggerEvent(pendingEvents.PreviousRatio, pendingEvents.CurrentRatio);
			}
			if (pendingEvents.Tick)
			{
				this.Tick?.Invoke(this, pendingEvents.CurrentRatio);
			}
			if (pendingEvents.InProgressChanged)
			{
				this.InProgressChange?.Invoke(this, inProgress);
			}
			pendingEvents = PendingEventsData.Clear;
		}

		protected override void Load()
		{
			base.Load();
			if (animationAsset == null)
			{
				Diagnostics.LogWarning(54uL, "UIAnimationComponent on '" + base.gameObject.name + "' has no Asset.");
				return;
			}
			animationAsset.Load();
			CreateItems();
			LoadAssetProperties();
			LoadItems();
			OnTransformVisibleGloballyChanged(previouslyVisible: false, UITransform.VisibleGlobally);
		}

		protected override void Unload()
		{
			if (inProgress)
			{
				UIAnimatorManager.Instance?.StopAnimation(this);
				inProgress = false;
			}
			items = null;
			base.Unload();
		}

		protected override void OnTransformVisibleGloballyChanged(bool previouslyVisible, bool currentlyVisible)
		{
			if (!base.Loaded)
			{
				return;
			}
			if (currentlyVisible)
			{
				if (autoTrigger)
				{
					ResetToBeginning(apply: true);
					Play();
				}
			}
			else if (inProgress)
			{
				Pause();
			}
		}

		private void CreateItems()
		{
			workingAnimationItemOrders = new List<AnimationItemOrder>();
			int num = ((animationAsset.Items != null) ? animationAsset.Items.Length : 0);
			int num2 = ((animationTargets != null) ? animationTargets.Length : 0);
			if (num != num2)
			{
				num = Mathf.Min(num, num2);
			}
			items = new UIAnimationItem[num];
			if (num == 0)
			{
				return;
			}
			for (int i = 0; i < num; i++)
			{
				if (items[i] != null)
				{
					continue;
				}
				if (i >= num2)
				{
					Diagnostics.LogWarning(54uL, "UIAnimationComponent on '" + UITransform.ToString() + "' does not specify enough Target for Asset '" + animationAsset.name + "'");
					return;
				}
				UIBehaviour uIBehaviour = animationTargets[i];
				if (uIBehaviour == null)
				{
					Diagnostics.LogError($"UIAnimationComponent on '{base.gameObject.name}' has no Target for Item '{animationAsset.Items[i].Name}': Expected to animate a '{i}' (Asset = '{animationAsset.name}')");
					continue;
				}
				IUIAnimationTarget iUIAnimationTarget = uIBehaviour as IUIAnimationTarget;
				if (iUIAnimationTarget == null)
				{
					Diagnostics.LogError($"UIAnimationComponent on '{base.gameObject.name}' Target for Item '{animationAsset.Items[i].Name}': '{uIBehaviour.GetType().Name}' is not a IUIAnimationTarget");
					continue;
				}
				uIBehaviour.LoadIfNecessary();
				for (int j = i; j < num; j++)
				{
					if (items[j] == null && !(uIBehaviour != animationTargets[j]))
					{
						workingAnimationItemOrders.Add(new AnimationItemOrder
						{
							ItemIndex = j,
							Name = animationAsset.Items[j].Name,
							AnimationItem = null
						});
					}
				}
				iUIAnimationTarget.CreateAnimationItems(this);
				int count = workingAnimationItemOrders.Count;
				for (int k = 0; k < count; k++)
				{
					AnimationItemOrder animationItemOrder = workingAnimationItemOrders[k];
					if (animationItemOrder.AnimationItem == null)
					{
						Diagnostics.LogError(54uL, $"In '{iUIAnimationTarget}', AnimationItem '{animationItemOrder.Name}' could not be created from Target '{iUIAnimationTarget.GetType().Name}'");
					}
					else
					{
						items[animationItemOrder.ItemIndex] = animationItemOrder.AnimationItem;
					}
				}
				workingAnimationItemOrders.Clear();
			}
			workingAnimationItemOrders = null;
		}

		private void LoadAssetProperties()
		{
			loopMode = animationAsset.LoopMode;
			autoTrigger = animationAsset.AutoTrigger;
			durationFactor = animationAsset.DurationFactor;
		}

		private void LoadItems()
		{
			int num = items.Length;
			for (int i = 0; i < num; i++)
			{
				if (items[i] != null)
				{
					items[i].LoadFromAsset(animationAsset.Items[i]);
				}
			}
		}

		private void ApplyCurrentTime()
		{
			int num = items.Length;
			for (int i = 0; i < num; i++)
			{
				UIAnimationItem uIAnimationItem = items[i];
				float startTime = uIAnimationItem.StartTime;
				float duration = uIAnimationItem.Duration;
				if (startTime + duration < currentTime)
				{
					uIAnimationItem.Update(animationTargets[i], 1f);
					continue;
				}
				if (startTime > currentTime)
				{
					uIAnimationItem.Update(animationTargets[i], 0f);
					continue;
				}
				float value = (currentTime - startTime) / duration;
				uIAnimationItem.Update(animationTargets[i], Mathf.Clamp01(value));
			}
		}

		private void TryTriggerEvent(float previousRatio, float currentRatio)
		{
			int num = ((animationAsset.Events != null) ? animationAsset.Events.Length : 0);
			for (int i = 0; i < num; i++)
			{
				Amplitude.UI.Animations.Data.UIAnimationEvent uIAnimationEvent = animationAsset.Events[i];
				if (uIAnimationEvent.TriggerDirection != 0 && ((uIAnimationEvent.TriggerDirection == Amplitude.UI.Animations.Data.UIAnimationEvent.Direction.ForwardOnly && isReversed) || (uIAnimationEvent.TriggerDirection == Amplitude.UI.Animations.Data.UIAnimationEvent.Direction.BackwardOnly && !isReversed)))
				{
					continue;
				}
				if (!isReversed)
				{
					if (uIAnimationEvent.Ratio < previousRatio || uIAnimationEvent.Ratio > currentRatio)
					{
						continue;
					}
				}
				else if (uIAnimationEvent.Ratio < currentRatio || uIAnimationEvent.Ratio > previousRatio)
				{
					continue;
				}
				this.EventTrigger?.Invoke(this, uIAnimationEvent.Name);
				animationAsset.AudioController.TryTriggerEvent(uIAnimationEvent.Name);
			}
		}

		internal void OnAnimationEvent(UIAnimationEditionEventArg arg)
		{
		}
	}
}
