using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amplitude.UI
{
	public static class Interpolators
	{
		public static readonly FloatInterpolator FloatInterpolator = new FloatInterpolator();

		public static readonly ColorInterpolator ColorInterpolator = new ColorInterpolator();

		public static readonly Vector3Interpolator Vector3Interpolator = new Vector3Interpolator();

		public static readonly GradientInterpolator GradientInterpolator = new GradientInterpolator();

		public static readonly Dictionary<Type, object> InstancePerValueType = new Dictionary<Type, object>
		{
			{
				typeof(float),
				FloatInterpolator
			},
			{
				typeof(Color),
				ColorInterpolator
			},
			{
				typeof(Vector3),
				Vector3Interpolator
			},
			{
				typeof(UIGradient),
				GradientInterpolator
			}
		};
	}
}
