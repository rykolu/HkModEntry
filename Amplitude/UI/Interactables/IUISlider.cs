using System;

namespace Amplitude.UI.Interactables
{
	public interface IUISlider : IUIControl, IUIInteractable
	{
		IUIDragArea Thumb { get; }

		UITransform ThumbTransform { get; }

		OrientedSegment2 UsableSegment { get; set; }

		bool ValueChangeOnlyOnDragEnd { get; set; }

		float Min { get; set; }

		float Max { get; set; }

		float Step { get; set; }

		float CurrentValue { get; }

		bool IsDragging { get; }

		event Action<IUISlider, float> ValueChange;

		void SetCurrentValue(float value, bool force = false, bool silent = false);

		void CancelDrag(ref InputEvent inputEvent);
	}
}
