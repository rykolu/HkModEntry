using UnityEngine;

namespace Amplitude.UI
{
	public struct UIPrimitiveCurveData
	{
		public Curve.CurveType Type;

		public Vector2 Start;

		public Vector2 End;

		public Vector2 Control0;

		public Vector2 Control1;

		public Vector2 Abscissas;

		public Vector4 Texcoords;

		public Color Color;

		public float Thickness;

		public int TransformId;

		public int GradientId;
	}
}
