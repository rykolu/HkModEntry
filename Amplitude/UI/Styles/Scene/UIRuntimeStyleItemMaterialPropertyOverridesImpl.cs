using Amplitude.UI.Renderers;
using Amplitude.UI.Styles.Data;

namespace Amplitude.UI.Styles.Scene
{
	internal abstract class UIRuntimeStyleItemMaterialPropertyOverridesImpl<TValue> : UIRuntimeStyleItem where TValue : struct
	{
		private readonly IInterpolator<TValue> interpolator;

		protected sealed override bool CanInterpolate => interpolator != null;

		protected UIRuntimeStyleItemMaterialPropertyOverridesImpl(StaticString identifier, IInterpolator<TValue> interpolator)
			: base(identifier)
		{
			this.interpolator = interpolator;
		}

		protected UIRuntimeStyleItemMaterialPropertyOverridesImpl(StaticString identifier)
			: base(identifier)
		{
			interpolator = null;
		}

		internal sealed override bool IsDataValid(UIStyleItem item)
		{
			return item is UIStyleItemImpl<TValue>;
		}

		protected sealed override void SetValue(IUIStyleTarget target, ref LocalState localState)
		{
			UIStyleItemImpl<TValue> uIStyleItemImpl = (UIStyleItemImpl<TValue>)localState.StyleItem;
			ref UIMaterialPropertyOverrides materialPropertyOverrides = ref ((IUIMaterialPropertyOverridesProvider)target).MaterialPropertyOverrides;
			if (localState.IsAnimationInProgress)
			{
				TValue value = uIStyleItemImpl.GetValue(localState.PreviousReactivityIndex);
				TValue value2 = uIStyleItemImpl.GetValue(localState.CurrentReactivityIndex);
				TValue result = default(TValue);
				interpolator.Interpolate(value, value2, localState.ComputeAnimationRatio(), ref result);
				SetValue(ref materialPropertyOverrides, result);
			}
			else
			{
				SetValue(ref materialPropertyOverrides, uIStyleItemImpl.GetValue(localState.CurrentReactivityIndex));
			}
		}

		protected abstract TValue GetValue(ref UIMaterialPropertyOverrides materialPropertyOverrides);

		protected abstract void SetValue(ref UIMaterialPropertyOverrides materialPropertyOverrides, TValue value);
	}
}
