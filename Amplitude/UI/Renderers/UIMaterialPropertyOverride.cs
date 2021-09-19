using System;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[Serializable]
	public struct UIMaterialPropertyOverride : IComparable<UIMaterialPropertyOverride>
	{
		public enum PropertyType
		{
			Float = 0,
			Vector = 1,
			Color = 3
		}

		[SerializeField]
		public string Name;

		[SerializeField]
		public PropertyType Type;

		[SerializeField]
		public Vector4 Vector;

		public int CompareTo(UIMaterialPropertyOverride other)
		{
			return Name.CompareTo(other.Name);
		}
	}
}
