using UnityEngine;

namespace Amplitude.UI.Animations
{
	public abstract class UIAnimationInterpolator<Type> : IUIAnimationInterpolator
	{
		[SerializeField]
		public Type Min;

		[SerializeField]
		public Type Max;

		public abstract Type Interpolate(float t);
	}
}
