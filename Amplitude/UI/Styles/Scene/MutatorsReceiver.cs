using System;
using Amplitude.Graphics.Text;

namespace Amplitude.UI.Styles.Scene
{
	public class MutatorsReceiver
	{
		private PerformanceList<UIRuntimeStyleItem> items = new PerformanceList<UIRuntimeStyleItem>(16);

		internal int Count => items.Count;

		public void Add<TTarget, TValue>(MutatorSet<TTarget, TValue> mutator) where TTarget : UIComponent, IUIStyleTarget where TValue : struct
		{
			object value = null;
			Interpolators.InstancePerValueType.TryGetValue(typeof(TValue), out value);
			items.Add(new UIRuntimeStyleItemImpl<TTarget, TValue>(mutator.Name, mutator.Get, mutator.Set, value as IInterpolator<TValue>));
		}

		public void Add<TTarget>(MutatorSet<TTarget, UIGradient> mutator) where TTarget : UIComponent, IUIStyleTarget
		{
			items.Add(new UIRuntimeStyleGradientItem<TTarget>(mutator.Name, mutator.Get, mutator.Set, Interpolators.GradientInterpolator));
		}

		public void Add<TTarget>(MutatorSet<TTarget, FontFamily> mutator) where TTarget : UIComponent, IUIStyleTarget
		{
			items.Add(new UIRuntimeStyleFontFamilyItem<TTarget>(mutator.Name, mutator.Get, mutator.Set));
		}

		internal void Flush(ref PerformanceList<UIRuntimeStyleItem> output)
		{
			if (items.Count > 0)
			{
				output.Resize(items.Count);
				Array.Copy(items.Data, output.Data, items.Count);
				output.Count = items.Count;
				items.ClearArray();
			}
		}
	}
}
