using System;

namespace Amplitude.UI.Interactables
{
	public class UITooltipResponder : UIResponder
	{
		private UITooltip Tooltip => base.Interactable as UITooltip;

		public event Action<UITooltip, bool> Hover;

		public UITooltipResponder(UITooltip tooltip)
			: base(tooltip)
		{
			eventSensitivity = InputEvent.EventType.MouseHoverTooltip;
		}

		public override bool TryCatchEvent(ref InputEvent inputEvent)
		{
			if (inputEvent.Type != InputEvent.EventType.MouseHoverTooltip)
			{
				return false;
			}
			if (Contains(inputEvent.MousePosition) && UIHierarchyManager.Instance.MainFullscreenView.RenderedRect.Contains(inputEvent.MousePosition) && !inputEvent.Catched)
			{
				if (!hovered)
				{
					hovered = true;
					this.Hover?.Invoke(Tooltip, arg2: true);
					UITooltipManager.Instance?.OnTooltipHovered(Tooltip, hovered: true);
				}
				return true;
			}
			if (hovered)
			{
				hovered = false;
				this.Hover?.Invoke(Tooltip, arg2: false);
				UITooltipManager.Instance?.OnTooltipHovered(Tooltip, hovered: false);
			}
			return false;
		}

		public void OnTooltipModified()
		{
			if (hovered)
			{
				UITooltipManager.Instance?.OnTooltipModified(Tooltip);
			}
		}
	}
}
