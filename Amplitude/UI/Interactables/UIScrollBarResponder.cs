using System;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public class UIScrollBarResponder : UIControlResponder
	{
		private const float TrackZoneScrollingOffset = 0.9f;

		private IUIScrollBar ScrollBar => base.Interactable as IUIScrollBar;

		public event Action<IUIScrollBar, float> ThumbPositionChange;

		public UIScrollBarResponder(IUIScrollBar scrollBar)
			: base(scrollBar)
		{
		}

		public void Start()
		{
			if (ScrollBar.TrackZone != null && ScrollBar.TrackZone.Loaded)
			{
				ScrollBar.TrackZone.LeftMouseDown += TrackZone_LeftMouseDown;
			}
			if (ScrollBar.Thumb != null && ScrollBar.Thumb.Loaded)
			{
				ScrollBar.Thumb.DragMove += Thumb_DragMove;
			}
		}

		public void Stop()
		{
			if (ScrollBar.TrackZone != null && ScrollBar.TrackZone.Loaded)
			{
				ScrollBar.TrackZone.LeftMouseDown -= TrackZone_LeftMouseDown;
			}
			if (ScrollBar.Thumb != null && ScrollBar.Thumb.Loaded)
			{
				ScrollBar.Thumb.DragMove -= Thumb_DragMove;
			}
		}

		private void TrackZone_LeftMouseDown(IUIControl control, Vector2 mousePosition)
		{
			Vector2 vector = mousePosition - new Vector2(ScrollBar.TrackZoneTransform.GlobalRect.xMin, ScrollBar.TrackZoneTransform.GlobalRect.yMin);
			if (ScrollBar.Type == Orientation.Horizontal)
			{
				if (vector.x < ScrollBar.ThumbTransform.Left)
				{
					float a = ScrollBar.ThumbTransform.Left - ScrollBar.ThumbTransform.Width * 0.9f;
					a = Mathf.Max(a, 0f);
					this.ThumbPositionChange?.Invoke(ScrollBar, a / ScrollBar.TrackZoneTransform.Width);
				}
				else if (vector.x > ScrollBar.ThumbTransform.Bottom)
				{
					float a2 = ScrollBar.ThumbTransform.Right + ScrollBar.ThumbTransform.Width * 0.9f;
					a2 = Mathf.Min(a2, ScrollBar.TrackZoneTransform.Width);
					this.ThumbPositionChange?.Invoke(ScrollBar, (a2 - ScrollBar.ThumbTransform.Width) / ScrollBar.TrackZoneTransform.Width);
				}
			}
			else if (vector.y < ScrollBar.ThumbTransform.Top)
			{
				float a3 = ScrollBar.ThumbTransform.Top - ScrollBar.ThumbTransform.Height * 0.9f;
				a3 = Mathf.Max(a3, 0f);
				this.ThumbPositionChange?.Invoke(ScrollBar, a3 / ScrollBar.TrackZoneTransform.Height);
			}
			else if (vector.y > ScrollBar.ThumbTransform.Bottom)
			{
				float a4 = ScrollBar.ThumbTransform.Bottom + ScrollBar.ThumbTransform.Height * 0.9f;
				a4 = Mathf.Min(a4, ScrollBar.TrackZoneTransform.Height);
				this.ThumbPositionChange?.Invoke(ScrollBar, (a4 - ScrollBar.ThumbTransform.Height) / ScrollBar.TrackZoneTransform.Height);
			}
		}

		private void Thumb_DragMove(IUIDragArea dragArea, Vector2 mousePosition, Vector2 offset)
		{
			if (ScrollBar.Type == Orientation.Horizontal)
			{
				float a = ScrollBar.ThumbTransform.Left + offset.x;
				a = Mathf.Max(a, 0f);
				a = Mathf.Min(a, ScrollBar.TrackZoneTransform.Width - ScrollBar.ThumbTransform.Width);
				if (ScrollBar.ThumbTransform.Left != a)
				{
					this.ThumbPositionChange?.Invoke(ScrollBar, a / ScrollBar.TrackZoneTransform.Width);
				}
			}
			else
			{
				float a2 = ScrollBar.ThumbTransform.Top + offset.y;
				a2 = Mathf.Max(a2, 0f);
				a2 = Mathf.Min(a2, ScrollBar.TrackZoneTransform.Height - ScrollBar.ThumbTransform.Height);
				if (ScrollBar.ThumbTransform.Top != a2)
				{
					this.ThumbPositionChange?.Invoke(ScrollBar, a2 / ScrollBar.TrackZoneTransform.Height);
				}
			}
		}
	}
}
