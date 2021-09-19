using System;
using UnityEngine;

namespace Amplitude.UI
{
	[Serializable]
	public struct MaterialPropertyFieldInfo
	{
		public string Name;

		public MaterialPropertyType Type;

		public Vector4 DefaultValue;

		public StaticString Identifier;

		private static MaterialPropertyFieldInfo[] availableMaterialProperties;

		public static MaterialPropertyFieldInfo[] AvailableMaterialProperties
		{
			get
			{
				if (availableMaterialProperties == null)
				{
					availableMaterialProperties = new MaterialPropertyFieldInfo[3]
					{
						new MaterialPropertyFieldInfo("Alpha", MaterialPropertyType.Percent, Vector4.one),
						new MaterialPropertyFieldInfo("Saturation", MaterialPropertyType.Percent, Vector4.one),
						new MaterialPropertyFieldInfo("Color", MaterialPropertyType.Color, Vector4.one)
					};
				}
				return availableMaterialProperties;
			}
		}

		public MaterialPropertyFieldInfo(string name, MaterialPropertyType type, Vector4 defaultValue)
		{
			Name = name;
			Type = type;
			DefaultValue = defaultValue;
			Identifier = new StaticString(Name);
		}
	}
}
