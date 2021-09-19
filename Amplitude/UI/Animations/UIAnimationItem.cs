using System;
using Amplitude.UI.Animations.Data;
using Amplitude.UI.Traits;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	public abstract class UIAnimationItem<Type, InterpolatorType, TargetType> : IUIAnimationItem, IUIAnimationItemInspectable, IUIAnimationItemSerializable, IUIAnimationEventsSetClient where InterpolatorType : UIAnimationInterpolator<Type>, new()where TargetType : class, IUITrait<Type>
	{
		protected InterpolatorType interpolator = new InterpolatorType();

		protected TargetType target;

		protected UIAnimationItemParams parameters;

		private IUIAnimationItemClient client;

		private bool isReversed;

		private bool inProgress;

		private bool isPaused;

		private float currentTime;

		public bool IsInitialized
		{
			get
			{
				if (target != null)
				{
					return client != null;
				}
				return false;
			}
		}

		public virtual bool OncePerController => true;

		public InterpolatorType Interpolator => interpolator;

		IUIAnimationInterpolator IUIAnimationItem.Interpolator => interpolator;

		IUIAnimationInterpolator IUIAnimationItemSerializable.Interpolator => interpolator;

		UIAnimationItemParams IUIAnimationItemSerializable.Parameters
		{
			get
			{
				return parameters;
			}
			set
			{
				parameters = value;
			}
		}

		public AnimationCurve Curve
		{
			get
			{
				if (parameters.Curve == null)
				{
					parameters.Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
				}
				return parameters.Curve;
			}
			set
			{
				parameters.Curve = value;
			}
		}

		public float Duration
		{
			get
			{
				return parameters.Duration;
			}
			set
			{
				parameters.Duration = Mathf.Max(0f, value);
			}
		}

		public bool Repeat
		{
			get
			{
				return parameters.Repeat;
			}
			set
			{
				parameters.Repeat = value;
			}
		}

		public float Delay
		{
			get
			{
				return parameters.Delay;
			}
			set
			{
				parameters.Delay = Mathf.Max(0f, value);
			}
		}

		public float ReverseDelay
		{
			get
			{
				return parameters.ReverseDelay;
			}
			set
			{
				parameters.ReverseDelay = Mathf.Max(0f, value);
			}
		}

		public bool AutoTrigger
		{
			get
			{
				return parameters.AutoTrigger;
			}
			set
			{
				if (parameters.AutoTrigger == value)
				{
					return;
				}
				parameters.AutoTrigger = value;
				if (Application.isPlaying)
				{
					if (value)
					{
						StartAnimation(!isReversed);
					}
					else
					{
						StopAnimation();
					}
				}
			}
		}

		public UIAnimationEventsSet Events
		{
			get
			{
				if (parameters.Events == null)
				{
					parameters.Events = new UIAnimationEventsSet();
				}
				return parameters.Events;
			}
		}

		public bool IsPaused
		{
			get
			{
				return isPaused;
			}
			set
			{
				isPaused = value;
			}
		}

		public bool InProgress => inProgress;

		public bool IsReversed => isReversed;

		UIComponent IUIAnimationItemInspectable.Target => target as UIComponent;

		float IUIAnimationItemInspectable.CurrentTime => currentTime;

		public event Action<IUIAnimationItem> ItemStart;

		public event Action<IUIAnimationItem> ItemStop;

		public UIAnimationItem()
		{
			parameters.Duration = 1f;
		}

		public bool IsValidTarget(UIComponent potentialTarget)
		{
			return potentialTarget is TargetType;
		}

		public void Initialize(IUIAnimationItemClient client, object target, bool setValuesToDefault = false)
		{
			this.client = client;
			this.target = target as TargetType;
			if (this.target != null && setValuesToDefault)
			{
				InitValues();
			}
		}

		public T GetMin<T>()
		{
			UIAnimationInterpolator<T> uIAnimationInterpolator = interpolator as UIAnimationInterpolator<T>;
			if (uIAnimationInterpolator != null)
			{
				return uIAnimationInterpolator.Min;
			}
			Diagnostics.LogWarning($"Invalid type '{typeof(T)}' - Expected '{interpolator.Min.GetType()}'");
			return default(T);
		}

		public void SetMin<T>(T value)
		{
			UIAnimationInterpolator<T> uIAnimationInterpolator = interpolator as UIAnimationInterpolator<T>;
			if (uIAnimationInterpolator != null)
			{
				uIAnimationInterpolator.Min = value;
			}
			else
			{
				Diagnostics.LogWarning($"Invalid type '{typeof(T)}' - Expected '{interpolator.Min.GetType()}'");
			}
		}

		public T GetMax<T>()
		{
			UIAnimationInterpolator<T> uIAnimationInterpolator = interpolator as UIAnimationInterpolator<T>;
			if (uIAnimationInterpolator != null)
			{
				return uIAnimationInterpolator.Max;
			}
			Diagnostics.LogWarning($"Invalid type '{typeof(T)}' - Expected '{interpolator.Max.GetType()}'");
			return default(T);
		}

		public void SetMax<T>(T value)
		{
			UIAnimationInterpolator<T> uIAnimationInterpolator = interpolator as UIAnimationInterpolator<T>;
			if (uIAnimationInterpolator != null)
			{
				uIAnimationInterpolator.Max = value;
			}
			else
			{
				Diagnostics.LogWarning($"Invalid type '{typeof(T)}' - Expected '{interpolator.Min.GetType()}'");
			}
		}

		public void StartAnimation(bool forward = true, bool ignoreDelays = false)
		{
			if (target != null)
			{
				isReversed = !forward;
				ResetAnimation(toStart: false, ignoreDelays);
				inProgress = true;
				isPaused = false;
				Events.ResetTriggered();
				UpdateAnimation();
				this.ItemStart?.Invoke(this);
				client.OnItemStart(this);
			}
		}

		public void ResetAnimation(bool toStart = true, bool applyValue = false, bool ignoreDelays = false)
		{
			Events.ResetTriggered();
			inProgress = false;
			currentTime = (ignoreDelays ? 0f : ((!isReversed) ? (0f - Delay) : (0f - ReverseDelay)));
			if (applyValue)
			{
				ResetValue(toStart);
			}
		}

		public void StopAnimation()
		{
			Events.ResetTriggered();
			if (inProgress)
			{
				inProgress = false;
				this.ItemStop?.Invoke(this);
				client.OnItemStop(this);
			}
		}

		public void ResetValue(bool toStart = true)
		{
			ApplyInterpolation(FixRatio(toStart ? 0f : 1f));
		}

		public void ReverseAnimation()
		{
			isReversed = !isReversed;
			if (inProgress)
			{
				currentTime = Duration - currentTime;
			}
		}

		public bool UpdateAnimation()
		{
			if (target != null && inProgress)
			{
				if (isPaused)
				{
					return true;
				}
				currentTime += Time.deltaTime / client.DurationFactor;
				bool flag = currentTime > Duration;
				if (flag && Repeat)
				{
					currentTime -= Duration;
					flag = false;
					Events.TryTrigger(this, FixRatio(1f), !isReversed);
					Events.ResetTriggered();
				}
				if (!flag && Duration > 0f)
				{
					float ratio = FixRatio(currentTime / Duration);
					ApplyInterpolation(ratio);
					Events.TryTrigger(this, ratio, !isReversed);
					return true;
				}
				Events.TryTrigger(this, FixRatio(1f), !isReversed);
				ResetValue(toStart: false);
				inProgress = false;
				this.ItemStop?.Invoke(this);
				return false;
			}
			return false;
		}

		public string GetTargetName()
		{
			string text = ((target != null) ? target.GetType().ToString() : typeof(TargetType).ToString());
			text = text.Substring(text.LastIndexOf('.') + 1);
			if (target == null)
			{
				text = "<" + text + ">";
			}
			return text;
		}

		public abstract string GetShortName();

		void IUIAnimationEventsSetClient.TriggerEvent(UIAnimationEvent triggeredEvent)
		{
		}

		protected abstract void InitValues();

		protected abstract void Apply(Type value);

		private float FixRatio(float ratio)
		{
			if (!isReversed)
			{
				return ratio;
			}
			return 1f - ratio;
		}

		private void ApplyInterpolation(float ratio)
		{
			if (target != null)
			{
				UIBehaviour uIBehaviour = target as UIBehaviour;
				if (!(uIBehaviour != null) || uIBehaviour.Loaded)
				{
					float t = Curve.Evaluate(ratio);
					Apply(interpolator.Interpolate(t));
				}
			}
		}
	}
	public abstract class UIAnimationItem
	{
		public readonly StaticString Name;

		private float duration;

		private float startTime;

		private AnimationCurve curve;

		public float Duration
		{
			get
			{
				return duration;
			}
			set
			{
				duration = value;
			}
		}

		public float StartTime
		{
			get
			{
				return startTime;
			}
			set
			{
				startTime = value;
			}
		}

		public AnimationCurve Curve
		{
			get
			{
				return curve;
			}
			set
			{
				curve = value;
			}
		}

		protected UIAnimationItem(StaticString name)
		{
			Name = name;
		}

		internal virtual void LoadFromAsset(UIAnimationItemAsset itemAsset)
		{
			duration = itemAsset.Duration;
			startTime = itemAsset.StartTime;
			curve = itemAsset.Curve;
		}

		internal abstract void Update(UIBehaviour target, float ratio);
	}
}
