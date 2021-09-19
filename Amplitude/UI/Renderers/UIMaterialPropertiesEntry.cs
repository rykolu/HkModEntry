using System;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[Serializable]
	public struct UIMaterialPropertiesEntry : IComparable<UIMaterialPropertiesEntry>
	{
		public string Name;

		public Vector4 Data;

		public MaterialPropertyBlendMode BlendMode;

		private StaticString identifier;

		public StaticString Identifier
		{
			get
			{
				if (identifier == StaticString.Empty)
				{
					identifier = new StaticString(Name);
				}
				return identifier;
			}
		}

		public UIMaterialPropertiesEntry(string name, Vector4 data, MaterialPropertyBlendMode blendMode)
		{
			Name = name;
			Data = data;
			BlendMode = blendMode;
			identifier = new StaticString(Name);
		}

		public int CompareTo(UIMaterialPropertiesEntry other)
		{
			return Identifier.CompareTo(other.Identifier);
		}
	}
}
