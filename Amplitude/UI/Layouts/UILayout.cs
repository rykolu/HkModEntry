using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Amplitude.UI.Layouts
{
	[RequireComponent(typeof(UITransform))]
	[ExecuteInEditMode]
	public abstract class UILayout : UIComponent
	{
		[NonSerialized]
		protected Vector2 cursor = Vector2.zero;

		private static List<List<UITransform>> freeTmpChildrenList = new List<List<UITransform>>();

		[NonSerialized]
		private int nestedCallsToArrangeChildren;

		[NonSerialized]
		private bool isCurrentlyArrangingChildren;

		[NonSerialized]
		private IComparer<UITransform> comparer;

		[NonSerialized]
		private string arrangeChildrenProfileLabel;

		public override string ToString()
		{
			if (UITransform != null)
			{
				return $"{UITransform}|{GetType().Name}";
			}
			return $"???|{GetType().Name}";
		}

		public void ArrangeChildren()
		{
			if (UITransform == null || !UITransform.SortingRange.IsValid || !UITransform.VisibleGlobally || !base.gameObject.activeInHierarchy)
			{
				return;
			}
			isCurrentlyArrangingChildren = true;
			nestedCallsToArrangeChildren++;
			if (nestedCallsToArrangeChildren <= 3)
			{
				if (comparer != null)
				{
					SortChildren();
				}
				DoArrangeChildren(ref UITransform.Children);
			}
			else
			{
				Diagnostics.LogError("Detected infinite loop while calling ArrangeChildren on '{0}'", this);
			}
			nestedCallsToArrangeChildren--;
			isCurrentlyArrangingChildren = false;
		}

		public void SetComparer(IComparer<UITransform> comparer)
		{
			if (this.comparer != comparer)
			{
				this.comparer = comparer;
				if (base.Loaded && this.comparer != null)
				{
					ArrangeChildren();
				}
			}
		}

		internal static bool IsDirectionHorizontal(Direction direction)
		{
			if (direction != Direction.LeftToRight)
			{
				return direction == Direction.RightToLeft;
			}
			return true;
		}

		internal static bool IsDirectionVertical(Direction direction)
		{
			if (direction != Direction.TopToBottom)
			{
				return direction == Direction.BottomToTop;
			}
			return true;
		}

		protected override void Load()
		{
			nestedCallsToArrangeChildren = 0;
			base.Load();
			arrangeChildrenProfileLabel = "UI|Updates|Layout|ArrangeChildren|" + GetType().ToString();
			UITransform.ChildrenChange += UiTransform_ChildrenChange;
			UITransform.VisibleGloballyChange += UiTransform_VisibleGloballyChange;
			UITransform.PositionOrSizeChange += UiTransform_PositionOrSizeChange;
			UITransform.PivotChange += UiTransform_PivotChange;
			int count = UITransform.Children.Count;
			for (int i = 0; i < count; i++)
			{
				UITransform.Children.Data[i].VisibleGloballyChange += Child_VisibleGloballyChange;
				UITransform.Children.Data[i].PositionOrSizeChange += Child_PositionOrSizeChange;
			}
			UiTransform_VisibleGloballyChange(UITransform.VisibleGlobally);
		}

		protected override void Unload()
		{
			comparer = null;
			int count = UITransform.Children.Count;
			for (int i = 0; i < count; i++)
			{
				UITransform.Children.Data[i].VisibleGloballyChange -= Child_VisibleGloballyChange;
				UITransform.Children.Data[i].PositionOrSizeChange -= Child_PositionOrSizeChange;
			}
			if (UITransform != null)
			{
				UiTransform_VisibleGloballyChange(visibleGlobally: false);
				UITransform.ChildrenChange -= UiTransform_ChildrenChange;
				UITransform.VisibleGloballyChange -= UiTransform_VisibleGloballyChange;
				UITransform.PositionOrSizeChange -= UiTransform_PositionOrSizeChange;
				UITransform.PivotChange -= UiTransform_PivotChange;
			}
			base.Unload();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (base.Loaded)
			{
				ArrangeChildren();
			}
		}

		protected virtual void DoArrangeChildren(ref PerformanceList<UITransform> sortedChildren)
		{
		}

		protected virtual void UiTransform_ChildrenChange(UITransform child, CollectionChangeAction action)
		{
			switch (action)
			{
			case CollectionChangeAction.Add:
				child.VisibleGloballyChange += Child_VisibleGloballyChange;
				child.PositionOrSizeChange += Child_PositionOrSizeChange;
				Child_VisibleGloballyChange(child.VisibleGlobally);
				break;
			case CollectionChangeAction.Remove:
				child.VisibleGloballyChange -= Child_VisibleGloballyChange;
				child.PositionOrSizeChange -= Child_PositionOrSizeChange;
				break;
			default:
				if (!isCurrentlyArrangingChildren)
				{
					ArrangeChildren();
				}
				break;
			}
		}

		protected virtual void UiTransform_VisibleGloballyChange(bool visibleGlobally)
		{
			if (visibleGlobally)
			{
				ArrangeChildren();
			}
		}

		protected virtual void UiTransform_PositionOrSizeChange(bool positionChanged, bool sizeChanged)
		{
			if (sizeChanged)
			{
				ArrangeChildren();
			}
		}

		protected virtual void UiTransform_PivotChange()
		{
		}

		private void SortChildren()
		{
			if (freeTmpChildrenList == null)
			{
				freeTmpChildrenList = new List<List<UITransform>>();
			}
			List<UITransform> list = null;
			if (freeTmpChildrenList.Count == 0)
			{
				list = new List<UITransform>();
			}
			else
			{
				int index = freeTmpChildrenList.Count - 1;
				list = freeTmpChildrenList[index];
				freeTmpChildrenList.RemoveAt(index);
			}
			ref PerformanceList<UITransform> children = ref UITransform.Children;
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				list.Add(children.Data[i]);
			}
			list.Sort(comparer);
			for (int j = 0; j < count; j++)
			{
				list[j].transform.SetSiblingIndex(j);
			}
			list.Clear();
			freeTmpChildrenList.Add(list);
		}

		private void Child_VisibleGloballyChange(bool visibleGlobally)
		{
			if (!isCurrentlyArrangingChildren)
			{
				ArrangeChildren();
			}
		}

		private void Child_PositionOrSizeChange(bool positionChanged, bool sizeChanged)
		{
			if (!isCurrentlyArrangingChildren)
			{
				ArrangeChildren();
			}
		}
	}
}
