using Amplitude.UI.Audio;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UITransform))]
	public class UIScrollView : UIControl, IUIScrollView, IUIControl, IUIInteractable, IAudioControllerOwner
	{
		public const float PixelsPerIncrement = 20f;

		[SerializeField]
		private UITransform content;

		[SerializeField]
		private UITransform viewport;

		[SerializeField]
		[HideInInspector]
		private bool scrollHorizontally;

		[SerializeField]
		[HideInInspector]
		private bool scrollVertically;

		[SerializeField]
		[HideInInspector]
		private UIScrollBar horizontalScrollBar;

		[SerializeField]
		[HideInInspector]
		private UIScrollBar verticalScrollBar;

		[SerializeField]
		[HideInInspector]
		private HorizontalDirection horizontalScrollingDirection = HorizontalDirection.LeftToRight;

		[SerializeField]
		[HideInInspector]
		private VerticalDirection verticalScrollingDirection = VerticalDirection.TopToBottom;

		[SerializeField]
		[HideInInspector]
		private float horizontalScrollingSpeed = 1f;

		[SerializeField]
		[HideInInspector]
		private float verticalScrollingSpeed = 1f;

		[SerializeField]
		[HideInInspector]
		private bool autoAdjustWidth;

		[SerializeField]
		[HideInInspector]
		private float minWidth;

		[SerializeField]
		[HideInInspector]
		private float maxWidth;

		[SerializeField]
		[HideInInspector]
		private bool autoAdjustHeight;

		[SerializeField]
		[HideInInspector]
		private float minHeight;

		[SerializeField]
		[HideInInspector]
		private float maxHeight;

		[SerializeField]
		[HideInInspector]
		private float leftMargin;

		[SerializeField]
		[HideInInspector]
		private float rightMargin;

		[SerializeField]
		[HideInInspector]
		private float topMargin;

		[SerializeField]
		[HideInInspector]
		private float bottomMargin;

		[SerializeField]
		[HideInInspector]
		private bool onlyBlockMouseScrollIfActive;

		[SerializeField]
		[HideInInspector]
		private bool ignoreMouseScrollEvent;

		[SerializeField]
		[HideInInspector]
		private AudioController audioController;

		public UITransform Content
		{
			get
			{
				if (content != null && content.Loaded)
				{
					return content;
				}
				return null;
			}
			set
			{
				if (content != value)
				{
					UITransform previousContent = content;
					content = value;
					ScrollViewResponder.OnContentChanged(previousContent, content);
					ScrollViewResponder.Refresh();
				}
			}
		}

		public UITransform Viewport
		{
			get
			{
				if (viewport != null && viewport.Loaded)
				{
					return viewport;
				}
				return null;
			}
			set
			{
				UITransform previousViewport = viewport;
				viewport = value;
				ScrollViewResponder.OnViewportChanged(previousViewport, viewport);
				ScrollViewResponder.Refresh();
			}
		}

		public bool ScrollHorizontally
		{
			get
			{
				return scrollHorizontally;
			}
			set
			{
				if (scrollHorizontally != value)
				{
					scrollHorizontally = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public bool ScrollVertically
		{
			get
			{
				return scrollVertically;
			}
			set
			{
				if (scrollVertically != value)
				{
					scrollVertically = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public IUIScrollBar HorizontalScrollBar
		{
			get
			{
				return horizontalScrollBar;
			}
			set
			{
				UIScrollBar uIScrollBar = value as UIScrollBar;
				if (horizontalScrollBar != uIScrollBar)
				{
					IUIScrollBar previousHorizontalScrollBar = horizontalScrollBar;
					horizontalScrollBar = uIScrollBar;
					horizontalScrollBar?.LoadIfNecessary();
					ScrollViewResponder.OnHorizontalScrollBarChanged(previousHorizontalScrollBar, HorizontalScrollBar);
					ScrollViewResponder.Refresh();
				}
			}
		}

		public IUIScrollBar VerticalScrollBar
		{
			get
			{
				return verticalScrollBar;
			}
			set
			{
				UIScrollBar uIScrollBar = value as UIScrollBar;
				if (verticalScrollBar != uIScrollBar)
				{
					IUIScrollBar previousVerticalScrollBar = verticalScrollBar;
					verticalScrollBar = uIScrollBar;
					verticalScrollBar?.LoadIfNecessary();
					ScrollViewResponder.OnVerticalScrollBarChanged(previousVerticalScrollBar, VerticalScrollBar);
					ScrollViewResponder.Refresh();
				}
			}
		}

		public HorizontalDirection HorizontalScrollingDirection
		{
			get
			{
				return horizontalScrollingDirection;
			}
			set
			{
				if (horizontalScrollingDirection != value)
				{
					horizontalScrollingDirection = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public VerticalDirection VerticalScrollingDirection
		{
			get
			{
				return verticalScrollingDirection;
			}
			set
			{
				if (verticalScrollingDirection != value)
				{
					verticalScrollingDirection = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public float HorizontalScrollingSpeed
		{
			get
			{
				return horizontalScrollingSpeed;
			}
			set
			{
				horizontalScrollingSpeed = value;
			}
		}

		public float VerticalScrollingSpeed
		{
			get
			{
				return verticalScrollingSpeed;
			}
			set
			{
				verticalScrollingSpeed = value;
			}
		}

		public bool AutoAdjustWidth
		{
			get
			{
				return autoAdjustWidth;
			}
			set
			{
				if (autoAdjustWidth != value)
				{
					autoAdjustWidth = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public float MinWidth
		{
			get
			{
				return minWidth;
			}
			set
			{
				if (minWidth != value)
				{
					minWidth = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public float MaxWidth
		{
			get
			{
				return maxWidth;
			}
			set
			{
				if (maxWidth != value)
				{
					maxWidth = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public bool AutoAdjustHeight
		{
			get
			{
				return autoAdjustHeight;
			}
			set
			{
				if (autoAdjustHeight != value)
				{
					autoAdjustHeight = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public float MinHeight
		{
			get
			{
				return minHeight;
			}
			set
			{
				if (minHeight != value)
				{
					minHeight = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public float MaxHeight
		{
			get
			{
				return maxHeight;
			}
			set
			{
				if (maxHeight != value)
				{
					maxHeight = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public float LeftMargin
		{
			get
			{
				return leftMargin;
			}
			set
			{
				if (leftMargin != value)
				{
					leftMargin = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public float RightMargin
		{
			get
			{
				return rightMargin;
			}
			set
			{
				if (rightMargin != value)
				{
					rightMargin = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public float TopMargin
		{
			get
			{
				return topMargin;
			}
			set
			{
				if (topMargin != value)
				{
					topMargin = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public float BottomMargin
		{
			get
			{
				return bottomMargin;
			}
			set
			{
				if (bottomMargin != value)
				{
					bottomMargin = value;
					ScrollViewResponder.Refresh();
				}
			}
		}

		public bool OnlyBlockMouseScrollIfActive
		{
			get
			{
				return onlyBlockMouseScrollIfActive;
			}
			set
			{
				onlyBlockMouseScrollIfActive = value;
			}
		}

		public override UIEventBlockingMask BlockedEvents
		{
			get
			{
				UIEventBlockingMask uIEventBlockingMask = base.BlockedEvents;
				if (onlyBlockMouseScrollIfActive && !ScrollViewResponder.IsScrollActive)
				{
					uIEventBlockingMask &= ~UIEventBlockingMask.MouseScroll;
				}
				return uIEventBlockingMask;
			}
			set
			{
				base.BlockedEvents = value;
			}
		}

		public bool IgnoreMouseScrollEvent
		{
			get
			{
				return ignoreMouseScrollEvent;
			}
			set
			{
				ignoreMouseScrollEvent = value;
			}
		}

		public AudioController AudioController => audioController;

		private UIScrollViewResponder ScrollViewResponder => (UIScrollViewResponder)base.Responder;

		public void Reset(bool toEnd = false)
		{
			ResetHorizontally(toEnd);
			ResetVertically(toEnd);
		}

		public void ResetHorizontally(bool toEnd = false)
		{
			if (scrollHorizontally)
			{
				if ((horizontalScrollingDirection == HorizontalDirection.LeftToRight && !toEnd) || (horizontalScrollingDirection == HorizontalDirection.RightToLeft && toEnd))
				{
					content.Left = leftMargin;
				}
				if ((horizontalScrollingDirection == HorizontalDirection.RightToLeft && !toEnd) || (horizontalScrollingDirection == HorizontalDirection.LeftToRight && toEnd))
				{
					content.Right = viewport.Width - rightMargin;
				}
			}
		}

		public void ResetVertically(bool toEnd = false)
		{
			if (scrollVertically)
			{
				if ((verticalScrollingDirection == VerticalDirection.TopToBottom && !toEnd) || (verticalScrollingDirection == VerticalDirection.BottomToTop && toEnd))
				{
					content.Top = topMargin;
				}
				if ((verticalScrollingDirection == VerticalDirection.BottomToTop && !toEnd) || (verticalScrollingDirection == VerticalDirection.TopToBottom && toEnd))
				{
					content.Bottom = viewport.Height - bottomMargin;
				}
			}
		}

		public void SetAudioProfile(AudioProfile audioProfile)
		{
			audioController.Profile = audioProfile;
		}

		void IAudioControllerOwner.InitializeAudioProfile(AudioProfile audioProfile)
		{
			audioProfile.Initialize(AudioEvent.Interactivity.MouseEnter, AudioEvent.Interactivity.MouseLeave, AudioEvent.Interactivity.ScrollViewThumbMove);
		}

		protected override IUIResponder InstantiateResponder()
		{
			return new UIScrollViewResponder(this);
		}

		protected override void Load()
		{
			content?.LoadIfNecessary();
			viewport?.LoadIfNecessary();
			horizontalScrollBar?.LoadIfNecessary();
			verticalScrollBar?.LoadIfNecessary();
			base.Load();
			if (maxWidth == 0f)
			{
				maxWidth = UITransform.Width;
			}
			if (maxHeight == 0f)
			{
				maxHeight = UITransform.Height;
			}
			if (content != null && viewport != null && content.Parent != viewport)
			{
				Diagnostics.LogWarning("In the UIScrollView '{0}' the content's parent is not the viewport.", this);
			}
			OnContentChanged(null, content);
			OnViewportChanged(null, viewport);
			OnHorizontalScrollBarChanged(null, HorizontalScrollBar);
			OnVerticalScrollBarChanged(null, VerticalScrollBar);
			ScrollViewResponder.Refresh();
		}

		protected override void Unload()
		{
			OnContentChanged(content, null);
			OnViewportChanged(viewport, null);
			OnHorizontalScrollBarChanged(horizontalScrollBar, null);
			OnVerticalScrollBarChanged(verticalScrollBar, null);
			base.Unload();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (base.Loaded)
			{
				ScrollViewResponder.Refresh();
			}
		}

		private void OnContentChanged(UITransform previousContent, UITransform newContent)
		{
			ScrollViewResponder.OnContentChanged(previousContent, newContent);
		}

		private void OnViewportChanged(UITransform previousViewport, UITransform newViewport)
		{
			ScrollViewResponder.OnViewportChanged(previousViewport, newViewport);
		}

		private void OnHorizontalScrollBarChanged(IUIScrollBar previousHorizontalScrollBar, IUIScrollBar newHorizontalScrollBar)
		{
			ScrollViewResponder.OnHorizontalScrollBarChanged(previousHorizontalScrollBar, newHorizontalScrollBar);
		}

		private void OnVerticalScrollBarChanged(IUIScrollBar previousVerticalScrollBar, IUIScrollBar newVerticalScrollBar)
		{
			ScrollViewResponder.OnVerticalScrollBarChanged(previousVerticalScrollBar, newVerticalScrollBar);
		}
	}
}
