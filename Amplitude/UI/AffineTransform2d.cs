using System;
using UnityEngine;

namespace Amplitude.UI
{
	[Serializable]
	public struct AffineTransform2d
	{
		public static AffineTransform2d Identity = new AffineTransform2d(Vector2.zero, 0f, Vector2.one);

		public Vector2 Translation;

		public Vector2 Scale;

		public float Rotation;

		public AffineTransform2d(Vector2 translation, float rotation, Vector2 scale)
		{
			Translation = translation;
			Rotation = rotation;
			Scale = scale;
		}
	}
}
