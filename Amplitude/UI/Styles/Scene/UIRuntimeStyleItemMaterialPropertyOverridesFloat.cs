using Amplitude.UI.Renderers;

namespace Amplitude.UI.Styles.Scene
{
	[UIRuntimeStyleItemMaterialPropertyOverrides(UIMaterialPropertyOverride.PropertyType.Float)]
	internal class UIRuntimeStyleItemMaterialPropertyOverridesFloat : UIRuntimeStyleItemMaterialPropertyOverridesImpl<float>
	{
		public UIRuntimeStyleItemMaterialPropertyOverridesFloat(StaticString identifier)
			: base(identifier, (IInterpolator<float>)Interpolators.FloatInterpolator)
		{
		}

		protected override float GetValue(ref UIMaterialPropertyOverrides materialPropertyOverrides)
		{
			StaticString identifier = Identifier;
			return materialPropertyOverrides.GetFloatValue(identifier.ToString());
		}

		protected override void SetValue(ref UIMaterialPropertyOverrides materialPropertyOverrides, float value)
		{
			StaticString identifier = Identifier;
			materialPropertyOverrides.SetFloatValue(identifier.ToString(), value);
		}
	}
}
