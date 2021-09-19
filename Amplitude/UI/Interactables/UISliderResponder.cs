using System;
using Amplitude.Framework.Input;
using Amplitude.UI.Audio;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public class UISliderResponder : UIControlResponder
	{
		private float valueBeforeDrag = -1f;

		private IUISlider Slider => base.Interactable as IUISlider;

		public event Action<IUISlider, float> ValueChange;

		public UISliderResponder(IUISlider slider)
			: base(slider)
		{
		}

		public void OnCurrentValueChanged(bool silent = false)
		{
			SetThumbPositionFromCurrentValue();
			if (!silent)
			{
				this.ValueChange?.Invoke(Slider, Slider.CurrentValue);
				Slider.TryTriggerAudioEvent(AudioEvent.Interactivity.SliderValueChange);
			}
		}

		public void Start()
		{
			if (Slider != null)
			{
				Slider.MouseEnter += Thumb_MouseEnter;
				Slider.MouseLeave += Thumb_MouseLeave;
				if (Slider.Thumb != null && Slider.Thumb.Loaded)
				{
					Slider.Thumb.DragStart += Thumb_DragStart;
					Slider.Thumb.DragMove += Thumb_DragMove;
					Slider.Thumb.DragComplete += Thumb_DragComplete;
					Slider.Thumb.DragCancel += Thumb_DragCancel;
				}
			}
		}

		public void Stop()
		{
			if (Slider != null)
			{
				Slider.MouseEnter -= Thumb_MouseEnter;
				Slider.MouseLeave -= Thumb_MouseLeave;
				if (Slider.Thumb != null && Slider.Thumb.Loaded)
				{
					Slider.Thumb.DragStart -= Thumb_DragStart;
					Slider.Thumb.DragMove -= Thumb_DragMove;
					Slider.Thumb.DragComplete -= Thumb_DragComplete;
					Slider.Thumb.DragCancel -= Thumb_DragCancel;
				}
			}
		}

		protected override void OnMouseDown(ref InputEvent mouseEvent, bool isMouseInside)
		{
			base.OnMouseDown(ref mouseEvent, isMouseInside);
			if (mouseEvent.Button == MouseButton.Left)
			{
				SetCurrentValueFromMousePosition(mouseEvent.MousePosition);
				Slider.Thumb.ForceDragStart(ref mouseEvent);
			}
		}

		private void SetCurrentValueFromMousePosition(Vector2 mousePosition)
		{
			UITransform parent = Slider.ThumbTransform.Parent;
			float value = Vector2.Dot(mousePosition - new Vector2(parent.GlobalRect.xMin, parent.GlobalRect.yMin) - Slider.UsableSegment.Origin, Slider.UsableSegment.AsVector().normalized) / Slider.UsableSegment.AsVector().magnitude * (Slider.Max - Slider.Min) + Slider.Min;
			Slider.SetCurrentValue(value, force: false, Slider.ValueChangeOnlyOnDragEnd);
		}

		private void SetThumbPositionFromCurrentValue()
		{
			float num = (Slider.CurrentValue - Slider.Min) / (Slider.Max - Slider.Min);
			Vector2 vector = new Vector2(num * Slider.UsableSegment.AsVector().x, num * Slider.UsableSegment.AsVector().y);
			Slider.ThumbTransform.Position = Slider.UsableSegment.Origin + vector;
		}

		private void Thumb_DragStart(IUIDragArea dragArea, Vector2 mousePosition)
		{
			valueBeforeDrag = Slider.CurrentValue;
		}

		private void Thumb_DragMove(IUIDragArea dragArea, Vector2 mousePosition, Vector2 offset)
		{
			SetCurrentValueFromMousePosition(mousePosition);
		}

		private void Thumb_DragComplete(IUIDragArea dragArea, Vector2 mousePosition)
		{
			if (Slider.ValueChangeOnlyOnDragEnd)
			{
				this.ValueChange?.Invoke(Slider, Slider.CurrentValue);
			}
			valueBeforeDrag = -1f;
		}

		private void Thumb_DragCancel(IUIDragArea dragArea, Vector2 mousePosition)
		{
			this.ValueChange?.Invoke(Slider, valueBeforeDrag);
			valueBeforeDrag = -1f;
		}

		private void Thumb_MouseEnter(IUIControl thumb, Vector2 mousePosition)
		{
			Slider.TryTriggerAudioEvent(AudioEvent.Interactivity.SliderThumbMouseEnter);
		}

		private void Thumb_MouseLeave(IUIControl thumb, Vector2 mousePosition)
		{
			Slider.TryTriggerAudioEvent(AudioEvent.Interactivity.SliderThumbMouseLeave);
		}
	}
}
