using Amplitude.UI.Audio;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public class UIScrollViewResponder : UIControlResponder
	{
		public const float PixelsPerIncrement = 200f;

		public const float Epsilon = 0.01f;

		public IUIScrollView ScrollView => base.Interactable as IUIScrollView;

		public bool IsScrollActive { get; private set; }

		public UIScrollViewResponder(IUIScrollView scrollView)
			: base(scrollView)
		{
		}

		public void Refresh()
		{
			if (!(ScrollView.Content == null) && !(ScrollView.Viewport == null))
			{
				UpdateContentMarginsIfNecessary();
				ConstrainContent();
				UpdateScrollBars();
				UpdateBlockedEvent();
			}
		}

		public void UpdateBlockedEvent()
		{
			IsScrollActive = false;
			if (ScrollView.ScrollHorizontally)
			{
				IsScrollActive |= ScrollView.Content.Width > ScrollView.Viewport.Width - ScrollView.LeftMargin - ScrollView.RightMargin;
			}
			if (ScrollView.ScrollVertically)
			{
				IsScrollActive |= ScrollView.Content.Height > ScrollView.Viewport.Height - ScrollView.TopMargin - ScrollView.BottomMargin;
			}
		}

		public void OnContentChanged(UITransform previousContent, UITransform newContent)
		{
			if (previousContent != null)
			{
				previousContent.PositionOrSizeChange -= Content_PositionOrSizeChange;
			}
			if (newContent != null)
			{
				newContent.PositionOrSizeChange += Content_PositionOrSizeChange;
			}
		}

		public void OnViewportChanged(UITransform previousViewport, UITransform newViewport)
		{
			if (previousViewport != null)
			{
				previousViewport.PositionOrSizeChange -= Viewport_PositionOrSizeChange;
			}
			if (newViewport != null)
			{
				newViewport.PositionOrSizeChange += Viewport_PositionOrSizeChange;
			}
		}

		public void OnHorizontalScrollBarChanged(IUIScrollBar previousHorizontalScrollBar, IUIScrollBar newHorizontalScrollBar)
		{
			if (previousHorizontalScrollBar != null)
			{
				previousHorizontalScrollBar.ThumbPositionChange -= HorizontalScrollBar_ThumbPositionChange;
				previousHorizontalScrollBar.MouseScroll -= HorizontalScrollBar_MouseScroll;
				previousHorizontalScrollBar.AxisUpdate2D -= HorizontalScrollBar_AxisUpdate2D;
				if (previousHorizontalScrollBar.Thumb != null && previousHorizontalScrollBar.Thumb.Loaded)
				{
					previousHorizontalScrollBar.Thumb.MouseEnter -= Thumb_MouseEnter;
					previousHorizontalScrollBar.Thumb.MouseLeave -= Thumb_MouseLeave;
				}
				if (previousHorizontalScrollBar.TrackZone != null && previousHorizontalScrollBar.TrackZone.Loaded)
				{
					previousHorizontalScrollBar.TrackZone.MouseEnter -= TrackZone_MouseEnter;
					previousHorizontalScrollBar.TrackZone.MouseLeave -= TrackZone_MouseLeave;
				}
			}
			if (newHorizontalScrollBar != null)
			{
				newHorizontalScrollBar.ThumbPositionChange += HorizontalScrollBar_ThumbPositionChange;
				newHorizontalScrollBar.MouseScroll += HorizontalScrollBar_MouseScroll;
				newHorizontalScrollBar.AxisUpdate2D += HorizontalScrollBar_AxisUpdate2D;
				if (newHorizontalScrollBar.Thumb != null && newHorizontalScrollBar.Thumb.Loaded)
				{
					newHorizontalScrollBar.Thumb.MouseEnter += Thumb_MouseEnter;
					newHorizontalScrollBar.Thumb.MouseLeave += Thumb_MouseLeave;
				}
				if (newHorizontalScrollBar.TrackZone != null && newHorizontalScrollBar.TrackZone.Loaded)
				{
					newHorizontalScrollBar.TrackZone.MouseEnter += TrackZone_MouseEnter;
					newHorizontalScrollBar.TrackZone.MouseLeave += TrackZone_MouseLeave;
				}
			}
		}

		public void OnVerticalScrollBarChanged(IUIScrollBar previousVerticalScrollBar, IUIScrollBar newVerticalScrollBar)
		{
			if (previousVerticalScrollBar != null)
			{
				previousVerticalScrollBar.ThumbPositionChange -= VerticalScrollBar_ThumbPositionChange;
				previousVerticalScrollBar.MouseScroll -= VerticalScrollBar_MouseScroll;
				previousVerticalScrollBar.AxisUpdate2D -= VerticalScrollBar_AxisUpdate2D;
				if (previousVerticalScrollBar.Thumb != null && previousVerticalScrollBar.Thumb.Loaded)
				{
					previousVerticalScrollBar.Thumb.MouseEnter -= Thumb_MouseEnter;
					previousVerticalScrollBar.Thumb.MouseLeave -= Thumb_MouseLeave;
				}
				if (previousVerticalScrollBar.TrackZone != null && previousVerticalScrollBar.TrackZone.Loaded)
				{
					previousVerticalScrollBar.TrackZone.MouseEnter -= TrackZone_MouseEnter;
					previousVerticalScrollBar.TrackZone.MouseLeave -= TrackZone_MouseLeave;
				}
			}
			if (newVerticalScrollBar != null)
			{
				newVerticalScrollBar.ThumbPositionChange += VerticalScrollBar_ThumbPositionChange;
				newVerticalScrollBar.MouseScroll += VerticalScrollBar_MouseScroll;
				newVerticalScrollBar.AxisUpdate2D += VerticalScrollBar_AxisUpdate2D;
				if (newVerticalScrollBar.Thumb != null && newVerticalScrollBar.Thumb.Loaded)
				{
					newVerticalScrollBar.Thumb.MouseEnter += Thumb_MouseEnter;
					newVerticalScrollBar.Thumb.MouseLeave += Thumb_MouseLeave;
				}
				if (newVerticalScrollBar.TrackZone != null && newVerticalScrollBar.TrackZone.Loaded)
				{
					newVerticalScrollBar.TrackZone.MouseEnter += TrackZone_MouseEnter;
					newVerticalScrollBar.TrackZone.MouseLeave += TrackZone_MouseLeave;
				}
			}
		}

		protected override void OnMouseScroll(ref InputEvent mouseScrollEvent, bool isMouseInside)
		{
			base.OnMouseScroll(ref mouseScrollEvent, isMouseInside);
			if (!ScrollView.IgnoreMouseScrollEvent)
			{
				if (ScrollView.ScrollVertically)
				{
					ApplyVerticalScrolling(mouseScrollEvent.ScrollIncrement);
				}
				else if (ScrollView.ScrollHorizontally)
				{
					ApplyHorizontalScrolling(mouseScrollEvent.ScrollIncrement);
				}
			}
		}

		protected override void OnAxisUpdate2D(ref InputEvent axisUpdate2DEvent, bool isMouseInside)
		{
			base.OnAxisUpdate2D(ref axisUpdate2DEvent, isMouseInside);
			if (!ScrollView.IgnoreMouseScrollEvent)
			{
				if (ScrollView.ScrollHorizontally)
				{
					ApplyHorizontalScrolling(0f - axisUpdate2DEvent.AxisUpdate2D.x);
				}
				if (ScrollView.ScrollVertically)
				{
					ApplyVerticalScrolling(axisUpdate2DEvent.AxisUpdate2D.y);
				}
			}
		}

		private void UpdateContentMarginsIfNecessary()
		{
			UIBorderAnchor uIBorderAnchor3;
			if (!ScrollView.ScrollHorizontally)
			{
				UIBorderAnchor uIBorderAnchor = new UIBorderAnchor(attach: true, 0f, ScrollView.LeftMargin, ScrollView.Content.LeftAnchor.Offset);
				if (ScrollView.Content.LeftAnchor != uIBorderAnchor)
				{
					ScrollView.Content.LeftAnchor = uIBorderAnchor;
				}
				UIBorderAnchor uIBorderAnchor2 = new UIBorderAnchor(attach: true, 1f, ScrollView.RightMargin, ScrollView.Content.RightAnchor.Offset);
				if (ScrollView.Content.RightAnchor != uIBorderAnchor2)
				{
					ScrollView.Content.RightAnchor = uIBorderAnchor2;
				}
			}
			else
			{
				if (ScrollView.Content.LeftAnchor.Attach)
				{
					UITransform content = ScrollView.Content;
					uIBorderAnchor3 = new UIBorderAnchor(ScrollView.Content.LeftAnchor)
					{
						Attach = false
					};
					content.LeftAnchor = uIBorderAnchor3;
				}
				if (ScrollView.Content.RightAnchor.Attach)
				{
					UITransform content2 = ScrollView.Content;
					uIBorderAnchor3 = new UIBorderAnchor(ScrollView.Content.RightAnchor)
					{
						Attach = false
					};
					content2.RightAnchor = uIBorderAnchor3;
				}
			}
			if (!ScrollView.ScrollVertically)
			{
				UIBorderAnchor uIBorderAnchor4 = new UIBorderAnchor(attach: true, 0f, ScrollView.TopMargin, ScrollView.Content.TopAnchor.Offset);
				if (ScrollView.Content.TopAnchor != uIBorderAnchor4)
				{
					ScrollView.Content.TopAnchor = uIBorderAnchor4;
				}
				UIBorderAnchor uIBorderAnchor5 = new UIBorderAnchor(attach: true, 1f, ScrollView.BottomMargin, ScrollView.Content.BottomAnchor.Offset);
				if (ScrollView.Content.BottomAnchor != uIBorderAnchor5)
				{
					ScrollView.Content.BottomAnchor = uIBorderAnchor5;
				}
				return;
			}
			if (ScrollView.Content.TopAnchor.Attach)
			{
				UITransform content3 = ScrollView.Content;
				uIBorderAnchor3 = new UIBorderAnchor(ScrollView.Content.TopAnchor)
				{
					Attach = false
				};
				content3.TopAnchor = uIBorderAnchor3;
			}
			if (ScrollView.Content.BottomAnchor.Attach)
			{
				UITransform content4 = ScrollView.Content;
				uIBorderAnchor3 = new UIBorderAnchor(ScrollView.Content.BottomAnchor)
				{
					Attach = false
				};
				content4.BottomAnchor = uIBorderAnchor3;
			}
		}

		private void ConstrainContent()
		{
			if (ScrollView.ScrollHorizontally)
			{
				if (ScrollView.AutoAdjustWidth)
				{
					AdjustWidthIfPossible();
				}
				ConstrainContentHorizontally();
			}
			if (ScrollView.ScrollVertically)
			{
				if (ScrollView.AutoAdjustHeight)
				{
					AdjustHeightIfPossible();
				}
				ConstrainContentVertically();
			}
		}

		private void ConstrainContentHorizontally()
		{
			if (ScrollView.Content.Width <= ScrollView.Viewport.Width - ScrollView.LeftMargin - ScrollView.RightMargin)
			{
				ScrollView.ResetHorizontally();
			}
			else if (ScrollView.Content.Left > ScrollView.LeftMargin + 0.01f)
			{
				ScrollView.Content.Left = ScrollView.LeftMargin;
			}
			else if (ScrollView.Content.Right < ScrollView.Viewport.Width - ScrollView.RightMargin - 0.01f)
			{
				ScrollView.Content.Right = ScrollView.Viewport.Width - ScrollView.RightMargin;
			}
		}

		private void ConstrainContentVertically()
		{
			if (ScrollView.Content.Height <= ScrollView.Viewport.Height - ScrollView.TopMargin - ScrollView.BottomMargin)
			{
				ScrollView.ResetVertically();
			}
			else if (ScrollView.Content.Top > ScrollView.TopMargin + 0.01f)
			{
				ScrollView.Content.Top = ScrollView.TopMargin;
			}
			else if (ScrollView.Content.Bottom < ScrollView.Viewport.Height - ScrollView.BottomMargin - 0.01f)
			{
				ScrollView.Content.Bottom = ScrollView.Viewport.Height - ScrollView.BottomMargin;
			}
		}

		private void AdjustWidthIfPossible()
		{
			float num = ScrollView.Content.Width + ScrollView.LeftMargin + ScrollView.RightMargin;
			if (ScrollView.Viewport != ScrollView.GetUITransform())
			{
				num += ScrollView.GetUITransform().Width - ScrollView.Viewport.Width;
			}
			num = Mathf.Max(num, ScrollView.MinWidth);
			num = Mathf.Min(num, ScrollView.MaxWidth);
			ScrollView.GetUITransform().Width = num;
		}

		private void AdjustHeightIfPossible()
		{
			float num = ScrollView.Content.Height + ScrollView.TopMargin + ScrollView.BottomMargin;
			if (ScrollView.Viewport != ScrollView.GetUITransform())
			{
				num += ScrollView.GetUITransform().Height - ScrollView.Viewport.Height;
			}
			num = Mathf.Max(num, ScrollView.MinHeight);
			num = Mathf.Min(num, ScrollView.MaxHeight);
			ScrollView.GetUITransform().Height = num;
		}

		private void ApplyHorizontalScrolling(float increment)
		{
			ScrollView.Content.X += increment * 20f * ScrollView.HorizontalScrollingSpeed;
			ConstrainContent();
			UpdateScrollBars();
		}

		private void ApplyVerticalScrolling(float increment)
		{
			ScrollView.Content.Y += increment * 20f * ScrollView.VerticalScrollingSpeed;
			ConstrainContent();
			UpdateScrollBars();
		}

		private void UpdateScrollBars()
		{
			if (ScrollView.ScrollHorizontally && ScrollView.HorizontalScrollBar != null)
			{
				UpdateHorizontalScrollBar();
			}
			if (ScrollView.ScrollVertically && ScrollView.VerticalScrollBar != null)
			{
				UpdateVerticalScrollBar();
			}
		}

		private void UpdateHorizontalScrollBar()
		{
			if (ScrollView.HorizontalScrollBar == null)
			{
				return;
			}
			float num = ScrollView.Viewport.Width - ScrollView.LeftMargin - ScrollView.RightMargin;
			if (ScrollView.Content.Width < float.Epsilon || ScrollView.Content.Width <= num)
			{
				if (!ScrollView.HorizontalScrollBar.AutoHide)
				{
					ScrollView.HorizontalScrollBar.OnVisibleAreaChanged(0f, 1f);
					ScrollView.HorizontalScrollBar.GetUITransform().VisibleSelf = true;
				}
				else
				{
					UpdateHorizontalScrollBarVisibility(visible: false);
				}
			}
			else
			{
				float num2 = (ScrollView.LeftMargin - ScrollView.Content.Left) / Mathf.Max(1f, ScrollView.Content.Width);
				float visibleAreaPercentageEnd = num2 + num / Mathf.Max(1f, ScrollView.Content.Width);
				ScrollView.HorizontalScrollBar.OnVisibleAreaChanged(num2, visibleAreaPercentageEnd);
				UpdateHorizontalScrollBarVisibility(visible: true);
			}
		}

		private void UpdateVerticalScrollBar()
		{
			if (ScrollView.VerticalScrollBar == null || !ScrollView.VerticalScrollBar.Loaded)
			{
				return;
			}
			float num = ScrollView.Viewport.Height - ScrollView.TopMargin - ScrollView.BottomMargin;
			if (ScrollView.Content.Height < float.Epsilon || ScrollView.Content.Height <= num)
			{
				if (!ScrollView.VerticalScrollBar.AutoHide)
				{
					ScrollView.VerticalScrollBar.OnVisibleAreaChanged(0f, 1f);
					ScrollView.VerticalScrollBar.GetUITransform().VisibleSelf = true;
				}
				else
				{
					UpdateVerticalScrollBarVisibility(visible: false);
				}
			}
			else
			{
				float num2 = (ScrollView.TopMargin - ScrollView.Content.Top) / ScrollView.Content.Height;
				float visibleAreaPercentageEnd = num2 + num / ScrollView.Content.Height;
				ScrollView.VerticalScrollBar.OnVisibleAreaChanged(num2, visibleAreaPercentageEnd);
				UpdateVerticalScrollBarVisibility(visible: true);
			}
		}

		private void UpdateHorizontalScrollBarVisibility(bool visible)
		{
			if (ScrollView.HorizontalScrollBar == null)
			{
				return;
			}
			UITransform uITransform = ScrollView.HorizontalScrollBar.GetUITransform();
			if (uITransform.VisibleSelf == visible)
			{
				return;
			}
			uITransform.VisibleSelf = visible;
			if (!ScrollView.HorizontalScrollBar.PushContentAside)
			{
				return;
			}
			if (uITransform.Y < ScrollView.Content.Y + ScrollView.Content.Height / 2f)
			{
				if (visible)
				{
					ScrollView.TopMargin += uITransform.Height;
				}
				else
				{
					ScrollView.TopMargin -= uITransform.Height;
				}
			}
			else if (visible)
			{
				ScrollView.BottomMargin += uITransform.Height;
			}
			else
			{
				ScrollView.BottomMargin -= uITransform.Height;
			}
		}

		private void UpdateVerticalScrollBarVisibility(bool visible)
		{
			if (ScrollView.VerticalScrollBar == null || !ScrollView.VerticalScrollBar.Loaded)
			{
				return;
			}
			UITransform uITransform = ScrollView.VerticalScrollBar.GetUITransform();
			if (uITransform.VisibleSelf == visible)
			{
				return;
			}
			uITransform.VisibleSelf = visible;
			if (!ScrollView.VerticalScrollBar.PushContentAside)
			{
				return;
			}
			if (uITransform.X < ScrollView.Content.X + ScrollView.Content.Width / 2f)
			{
				if (visible)
				{
					ScrollView.LeftMargin += uITransform.Width;
				}
				else
				{
					ScrollView.LeftMargin -= uITransform.Width;
				}
			}
			else if (visible)
			{
				ScrollView.RightMargin += uITransform.Width;
			}
			else
			{
				ScrollView.RightMargin -= uITransform.Width;
			}
		}

		private void Content_PositionOrSizeChange(bool positionChanged, bool sizeChanged)
		{
			Refresh();
		}

		private void Viewport_PositionOrSizeChange(bool positionChanged, bool sizeChanged)
		{
			Refresh();
		}

		private void HorizontalScrollBar_ThumbPositionChange(IUIScrollBar scrollBar, float thumbLeftPercentage)
		{
			ScrollView.Content.X = ScrollView.LeftMargin - thumbLeftPercentage * ScrollView.Content.Width;
			ScrollView.TryTriggerAudioEvent(AudioEvent.Interactivity.ScrollViewThumbMove);
		}

		private void VerticalScrollBar_ThumbPositionChange(IUIScrollBar scrollBar, float thumbTopPercentage)
		{
			ScrollView.Content.Y = ScrollView.TopMargin - thumbTopPercentage * ScrollView.Content.Height;
			ScrollView.TryTriggerAudioEvent(AudioEvent.Interactivity.ScrollViewThumbMove);
		}

		private void HorizontalScrollBar_MouseScroll(IUIControl scrollBar, float increment)
		{
			if (!ScrollView.IgnoreMouseScrollEvent)
			{
				ApplyHorizontalScrolling(increment);
				ScrollView.TryTriggerAudioEvent(AudioEvent.Interactivity.ScrollViewThumbMove);
			}
		}

		private void VerticalScrollBar_MouseScroll(IUIControl scrollBar, float increment)
		{
			if (!ScrollView.IgnoreMouseScrollEvent)
			{
				ApplyVerticalScrolling(increment);
				ScrollView.TryTriggerAudioEvent(AudioEvent.Interactivity.ScrollViewThumbMove);
			}
		}

		private void HorizontalScrollBar_AxisUpdate2D(IUIControl scrollBar, Vector2 increment)
		{
			if (!ScrollView.IgnoreMouseScrollEvent)
			{
				ApplyHorizontalScrolling(increment.x);
				ScrollView.TryTriggerAudioEvent(AudioEvent.Interactivity.ScrollViewThumbMove);
			}
		}

		private void VerticalScrollBar_AxisUpdate2D(IUIControl scrollBar, Vector2 increment)
		{
			if (!ScrollView.IgnoreMouseScrollEvent)
			{
				ApplyVerticalScrolling(increment.y);
				ScrollView.TryTriggerAudioEvent(AudioEvent.Interactivity.ScrollViewThumbMove);
			}
		}

		private void TrackZone_MouseEnter(IUIControl trackZone, Vector2 mousePosition)
		{
			ScrollView.TryTriggerAudioEvent(AudioEvent.Interactivity.ScrollViewTrackZoneMouseEnter);
		}

		private void TrackZone_MouseLeave(IUIControl trackZone, Vector2 mousePosition)
		{
			ScrollView.TryTriggerAudioEvent(AudioEvent.Interactivity.ScrollViewTrackZoneMouseLeave);
		}

		private void Thumb_MouseEnter(IUIControl thumb, Vector2 mousePosition)
		{
			ScrollView.TryTriggerAudioEvent(AudioEvent.Interactivity.ScrollViewThumbMouseEnter);
		}

		private void Thumb_MouseLeave(IUIControl thumb, Vector2 mousePosition)
		{
			ScrollView.TryTriggerAudioEvent(AudioEvent.Interactivity.ScrollViewThumbMouseLeave);
		}
	}
}
