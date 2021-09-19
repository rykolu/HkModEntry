using System;
using Amplitude.UI.Audio;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UITransform))]
	public class UISlider : UIControl, IUISlider, IUIControl, IUIInteractable, IAudioControllerOwner
	{
		[SerializeField]
		[Tooltip("The draggable thumb that show the current value and can be dragged.")]
		private UIDragArea thumb;

		[SerializeField]
		[HideInInspector]
		private DirectionWithCustom orientation = DirectionWithCustom.LeftToRight;

		[SerializeField]
		[HideInInspector]
		private OrientedSegment2 usableSegment = OrientedSegment2.Zero;

		[SerializeField]
		[HideInInspector]
		private float min;

		[SerializeField]
		[HideInInspector]
		private float max = 100f;

		[SerializeField]
		[HideInInspector]
		private float step = 1f;

		[SerializeField]
		[HideInInspector]
		private float currentValue = 50f;

		[SerializeField]
		[HideInInspector]
		private bool valueChangeOnlyOnDragEnd;

		[SerializeField]
		[HideInInspector]
		private AudioController audioController;

		public IUIDragArea Thumb => thumb;

		public UITransform ThumbTransform => thumb.UITransform;

		public DirectionWithCustom Orientation
		{
			get
			{
				return orientation;
			}
			set
			{
				if (orientation != value)
				{
					orientation = value;
					OnOrientationChanged();
				}
			}
		}

		public OrientedSegment2 UsableSegment
		{
			get
			{
				return usableSegment;
			}
			set
			{
				if (usableSegment != value)
				{
					usableSegment = value;
					OnUsableSegmentChanged();
				}
			}
		}

		public float Min
		{
			get
			{
				return min;
			}
			set
			{
				if (min != value)
				{
					float previousMin = min;
					min = value;
					OnMinChanged(previousMin, min);
				}
			}
		}

		public float Max
		{
			get
			{
				return max;
			}
			set
			{
				if (max != value)
				{
					float previousMax = max;
					max = value;
					OnMaxChanged(previousMax, max);
				}
			}
		}

		public float Step
		{
			get
			{
				return step;
			}
			set
			{
				if (step != value)
				{
					float previousStep = step;
					step = value;
					OnStepChanged(previousStep, step);
				}
			}
		}

		public float CurrentValue => currentValue;

		public bool ValueChangeOnlyOnDragEnd
		{
			get
			{
				return valueChangeOnlyOnDragEnd;
			}
			set
			{
				valueChangeOnlyOnDragEnd = value;
			}
		}

		public bool IsDragging => Thumb.IsDragging;

		public AudioController AudioController => audioController;

		private UISliderResponder SliderResponder => (UISliderResponder)base.Responder;

		public event Action<IUISlider, float> ValueChange
		{
			add
			{
				SliderResponder.ValueChange += value;
			}
			remove
			{
				SliderResponder.ValueChange -= value;
			}
		}

		void IUISlider.CancelDrag(ref InputEvent inputEvent)
		{
			if (Thumb.IsDragging)
			{
				Thumb.ForceDragCancel(ref inputEvent);
			}
		}

		public void SetCurrentValue(float value, bool force = false, bool silent = false)
		{
			float num = currentValue;
			currentValue = ConstrainWithStep(Mathf.Clamp(value, Min, Max));
			if (force || currentValue != num)
			{
				SliderResponder.OnCurrentValueChanged(silent);
			}
		}

		public void SetAudioProfile(AudioProfile audioProfile)
		{
			audioController.Profile = audioProfile;
		}

		void IAudioControllerOwner.InitializeAudioProfile(AudioProfile audioProfile)
		{
			audioProfile.Initialize(AudioEvent.Interactivity.MouseEnter, AudioEvent.Interactivity.MouseLeave, AudioEvent.Interactivity.SliderValueChange);
		}

		protected override IUIResponder InstantiateResponder()
		{
			return new UISliderResponder(this);
		}

		protected override void Load()
		{
			thumb?.LoadIfNecessary();
			base.Load();
			if (thumb != null)
			{
				thumb.AutoMove = false;
				SliderResponder.Start();
			}
			if (ThumbTransform != null && ThumbTransform.Loaded && ThumbTransform.Parent != null)
			{
				ThumbTransform.Parent.PositionOrSizeChange += ThumbContainer_PositionOrSizeChange;
				ThumbContainer_PositionOrSizeChange(positionChanged: true, sizeChanged: true);
			}
		}

		protected override void Unload()
		{
			if (ThumbTransform != null && ThumbTransform.Loaded && ThumbTransform.Parent != null)
			{
				ThumbTransform.Parent.PositionOrSizeChange -= ThumbContainer_PositionOrSizeChange;
			}
			SliderResponder.Stop();
			base.Unload();
		}

		private void RefreshOrientation()
		{
			UITransform parent = ThumbTransform.Parent;
			if (Orientation == DirectionWithCustom.LeftToRight)
			{
				UsableSegment = new OrientedSegment2(new Vector2(0f, parent.Height / 2f), new Vector2(parent.Width, parent.Height / 2f));
			}
			else if (Orientation == DirectionWithCustom.RightToLeft)
			{
				UsableSegment = new OrientedSegment2(new Vector2(parent.Width, parent.Height / 2f), new Vector2(0f, parent.Height / 2f));
			}
			else if (Orientation == DirectionWithCustom.TopToBottom)
			{
				UsableSegment = new OrientedSegment2(new Vector2(parent.Width / 2f, 0f), new Vector2(parent.Width / 2f, parent.Height));
			}
			else if (Orientation == DirectionWithCustom.BottomToTop)
			{
				UsableSegment = new OrientedSegment2(new Vector2(parent.Width / 2f, parent.Height), new Vector2(parent.Width / 2f, 0f));
			}
			else if (UsableSegment.Length == 0f)
			{
				UsableSegment = new OrientedSegment2(new Vector2(0f, parent.Height / 2f), new Vector2(parent.Width, parent.Height / 2f));
			}
		}

		private float ConstrainWithStep(float rawValue)
		{
			int num = Mathf.RoundToInt(rawValue / Step);
			float num2 = (float)num * Step;
			if (num2 < min)
			{
				num2 = (float)(num + 1) * Step;
			}
			else if (num2 > max)
			{
				num2 = (float)(num - 1) * Step;
			}
			return num2;
		}

		private void OnOrientationChanged()
		{
			RefreshOrientation();
			SliderResponder.OnCurrentValueChanged(silent: true);
		}

		private void OnUsableSegmentChanged()
		{
			SliderResponder.OnCurrentValueChanged(silent: true);
		}

		private void OnMinChanged(float previousMin, float newMin)
		{
			if (min >= max)
			{
				min = previousMin;
			}
			else
			{
				SetCurrentValue(currentValue, force: true, silent: true);
			}
		}

		private void OnMaxChanged(float previousMax, float newMax)
		{
			if (max <= min)
			{
				max = previousMax;
			}
			else
			{
				SetCurrentValue(currentValue, force: true, silent: true);
			}
		}

		private void OnStepChanged(float previousStep, float newStep)
		{
			if (step == 0f)
			{
				step = previousStep;
				return;
			}
			if (step < 0f)
			{
				step = 0f - step;
			}
			SetCurrentValue(currentValue, force: false, silent: true);
		}

		private void OnCurrentValueChanged(float previousValue, float newValue)
		{
			currentValue = ConstrainWithStep(Mathf.Clamp(newValue, Min, Max));
			if (currentValue != previousValue)
			{
				SliderResponder.OnCurrentValueChanged(silent: true);
			}
		}

		private void ThumbContainer_PositionOrSizeChange(bool positionChanged, bool sizeChanged)
		{
			if (sizeChanged)
			{
				RefreshOrientation();
				SliderResponder.OnCurrentValueChanged(silent: true);
			}
		}
	}
}
