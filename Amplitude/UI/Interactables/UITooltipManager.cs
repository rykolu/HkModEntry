using System;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UIHierarchyManager))]
	public class UITooltipManager : UIBehaviour
	{
		private static UITooltipManager instance;

		[SerializeField]
		private UIView tooltipView;

		[SerializeField]
		[HideInInspector]
		private UIView[] viewPerGroupIndex;

		[SerializeField]
		private bool showTooltips = true;

		private UITooltip currentlyHoveredTooltip;

		public static UITooltipManager Instance => instance;

		public UIView TooltipView
		{
			get
			{
				if (!(tooltipView != null))
				{
					return UIHierarchyManager.Instance.MainFullscreenView;
				}
				return tooltipView;
			}
		}

		public bool ShowTooltips => showTooltips;

		internal UITooltip CurrentlyHoveredTooltip => currentlyHoveredTooltip;

		public event EventHandler<UITooltipEventsArg> TooltipUpdate;

		protected UITooltipManager()
		{
			instance = this;
		}

		public UIView GetViewForGroupIndex(int groupIndex)
		{
			if (groupIndex >= viewPerGroupIndex.Length)
			{
				return null;
			}
			return viewPerGroupIndex[groupIndex];
		}

		internal void OnTooltipHovered(UITooltip uiTooltip, bool hovered)
		{
			if (hovered)
			{
				if (currentlyHoveredTooltip != null)
				{
					this.TooltipUpdate?.Invoke(this, new UITooltipEventsArg(UITooltipEventsArg.Type.HoveredStop));
					currentlyHoveredTooltip = null;
				}
				currentlyHoveredTooltip = uiTooltip;
				this.TooltipUpdate?.Invoke(this, new UITooltipEventsArg(UITooltipEventsArg.Type.HoveredStart, uiTooltip));
			}
			else if (currentlyHoveredTooltip == uiTooltip)
			{
				currentlyHoveredTooltip = null;
				this.TooltipUpdate?.Invoke(this, new UITooltipEventsArg(UITooltipEventsArg.Type.HoveredStop));
			}
		}

		internal void OnTooltipModified(UITooltip uiTooltip)
		{
			if (currentlyHoveredTooltip == uiTooltip)
			{
				this.TooltipUpdate?.Invoke(this, new UITooltipEventsArg(UITooltipEventsArg.Type.Update, currentlyHoveredTooltip));
			}
		}

		internal void SpecificUpdate()
		{
			if (currentlyHoveredTooltip != null && !currentlyHoveredTooltip.UITransform.VisibleGlobally)
			{
				currentlyHoveredTooltip = null;
				this.TooltipUpdate?.Invoke(this, new UITooltipEventsArg(UITooltipEventsArg.Type.HoveredStop));
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (!(UIHierarchyManager.Instance != null))
			{
				return;
			}
			int num = UIHierarchyManager.Instance.GroupNames.Length;
			if (viewPerGroupIndex == null || viewPerGroupIndex.Length != num)
			{
				UIView[] array = null;
				Array.Resize(ref array, num);
				if (viewPerGroupIndex != null)
				{
					Array.Copy(viewPerGroupIndex, array, Mathf.Min(num, viewPerGroupIndex.Length));
				}
				viewPerGroupIndex = array;
			}
		}

		protected override void Destruct()
		{
			instance = null;
			base.Destruct();
		}
	}
}
