using System;

namespace Amplitude.UI
{
	[Serializable]
	public enum DirectionWithCustom
	{
		LeftToRight = 1,
		RightToLeft = 2,
		TopToBottom = 4,
		BottomToTop = 8,
		Custom = 0x10
	}
}
