using System;

namespace Amplitude.UI
{
	public struct MutatorSet<TTarget, TValue> where TTarget : class
	{
		public readonly StaticString Name;

		public readonly Func<TTarget, TValue> Get;

		public readonly Action<TTarget, TValue> Set;

		public MutatorSet(StaticString name, Func<TTarget, TValue> get, Action<TTarget, TValue> set)
		{
			Name = name;
			Get = get;
			Set = set;
		}
	}
}
