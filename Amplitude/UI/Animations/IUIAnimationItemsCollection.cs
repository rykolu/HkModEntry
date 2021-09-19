using System;

namespace Amplitude.UI.Animations
{
	public interface IUIAnimationItemsCollection
	{
		void Add<TTarget, TValue>(StaticString name, Func<TTarget, TValue> getFunc, Action<TTarget, TValue> setAction) where TTarget : class, IUIAnimationTarget;

		void Add<TTarget, TValue>(MutatorSet<TTarget, TValue> mutatorSet) where TTarget : class, IUIAnimationTarget;
	}
}
