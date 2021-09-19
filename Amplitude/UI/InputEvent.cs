using System;
using Amplitude.Framework.Input;
using UnityEngine;

namespace Amplitude.UI
{
	public struct InputEvent
	{
		[Flags]
		public enum EventType
		{
			MouseHoverTooltip = 0x1,
			Tick = 0x2,
			MouseHover = 0x4,
			MouseDown = 0x8,
			MouseUp = 0x10,
			MouseScroll = 0x20,
			CastFocus = 0x40,
			KeyDown = 0x80,
			KeyUp = 0x100,
			NewChar = 0x200,
			AxisUpdate2D = 0x400,
			AllButTooltip = 0xFFFE,
			AllButKeyboard = 0x47F,
			All = 0xFFFFF,
			AllKeys = 0x380
		}

		public readonly EventType Type;

		public readonly MouseButton Button;

		public readonly float ScrollIncrement;

		public readonly Vector2 AxisUpdate2D;

		public readonly int Key;

		public readonly int CustomData;

		public readonly ulong UniqueId;

		public bool Catched;

		private static ulong nextId = 1uL;

		private Vector2 mousePosition;

		public Vector2 MousePosition
		{
			get
			{
				return mousePosition;
			}
			internal set
			{
				mousePosition = value;
			}
		}

		public bool IsKeyboardEvent => (Type & EventType.AllKeys) != 0;

		public InputEvent(EventType type, int customData = 0, int lineageId = 0)
		{
			this = default(InputEvent);
			Type = type;
			CustomData = customData;
			UniqueId = nextId++;
		}

		public InputEvent(EventType type, Vector2 mousePosition, int customData)
		{
			this = default(InputEvent);
			Type = type;
			this.mousePosition = mousePosition;
			CustomData = customData;
			UniqueId = nextId++;
		}

		public InputEvent(EventType type, MouseButton mouseButton, int customData)
		{
			this = default(InputEvent);
			Type = type;
			Button = mouseButton;
			CustomData = customData;
			UniqueId = nextId++;
		}

		public InputEvent(EventType type, float scrollIncrement, int customData)
		{
			this = default(InputEvent);
			Type = type;
			ScrollIncrement = scrollIncrement;
			CustomData = customData;
			UniqueId = nextId++;
		}

		public InputEvent(EventType type, float axisUpdateX, float axisUpdateY, int customData)
		{
			this = default(InputEvent);
			Type = type;
			AxisUpdate2D = new Vector2(axisUpdateX, axisUpdateY);
			CustomData = customData;
			UniqueId = nextId++;
		}

		public InputEvent(EventType type, KeyCode keyCode, int customData)
		{
			this = default(InputEvent);
			Type = type;
			Key = (int)keyCode;
			CustomData = customData;
			UniqueId = nextId++;
		}

		public InputEvent(EventType type, char inputChar, int customData)
		{
			this = default(InputEvent);
			Type = type;
			Key = inputChar;
			CustomData = customData;
			UniqueId = nextId++;
		}

		public override string ToString()
		{
			EventType type = Type;
			return type.ToString();
		}
	}
}
