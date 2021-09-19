using System;
using Amplitude.Graphics.Text;
using Amplitude.UI.Styles.Data;

namespace Amplitude.UI.Styles.Scene
{
	internal class UIRuntimeStyleFontFamilyItem<TTarget> : UIRuntimeStyleItem where TTarget : UIComponent
	{
		private readonly Func<TTarget, FontFamily> getValue;

		private readonly Action<TTarget, FontFamily> setValue;

		protected override bool CanInterpolate => false;

		public UIRuntimeStyleFontFamilyItem(StaticString identifier, Func<TTarget, FontFamily> getLambda, Action<TTarget, FontFamily> setLambda)
			: base(identifier)
		{
			setValue = setLambda;
			getValue = getLambda;
		}

		internal override bool IsDataValid(UIStyleItem item)
		{
			return item is UIStyleItemImpl<FontFamily>;
		}

		protected override void SetValue(IUIStyleTarget target, ref LocalState localState)
		{
			UIStyleItemImpl<FontFamily> uIStyleItemImpl = (UIStyleItemImpl<FontFamily>)localState.StyleItem;
			setValue((TTarget)target, uIStyleItemImpl.GetValue(localState.CurrentReactivityIndex));
		}
	}
}
