using Amplitude.UI.Renderers;
using UnityEngine;

namespace Amplitude.UI.Styles.Scene
{
	[UIRuntimeStyleItemMaterialPropertyOverrides(UIMaterialPropertyOverride.PropertyType.Vector)]
	internal class UIRuntimeStyleItemMaterialPropertyOverridesVector3 : UIRuntimeStyleItemMaterialPropertyOverridesImpl<Vector3>
	{
		public UIRuntimeStyleItemMaterialPropertyOverridesVector3(StaticString identifier)
			: base(identifier, (IInterpolator<Vector3>)Interpolators.Vector3Interpolator)
		{
		}

		protected override Vector3 GetValue(ref UIMaterialPropertyOverrides materialPropertyOverrides)
		{
			StaticString identifier = Identifier;
			return materialPropertyOverrides.GetVectorValue(identifier.ToString());
		}

		protected override void SetValue(ref UIMaterialPropertyOverrides materialPropertyOverrides, Vector3 value)
		{
			StaticString identifier = Identifier;
			materialPropertyOverrides.SetVectorValue(identifier.ToString(), value);
		}
	}
}
