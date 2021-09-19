using Amplitude.UI.Patterns.Bricks;

namespace Amplitude.UI.Tooltips
{
	public class UITooltipBrickDefinition : BrickDefinition
	{
		public override string ToString()
		{
			if (Prefab != null)
			{
				UITooltipBrick component = Prefab.GetComponent<UITooltipBrick>();
				if (component != null)
				{
					string text = component.GetType().ToString();
					return text.Substring(text.LastIndexOf('.') + 1);
				}
				return "<Invalid>";
			}
			return base.ToString();
		}
	}
}
