using UnityEngine;

namespace Amplitude.UI
{
	public struct RectColored
	{
		public Rect Rect;

		public Color32 Color;

		public RectColored(Rect rect, Color32 color)
		{
			Rect = rect;
			Color = color;
		}

		public override string ToString()
		{
			return Rect.ToString() + " " + Color.ToString();
		}
	}
}
