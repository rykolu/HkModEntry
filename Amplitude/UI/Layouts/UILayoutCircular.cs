using System;
using UnityEngine;

namespace Amplitude.UI.Layouts
{
	[RequireComponent(typeof(UITransform))]
	public class UILayoutCircular : UILayout
	{
		[SerializeField]
		private float originAngle;

		[SerializeField]
		private float spacing = 30f;

		[SerializeField]
		private float radius = 1f;

		[SerializeField]
		private bool centered;

		[SerializeField]
		private bool evenlySpaced;

		[SerializeField]
		private float totalAngle = 360f;

		[SerializeField]
		private bool lastPositionEmpty;

		[SerializeField]
		private bool orientateChildren;

		public float OriginAngle
		{
			get
			{
				return originAngle;
			}
			set
			{
				if (originAngle != value)
				{
					originAngle = value;
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

		public float Radius
		{
			get
			{
				return radius;
			}
			set
			{
				if (radius != value)
				{
					radius = value;
					ArrangeChildren();
				}
			}
		}

		public bool Centered
		{
			get
			{
				return centered;
			}
			set
			{
				if (centered != value)
				{
					centered = value;
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
					ArrangeChildren();
				}
			}
		}

		public float TotalAngle
		{
			get
			{
				return totalAngle;
			}
			set
			{
				if (totalAngle != value)
				{
					totalAngle = value;
					ArrangeChildren();
				}
			}
		}

		public bool LastPositionEmpty
		{
			get
			{
				return lastPositionEmpty;
			}
			set
			{
				if (lastPositionEmpty != value)
				{
					lastPositionEmpty = value;
					ArrangeChildren();
				}
			}
		}

		public bool OrientateChildren
		{
			get
			{
				return orientateChildren;
			}
			set
			{
				if (orientateChildren != value)
				{
					orientateChildren = value;
					ArrangeChildren();
				}
			}
		}

		protected override void DoArrangeChildren(ref PerformanceList<UITransform> sortedChildren)
		{
			base.DoArrangeChildren(ref sortedChildren);
			int visibleChildrenCount = UITransform.GetVisibleChildrenCount();
			if (evenlySpaced)
			{
				if (visibleChildrenCount > 1)
				{
					if (lastPositionEmpty)
					{
						spacing = totalAngle / (float)visibleChildrenCount;
					}
					else
					{
						spacing = totalAngle / (float)(visibleChildrenCount - 1);
					}
				}
			}
			else
			{
				totalAngle = spacing * (float)(visibleChildrenCount - 1);
			}
			float num = originAngle;
			if (centered)
			{
				float num2 = ((visibleChildrenCount > 1) ? (spacing * (float)(visibleChildrenCount - 1)) : 0f);
				num -= num2 / 2f;
			}
			int count = sortedChildren.Count;
			for (int i = 0; i < count; i++)
			{
				UITransform uITransform = sortedChildren.Data[i];
				if (uITransform.VisibleSelf)
				{
					SetPolarCoordinates(uITransform, num, radius);
					if (orientateChildren)
					{
						uITransform.Rotation = new Vector3(uITransform.Rotation.x, uITransform.Rotation.y, 90f - num);
					}
					num += spacing;
				}
			}
		}

		protected void SetPolarCoordinates(UITransform child, float angleInDegrees, float radiusInParentSizeRatio)
		{
			UITransform parent = child.Parent;
			if (!(parent == null))
			{
				float f = angleInDegrees * ((float)Math.PI / 180f);
				Rect globalRect = parent.GlobalRect;
				float num = globalRect.width * 0.5f * radiusInParentSizeRatio;
				float num2 = globalRect.height * 0.5f * radiusInParentSizeRatio;
				Vector2 vector = new Vector2(Mathf.Cos(f) * num, (0f - Mathf.Sin(f)) * num2) + new Vector2(child.Parent.Width * parent.Pivot.x, child.Parent.Height * parent.Pivot.y);
				child.Position = vector;
			}
		}

		protected override void UiTransform_PositionOrSizeChange(bool positionChanged, bool sizeChanged)
		{
			if (sizeChanged)
			{
				ArrangeChildren();
			}
		}

		protected override void UiTransform_PivotChange()
		{
			ArrangeChildren();
		}
	}
}
