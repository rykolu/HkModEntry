using UnityEngine;

namespace Amplitude.UI
{
	public class UIMaterialIdAttribute : PropertyAttribute
	{
		public readonly UIPrimitiveType Type;

		public UIMaterialIdAttribute(UIPrimitiveType type)
		{
			Type = type;
		}
	}
}
