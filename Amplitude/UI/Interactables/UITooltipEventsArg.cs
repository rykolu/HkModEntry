using System;

namespace Amplitude.UI.Interactables
{
	public class UITooltipEventsArg : EventArgs
	{
		public enum Type
		{
			None,
			HoveredStart,
			HoveredStop,
			Update
		}

		public readonly Type EventType;

		public readonly UITooltip Tooltip;

		public UITooltipEventsArg(Type type, UITooltip tooltip = null)
		{
			EventType = type;
			Tooltip = tooltip;
		}
	}
}
