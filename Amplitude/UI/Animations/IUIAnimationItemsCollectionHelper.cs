using System;
using Amplitude.UI.Animations.Scene;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	public static class IUIAnimationItemsCollectionHelper
	{
		public static void Add<TTarget, TValue>(IUIAnimationItemsCollection collection, StaticString name, Func<TTarget, TValue> getFunc, Action<TTarget, TValue> setAction) where TTarget : class, IUIAnimationTarget
		{
			if (Application.isEditor)
			{
				collection.Add(name, getFunc, setAction);
			}
			else
			{
				(collection as UIAnimatorComponent).Add(name, getFunc, setAction);
			}
		}

		public static void Add<TTarget, TValue>(IUIAnimationItemsCollection collection, MutatorSet<TTarget, TValue> mutatorSet) where TTarget : class, IUIAnimationTarget
		{
			if (Application.isEditor)
			{
				collection.Add(mutatorSet);
			}
			else
			{
				(collection as UIAnimatorComponent).Add(mutatorSet);
			}
		}
	}
}
