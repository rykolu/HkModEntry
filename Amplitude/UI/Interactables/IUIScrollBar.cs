using System;

namespace Amplitude.UI.Interactables
{
	public interface IUIScrollBar : IUIControl, IUIInteractable
	{
		IUIControl TrackZone { get; }

		UITransform TrackZoneTransform { get; }

		IUIDragArea Thumb { get; }

		UITransform ThumbTransform { get; }

		Orientation Type { get; set; }

		bool AutoHide { get; set; }

		bool PushContentAside { get; set; }

		event Action<IUIScrollBar, float> ThumbPositionChange;

		void OnVisibleAreaChanged(float visibleAreaPercentageBegin, float visibleAreaPercentageEnd);
	}
}
