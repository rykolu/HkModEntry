using System;
using Amplitude.UI.Renderers;

namespace Amplitude.UI.Styles.Scene
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class UIRuntimeStyleItemMaterialPropertyOverridesAttribute : Attribute
	{
		public UIMaterialPropertyOverride.PropertyType PropertyType { get; private set; }

		public UIRuntimeStyleItemMaterialPropertyOverridesAttribute(UIMaterialPropertyOverride.PropertyType propertyType)
		{
			PropertyType = propertyType;
		}
	}
}
