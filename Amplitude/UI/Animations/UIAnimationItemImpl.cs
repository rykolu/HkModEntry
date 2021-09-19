using System;
using Amplitude.UI.Animations.Data;

namespace Amplitude.UI.Animations
{
	public class UIAnimationItemImpl<TTarget, TValue> : UIAnimationItem, IUIAnimationItem<TValue> where TTarget : class, IUIAnimationTarget
	{
		private readonly Func<TTarget, TValue> getFunc;

		private readonly Action<TTarget, TValue> setAction;

		private readonly IInterpolator<TValue> interpolator;

		private TValue min;

		private TValue max;

		private TValue current;

		public TValue Min
		{
			get
			{
				return min;
			}
			set
			{
				min = value;
			}
		}

		public TValue Max
		{
			get
			{
				return max;
			}
			set
			{
				max = value;
			}
		}

		public UIAnimationItemImpl(StaticString name, Func<TTarget, TValue> getFunc, Action<TTarget, TValue> setAction, IInterpolator<TValue> interpolator)
			: base(name)
		{
			this.getFunc = getFunc;
			this.setAction = setAction;
			this.interpolator = interpolator;
		}

		internal override void LoadFromAsset(UIAnimationItemAsset itemAsset)
		{
			base.LoadFromAsset(itemAsset);
			UIAnimationItemAssetImpl<TValue> uIAnimationItemAssetImpl = itemAsset as UIAnimationItemAssetImpl<TValue>;
			if (uIAnimationItemAssetImpl == null)
			{
				Diagnostics.LogError(54uL, "Invalid AssetItem Type.");
				return;
			}
			min = uIAnimationItemAssetImpl.Min;
			max = uIAnimationItemAssetImpl.Max;
		}

		internal override void Update(UIBehaviour target, float ratio)
		{
			float ratio2 = base.Curve.Evaluate(ratio);
			interpolator.Interpolate(min, max, ratio2, ref current);
			TTarget arg = target as TTarget;
			setAction(arg, current);
		}
	}
}
