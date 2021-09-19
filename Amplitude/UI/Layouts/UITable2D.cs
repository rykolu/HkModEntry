using System;
using UnityEngine;

namespace Amplitude.UI.Layouts
{
	[RequireComponent(typeof(UITransform))]
	public class UITable2D : UILayout
	{
		[SerializeField]
		private Direction firstDirection = Direction.LeftToRight;

		[SerializeField]
		private Direction secondDirection = Direction.TopToBottom;

		[SerializeField]
		private bool useFixedLines;

		[SerializeField]
		private int fixedLinesCount;

		[SerializeField]
		private bool autoResize;

		[SerializeField]
		private float horizontalMargin;

		[SerializeField]
		private float horizontalSpacing;

		[SerializeField]
		[Range(0f, 1f)]
		private float horizontalOrigin;

		[SerializeField]
		private float verticalMargin;

		[SerializeField]
		private float verticalSpacing;

		[SerializeField]
		[Range(0f, 1f)]
		private float verticalOrigin;

		private Rect[] workingChildrenPosition;

		public Direction FirstDirection
		{
			get
			{
				return firstDirection;
			}
			set
			{
				if (firstDirection != value)
				{
					firstDirection = value;
					if (UILayout.IsDirectionHorizontal(firstDirection) && UILayout.IsDirectionHorizontal(secondDirection))
					{
						secondDirection = Direction.TopToBottom;
					}
					else if (UILayout.IsDirectionVertical(firstDirection) && UILayout.IsDirectionVertical(secondDirection))
					{
						secondDirection = Direction.LeftToRight;
					}
					ArrangeChildren();
				}
			}
		}

		public Direction SecondDirection
		{
			get
			{
				return secondDirection;
			}
			set
			{
				if (secondDirection != value && ((UILayout.IsDirectionHorizontal(firstDirection) && UILayout.IsDirectionVertical(value)) || (UILayout.IsDirectionVertical(firstDirection) && UILayout.IsDirectionHorizontal(value))))
				{
					secondDirection = value;
					ArrangeChildren();
				}
			}
		}

		public bool UseFixedLines
		{
			get
			{
				return useFixedLines;
			}
			set
			{
				if (useFixedLines != value)
				{
					useFixedLines = value;
					ArrangeChildren();
				}
			}
		}

		public int FixedLinesCount
		{
			get
			{
				return fixedLinesCount;
			}
			set
			{
				if (fixedLinesCount != value)
				{
					fixedLinesCount = value;
					ArrangeChildren();
				}
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
					ArrangeChildren();
				}
			}
		}

		public float HorizontalMargin
		{
			get
			{
				return horizontalMargin;
			}
			set
			{
				if (horizontalMargin != value)
				{
					horizontalMargin = value;
					ArrangeChildren();
				}
			}
		}

		public float HorizontalSpacing
		{
			get
			{
				return horizontalSpacing;
			}
			set
			{
				if (horizontalSpacing != value)
				{
					horizontalSpacing = value;
					ArrangeChildren();
				}
			}
		}

		public float HorizontalOrigin
		{
			get
			{
				return horizontalOrigin;
			}
			set
			{
				if (horizontalOrigin != value)
				{
					horizontalOrigin = value;
					ArrangeChildren();
				}
			}
		}

		public float VerticalMargin
		{
			get
			{
				return verticalMargin;
			}
			set
			{
				if (verticalMargin != value)
				{
					verticalMargin = value;
					ArrangeChildren();
				}
			}
		}

		public float VerticalSpacing
		{
			get
			{
				return verticalSpacing;
			}
			set
			{
				if (verticalSpacing != value)
				{
					verticalSpacing = value;
					ArrangeChildren();
				}
			}
		}

		public float VerticalOrigin
		{
			get
			{
				return verticalOrigin;
			}
			set
			{
				if (verticalOrigin != value)
				{
					verticalOrigin = value;
					ArrangeChildren();
				}
			}
		}

		protected override void DoArrangeChildren(ref PerformanceList<UITransform> sortedChildren)
		{
			base.DoArrangeChildren(ref sortedChildren);
			int count = sortedChildren.Count;
			if (count > 0)
			{
				int indexOfFirstItem = 0;
				int num = 0;
				int num2 = 0;
				float num3 = 0f;
				float num4 = 0f;
				float num5 = 0f;
				float num6 = 0f;
				ResetCursorAtStartOfAxis(firstDirection);
				ResetCursorAtStartOfAxis(secondDirection);
				bool flag = false;
				InitializeWorkingChildrenRect(ref sortedChildren);
				bool flag2 = UILayout.IsDirectionHorizontal(firstDirection);
				for (int i = 0; i < count; i++)
				{
					if (sortedChildren.Data[i].VisibleSelf)
					{
						ref Rect reference = ref workingChildrenPosition[i];
						bool flag3 = false;
						if ((!useFixedLines || fixedLinesCount <= 0) ? (flag && WillChildOverflow(firstDirection, ref reference)) : (num >= fixedLinesCount))
						{
							ApplyFirstDimensionOrigin(indexOfFirstItem, i - 1, num5, num);
							ResetCursorAtStartOfAxis(firstDirection);
							AdvanceCursorAlongAxis(secondDirection, num3, num4);
							num2++;
							num6 += (flag2 ? num4 : num3);
							num = 1;
							indexOfFirstItem = i;
							num3 = reference.width;
							num4 = reference.height;
							num5 = (flag2 ? reference.width : reference.height);
						}
						else
						{
							num++;
							num3 = Mathf.Max(reference.width, num3);
							num4 = Mathf.Max(reference.height, num4);
							num5 += (flag2 ? reference.width : reference.height);
						}
						PlaceChildAtCursor(firstDirection, secondDirection, ref reference);
						if (!flag)
						{
							flag = true;
						}
						AdvanceCursorAlongAxis(firstDirection, reference.width, reference.height);
					}
				}
				ApplyFirstDimensionOrigin(indexOfFirstItem, count - 1, num5, num);
				num2++;
				num6 += (flag2 ? num4 : num3);
				ApplySecondDimensionOrigin(num6, num2);
				FlushWorkingChildrenRect(ref sortedChildren);
			}
			if (autoResize)
			{
				AdjustSize();
			}
		}

		private void EnsureWorkingChildrenRect(int numberOfChildren)
		{
			Rect[] array = workingChildrenPosition;
			if (((array != null) ? array.Length : 0) < numberOfChildren)
			{
				Array.Resize(ref workingChildrenPosition, numberOfChildren * 2);
			}
		}

		private void InitializeWorkingChildrenRect(ref PerformanceList<UITransform> sortedChildren)
		{
			int count = sortedChildren.Count;
			EnsureWorkingChildrenRect(count);
			for (int i = 0; i < count; i++)
			{
				workingChildrenPosition[i] = sortedChildren.Data[i].Rect;
			}
		}

		private void FlushWorkingChildrenRect(ref PerformanceList<UITransform> sortedChildren)
		{
			int count = sortedChildren.Count;
			for (int i = 0; i < count; i++)
			{
				UITransform uITransform = sortedChildren.Data[i];
				if (uITransform.VisibleSelf)
				{
					uITransform.Rect = workingChildrenPosition[i];
				}
			}
		}

		private void ResetCursorAtStartOfAxis(Direction direction)
		{
			switch (direction)
			{
			case Direction.LeftToRight:
				cursor.x = horizontalMargin;
				break;
			case Direction.RightToLeft:
				cursor.x = UITransform.Width - horizontalMargin;
				break;
			case Direction.TopToBottom:
				cursor.y = verticalMargin;
				break;
			case Direction.BottomToTop:
				cursor.y = UITransform.Height - verticalMargin;
				break;
			}
		}

		private void AdvanceCursorAlongAxis(Direction direction, float deltaX, float deltaY)
		{
			switch (direction)
			{
			case Direction.LeftToRight:
				cursor.x += deltaX + horizontalSpacing;
				break;
			case Direction.RightToLeft:
				cursor.x -= deltaX + horizontalSpacing;
				break;
			case Direction.TopToBottom:
				cursor.y += deltaY + verticalSpacing;
				break;
			case Direction.BottomToTop:
				cursor.y -= deltaY + verticalSpacing;
				break;
			}
		}

		private void PlaceChildAtCursor(Direction firstDirection, Direction secondDirection, ref Rect childRect)
		{
			if (firstDirection == Direction.LeftToRight || secondDirection == Direction.LeftToRight)
			{
				childRect.x = cursor.x;
			}
			else if (firstDirection == Direction.RightToLeft || secondDirection == Direction.RightToLeft)
			{
				childRect.x = cursor.x - childRect.width;
			}
			if (firstDirection == Direction.TopToBottom || secondDirection == Direction.TopToBottom)
			{
				childRect.y = cursor.y;
			}
			else if (firstDirection == Direction.BottomToTop || secondDirection == Direction.BottomToTop)
			{
				childRect.y = cursor.y - childRect.height;
			}
		}

		private bool WillChildOverflow(Direction direction, ref Rect childRect)
		{
			return direction switch
			{
				Direction.LeftToRight => cursor.x + childRect.width + horizontalMargin > UITransform.Width, 
				Direction.RightToLeft => cursor.x - childRect.width - horizontalMargin < 0f, 
				Direction.TopToBottom => cursor.y + childRect.height + verticalMargin > UITransform.Height, 
				Direction.BottomToTop => cursor.y - childRect.height - verticalMargin < 0f, 
				_ => throw new ArgumentException(), 
			};
		}

		private void ApplyFirstDimensionOrigin(int indexOfFirstItem, int indexOfLastItem, float lineItemsTotalSize, int numberOfItems)
		{
			ApplyOrigin(indexOfFirstItem, indexOfLastItem, firstDirection, lineItemsTotalSize, numberOfItems);
		}

		private void ApplySecondDimensionOrigin(float totalLinesSize, int numberOfLines)
		{
			if (!autoResize)
			{
				ApplyOrigin(0, workingChildrenPosition.Length - 1, secondDirection, totalLinesSize, numberOfLines);
			}
		}

		private void ApplyOrigin(int firstIndex, int lastIndex, Direction direction, float totalSize, int numberOfItems)
		{
			bool flag = UILayout.IsDirectionHorizontal(direction);
			float num = (flag ? horizontalOrigin : verticalOrigin);
			if (Mathf.Approximately(num, 0f))
			{
				return;
			}
			float num2 = (flag ? (UITransform.Width - totalSize - 2f * horizontalMargin - (float)(numberOfItems - 1) * horizontalSpacing) : (UITransform.Height - totalSize - 2f * verticalMargin - (float)(numberOfItems - 1) * verticalSpacing)) * num;
			_ = workingChildrenPosition.Length;
			for (int i = firstIndex; i <= lastIndex; i++)
			{
				ref Rect reference = ref workingChildrenPosition[i];
				switch (direction)
				{
				case Direction.LeftToRight:
					reference.x += num2;
					break;
				case Direction.RightToLeft:
					reference.x -= num2;
					break;
				case Direction.TopToBottom:
					reference.y += num2;
					break;
				case Direction.BottomToTop:
					reference.y -= num2;
					break;
				}
			}
		}

		private void AdjustSize()
		{
			if (secondDirection == Direction.LeftToRight)
			{
				UITransform.AlignRightToRightmostVisibleChild(horizontalMargin);
			}
			else if (secondDirection == Direction.RightToLeft)
			{
				UITransform.AlignLeftToLeftmostVisibleChild(horizontalMargin);
			}
			else if (secondDirection == Direction.TopToBottom)
			{
				UITransform.AlignBottomToBottommostVisibleChild(verticalMargin);
			}
			else if (secondDirection == Direction.BottomToTop)
			{
				UITransform.AlignTopToTopmostVisibleChild(verticalMargin);
			}
		}
	}
}
