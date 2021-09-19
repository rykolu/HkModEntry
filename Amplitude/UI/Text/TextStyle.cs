using System;

namespace Amplitude.UI.Text
{
	[Flags]
	public enum TextStyle : byte
	{
		None = 0x0,
		Underline = 0x1,
		StrikeThrough = 0x2,
		Highlight = 0x4
	}
}
