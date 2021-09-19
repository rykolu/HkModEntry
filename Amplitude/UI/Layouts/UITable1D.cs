using System;
using UnityEngine;

namespace Amplitude.UI.Layouts
{
	[RequireComponent(typeof(UITransform))]
	public class UITable1D : UILayout
	{
		[SerializeField]
		private Direction direction = Direction.LeftToRight;

		[SerializeField]
		private float margin;

		[SerializeField]
		private float spacing;

		[SerializeField]
		private float origin;

		[SerializeField]
		private bool autoResize;

		[SerializeField]
		private bool evenlySpaced;

		public Direction Direction
		{
			get
			{
				return direction;
			}
			set
			{
				if (direction != value)
				{
					direction = value;
					ArrangeChildren();
				}
			}
		}

		public Orientation Orientation
		{
			get
			{
				if (direction == Direction.LeftToRight || direction == Direction.RightToLeft)
				{
					return Orientation.Horizontal;
				}
				return Orientation.Vertical;
			}
		}

		public bool AutoResize
		{
			get
			{
				return autoResize;
			}
			set
			{
				if (autoResize != value)
				{
					autoResize = value;
					if (autoResize)
					{
						evenlySpaced = false;
					}
					ArrangeChildren();
				}
			}
		}

		public bool EvenlySpaced
		{
			get
			{
				return evenlySpaced;
			}
			set
			{
				if (evenlySpaced != value)
				{
					evenlySpaced = value;
					if (evenlySpaced)
					{
						autoResize = false;
					}
					ArrangeChildren();
				}
			}
		}

		public float Margin
		{
			get
			{
				return margin;
			}
			set
			{
				if (margin != value)
				{
					margin = value;
					ArrangeChildren();
				}
			}
		}

		public float Spacing
		{
			get
			{
				return spacing;
			}
			set
			{
				if (spacing != value)
				{
					spacing = value;
					ArrangeChildren();
				}
			}
		}

		public float Origin
		{
			get
			{
				return origin;
			}
			set
			{
				if (origin != value)
				{
					origin = value;
					ArrangeChildren();
				}
			}
		}

		private bool IsHorizontal
		{
			get
			{
				if (direction != Direction.LeftToRight)
				{
					return direction == Direction.RightToLeft;
				}
				return true;
			}
		}

		protected override void DoArrangeChildren(ref PerformanceList<UITransform> sortedChildren)
		{
			base.DoArrangeChildren(ref sortedChildren);
			PerformanceList<UITransform> resizableChildren = default(PerformanceList<UITransform>);
			FindChildrenToResizeAndFreeSpace(ref resizableChildren, out var totalChildResizeWeights, out var freeSpace, out var visibleChildrenCount);
			if (totalChildResizeWeights > 0)
			{
				ResizeChildren(ref resizableChildren, totalChildResizeWeights, freeSpace);
			}
			else if (evenlySpaced && visibleChildrenCount > 1)
			{
				float num = freeSpace + spacing * (float)(visibleChildrenCount - 1);
				spacing = num / (float)(visibleChildrenCount - 1);
				freeSpace = 0f;
			}
			ResetCursor(freeSpace);
			int count = sortedChildren.Count;
			for (int i = 0; i < count; i++)
			{
				UITransform uITransform = sortedChildren.Data[i];
				if (uITransform.VisibleSelf)
				{
					PlaceChildAtCursor(direction, uITransform);
					AdvanceCursor(uITransform.Width, uITransform.Height);
				}
			}
			if (autoResize)
			{
				if (resizableChildren.Count > 0)
				{
					Diagnostics.LogWarning("The UITable1D '{0}' is auto-resizable but have resizable children so the auto-resize will be ignored.", UITransform);
				}
				else
				{
					AdjustSize(freeSpace);
				}
			}
		}

		private void FindChildrenToResizeAndFreeSpace(ref PerformanceList<UITransform> resizableChildren, out int totalChildResizeWeights, out float freeSpace, out int visibleChildrenCount)
		{
			totalChildResizeWeights = 0;
			visibleChildrenCount = 0;
			freeSpace = (IsHorizontal ? UITransform.Width : UITransform.Height);
			freeSpace -= margin * 2f;
			bool flag = true;
			ref PerformanceList<UITransform> children = ref UITransform.Children;
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				UITransform uITransform = children.Data[i];
				if (!uITransform.VisibleSelf)
				{
					continue;
				}
				if (IsHorizontal)
				{
					if (uITransform.LeftAnchor.Attach || uITransform.RightAnchor.Attach || uITransform.PivotXAnchor.Attach)
					{
						Diagnostics.LogWarning("The horizontal UITable1D {0} has its child {1} anchored horizontally", UITransform, uITransform);
					}
				}
				else if (uITransform.TopAnchor.Attach || uITransform.BottomAnchor.Attach || uITransform.PivotYAnchor.Attach)
				{
					Diagnostics.LogWarning("The vertical UITable1D {0} has its child {1} anchored vertically", UITransform, uITransform);
				}
				visibleChildrenCount++;
				if (!flag)
				{
					freeSpace -= spacing;
				}
				else
				{
					flag = false;
				}
				if (uITransform.ResizeWeight > 0)
				{
					totalChildResizeWeights += uITransform.ResizeWeight;
					resizableChildren.Add(uITransform);
				}
				else
				{
					freeSpace -= (IsHorizontal ? uITransform.Width : uITransform.Height);
				}
			}
		}

		private void ResizeChildren(ref PerformanceList<UITransform> resizableChildren, int totalChildResizeWeights, float availableSpace)
		{
			if (totalChildResizeWeights <= 0)
			{
				return;
			}
			if (availableSpace <= 0f)
			{
				Diagnostics.LogWarning("The UITable '{0}' have resizable children but not enough space left for giving them a positive size.", UITransform);
				return;
			}
			float num = availableSpace / (float)totalChildResizeWeights;
			for (int i = 0; i < resizableChildren.Count; i++)
			{
				float num2 = Mathf.Floor((float)resizableChildren.Data[i].ResizeWeight * num);
				if (IsHorizontal)
				{
					resizableChildren.Data[i].Width = num2;
					availableSpace -= resizableChildren.Data[i].Width;
				}
				else
				{
					resizableChildren.Data[i].Height = num2;
					availableSpace -= resizableChildren.Data[i].Height;
				}
			}
			if (availableSpace >= 1f)
			{
				if (IsHorizontal)
				{
					resizableChildren.Data[0].Width += Mathf.Floor(availableSpace);
				}
				else
				{
					resizableChildren.Data[0].Height += Mathf.Floor(availableSpace);
				}
			}
		}

		private void ResetCursor(float freeSpace)
		{
			if (direction == Direction.LeftToRight)
			{
				cursor.x = margin + freeSpace * origin;
				cursor.y = 0f;
			}
			else if (direction == Direction.RightToLeft)
			{
				cursor.x = UITransform.Width - margin - freeSpace * (1f - origin);
				cursor.y = 0f;
			}
			else if (direction == Direction.TopToBottom)
			{
				cursor.x = 0f;
				cursor.y = margin + freeSpace * origin;
			}
			else if (direction == Direction.BottomToTop)
			{
				cursor.x = 0f;
				cursor.y = UITransform.Height - margin - freeSpace * (1f - origin);
			}
		}

		private void AdvanceCursor(float deltaX, float deltaY)
		{
			if (direction == Direction.LeftToRight)
			{
				cursor.x += deltaX + spacing;
			}
			else if (direction == Direction.RightToLeft)
			{
				cursor.x -= deltaX + spacing;
			}
			else if (direction == Direction.TopToBottom)
			{
				cursor.y += deltaY + spacing;
			}
			else if (direction == Direction.BottomToTop)
			{
				cursor.y -= deltaY + spacing;
			}
		}

		private void PlaceChildAtCursor(Direction direction, UITransform child)
		{
			Rect rect = new Rect(child.Rect);
			switch (direction)
			{
			case Direction.LeftToRight:
				rect.x = cursor.x;
				break;
			case Direction.RightToLeft:
				rect.x = cursor.x - rect.width;
				break;
			}
			switch (direction)
			{
			case Direction.TopToBottom:
				rect.y = cursor.y;
				break;
			case Direction.BottomToTop:
				rect.y = cursor.y - rect.height;
				break;
			}
			child.Rect = rect;
		}

		private bool WillChildOverflow(Direction direction, UITransform child)
		{
			return direction switch
			{
				Direction.LeftToRight => cursor.x + child.Width + margin > UITransform.Width, 
				Direction.RightToLeft => cursor.x - child.Width - margin < 0f, 
				Direction.TopToBottom => cursor.y + child.Height + margin > UITransform.Height, 
				Direction.BottomToTop => cursor.y - child.Height - margin < 0f, 
				_ => throw new ArgumentException(), 
			};
		}

		private void AdjustSize(float freeSpace)
		{
			if (!(Mathf.Abs(freeSpace) < 0.1f))
			{
				if (IsHorizontal)
				{
					UITransform.Width -= freeSpace;
				}
				else
				{
					UITransform.Height -= freeSpace;
				}
			}
		}
	}
}
