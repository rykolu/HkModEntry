using System;
using Amplitude.UI.Styles.Data;

namespace Amplitude.UI.Styles.Scene
{
	internal class UIRuntimeStyleItemImpl<TTarget, TValue> : UIRuntimeStyleItem where TTarget : UIComponent where TValue : struct
	{
		private readonly Func<TTarget, TValue> getValue;

		private readonly Action<TTarget, TValue> setValue;

		private readonly IInterpolator<TValue> interpolator;

		protected override bool CanInterpolate => interpolator != null;

		public UIRuntimeStyleItemImpl(StaticString identifier, Func<TTarget, TValue> getLambda, Action<TTarget, TValue> setLambda, IInterpolator<TValue> interpolator)
			: base(identifier)
		{
			setValue = setLambda;
			getValue = getLambda;
			this.interpolator = interpolator;
		}

		internal override bool IsDataValid(UIStyleItem item)
		{
			return item is UIStyleItemImpl<TValue>;
		}

		protected override void SetValue(IUIStyleTarget target, ref LocalState localState)
		{
			UIStyleItemImpl<TValue> uIStyleItemImpl = (UIStyleItemImpl<TValue>)localState.StyleItem;
			if (localState.IsAnimationInProgress)
			{
				TValue value = uIStyleItemImpl.GetValue(localState.PreviousReactivityIndex);
				TValue value2 = uIStyleItemImpl.GetValue(localState.CurrentReactivityIndex);
				TValue result = default(TValue);
				interpolator.Interpolate(value, value2, localState.ComputeAnimationRatio(), ref result);
				setValue((TTarget)target, result);
			}
			else
			{
				setValue((TTarget)target, uIStyleItemImpl.GetValue(localState.CurrentReactivityIndex));
			}
		}
	}
}
