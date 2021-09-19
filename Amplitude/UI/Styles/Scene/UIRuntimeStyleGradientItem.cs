using System;
using Amplitude.UI.Styles.Data;

namespace Amplitude.UI.Styles.Scene
{
	internal class UIRuntimeStyleGradientItem<TTarget> : UIRuntimeStyleItem where TTarget : UIComponent
	{
		private readonly Func<TTarget, UIGradient> getValue;

		private readonly Action<TTarget, UIGradient> setValue;

		protected override bool CanInterpolate => true;

		public UIRuntimeStyleGradientItem(StaticString identifier, Func<TTarget, UIGradient> getLambda, Action<TTarget, UIGradient> setLambda, IInterpolator<UIGradient> interpolator)
			: base(identifier)
		{
			setValue = setLambda;
			getValue = getLambda;
		}

		internal override bool IsDataValid(UIStyleItem item)
		{
			return item is UIStyleItemImpl<UIGradient>;
		}

		protected override void SetValue(IUIStyleTarget target, ref LocalState localState)
		{
			UIStyleItemImpl<UIGradient> uIStyleItemImpl = (UIStyleItemImpl<UIGradient>)localState.StyleItem;
			if (localState.IsAnimationInProgress)
			{
				UIGradient value = uIStyleItemImpl.GetValue(localState.PreviousReactivityIndex);
				UIGradient value2 = uIStyleItemImpl.GetValue(localState.CurrentReactivityIndex);
				Interpolators.GradientInterpolator.Interpolate(value, value2, localState.ComputeAnimationRatio(), ref localState.GradientCache);
				setValue((TTarget)target, localState.GradientCache);
			}
			else
			{
				setValue((TTarget)target, uIStyleItemImpl.GetValue(localState.CurrentReactivityIndex));
			}
		}
	}
}
