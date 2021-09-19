using UnityEngine;

namespace Amplitude.UI
{
	public class UIVector2Attribute : PropertyAttribute
	{
		public readonly string LabelA;

		public readonly string LabelB;

		public readonly float LabelAWidth;

		public readonly float LabelBWidth;

		public UIVector2Attribute(string labelA = "X", string labelB = "Y", float labelAWidth = 0f, float labelBWidth = 0f)
		{
			LabelA = labelA;
			LabelB = labelB;
			LabelAWidth = labelAWidth;
			LabelBWidth = labelBWidth;
		}
	}
}
