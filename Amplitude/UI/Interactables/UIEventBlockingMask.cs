using System;

namespace Amplitude.UI.Interactables
{
	[Flags]
	public enum UIEventBlockingMask
	{
		None = 0x0,
		MouseOnly = 0x8FF,
		KeyboardOnly = 0x700,
		All = 0xFFF,
		MouseHover = 0x1,
		LeftMouseDown = 0x2,
		LeftMouseUp = 0x4,
		MiddleMouseDown = 0x8,
		MiddleMouseUp = 0x10,
		RightMouseDown = 0x20,
		RightMouseUp = 0x40,
		MouseScroll = 0x80,
		KeyDown = 0x100,
		KeyUp = 0x200,
		NewChar = 0x400,
		AxisUpdate2D = 0x800
	}
}
