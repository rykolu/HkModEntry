using Amplitude.UI.Renderers;
using UnityEngine;

namespace Amplitude.UI.Styles.Scene
{
	[UIRuntimeStyleItemMaterialPropertyOverrides(UIMaterialPropertyOverride.PropertyType.Color)]
	internal class UIRuntimeStyleItemMaterialPropertyOverridesColor : UIRuntimeStyleItemMaterialPropertyOverridesImpl<Color>
	{
		public UIRuntimeStyleItemMaterialPropertyOverridesColor(StaticString identifier)
			: base(identifier, (IInterpolator<Color>)Interpolators.ColorInterpolator)
		{
		}

		protected override Color GetValue(ref UIMaterialPropertyOverrides materialPropertyOverrides)
		{
			StaticString identifier = Identifier;
			return materialPropertyOverrides.GetColorValue(identifier.ToString());
		}

		protected override void SetValue(ref UIMaterialPropertyOverrides materialPropertyOverrides, Color value)
		{
			StaticString identifier = Identifier;
			materialPropertyOverrides.SetColorValue(identifier.ToString(), value);
		}
	}
}
