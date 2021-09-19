using System;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UITransform))]
	public class UIScrollBar : UIControl, IUIScrollBar, IUIControl, IUIInteractable
	{
		[SerializeField]
		[Tooltip("The whole area of the scrollbar, the user can click on it\n(it moves the thumb towards the click by 90% of the viewable content). ")]
		private UIControl trackZone;

		[SerializeField]
		[Tooltip("The draggable thumb of the scrollbar.")]
		private UIDragArea thumb;

		[SerializeField]
		[Tooltip("The direction of the scroll bar.")]
		private Orientation orientation = Orientation.Vertical;

		[SerializeField]
		[Tooltip("Hide the scrollbar if the content is smaller than the viewport.")]
		private bool autoHide;

		[SerializeField]
		[HideInInspector]
		private bool pushContentAside;

		public IUIControl TrackZone => trackZone;

		public UITransform TrackZoneTransform => trackZone.UITransform;

		public IUIDragArea Thumb => thumb;

		public UITransform ThumbTransform
		{
			get
			{
				if (thumb != null && thumb.Loaded)
				{
					return thumb.UITransform;
				}
				return null;
			}
		}

		public Orientation Type
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
				}
			}
		}

		public bool AutoHide
		{
			get
			{
				return autoHide;
			}
			set
			{
				if (autoHide != value)
				{
					autoHide = value;
				}
			}
		}

		public bool PushContentAside
		{
			get
			{
				return pushContentAside;
			}
			set
			{
				if (pushContentAside != value)
				{
					pushContentAside = value;
				}
			}
		}

		private UIScrollBarResponder ScrollBarResponder => (UIScrollBarResponder)base.Responder;

		public event Action<IUIScrollBar, float> ThumbPositionChange
		{
			add
			{
				ScrollBarResponder.ThumbPositionChange += value;
			}
			remove
			{
				ScrollBarResponder.ThumbPositionChange -= value;
			}
		}

		public void OnVisibleAreaChanged(float visibleAreaPercentageBegin, float visibleAreaPercentageEnd)
		{
			if (!(ThumbTransform == null))
			{
				if (orientation == Orientation.Horizontal)
				{
					ThumbTransform.Left = TrackZoneTransform.Width * visibleAreaPercentageBegin;
					ThumbTransform.Width = TrackZoneTransform.Width * (visibleAreaPercentageEnd - visibleAreaPercentageBegin);
				}
				else
				{
					ThumbTransform.Top = TrackZoneTransform.Height * visibleAreaPercentageBegin;
					ThumbTransform.Height = TrackZoneTransform.Height * (visibleAreaPercentageEnd - visibleAreaPercentageBegin);
				}
			}
		}

		protected override IUIResponder InstantiateResponder()
		{
			return new UIScrollBarResponder(this);
		}

		protected override void Load()
		{
			if (trackZone != this)
			{
				trackZone?.LoadIfNecessary();
			}
			thumb?.LoadIfNecessary();
			base.Load();
			if (thumb != null)
			{
				thumb.AutoMove = false;
			}
			if (thumb != null && thumb.Loaded && trackZone != null && (trackZone.Loaded || trackZone == this))
			{
				ScrollBarResponder.Start();
			}
		}

		protected override void Unload()
		{
			ScrollBarResponder.Stop();
			base.Unload();
		}
	}
}
