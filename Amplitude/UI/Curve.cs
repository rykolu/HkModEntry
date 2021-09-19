using System;
using UnityEngine;

namespace Amplitude.UI
{
	public struct Curve
	{
		public enum CurveType : uint
		{
			Linear,
			Quadratic,
			Cubic
		}

		public CurveType Type;

		public Vector2 Start;

		public Vector2 End;

		public Vector2 Control0;

		public Vector2 Control1;

		public float StartAbscissa;

		public float EndAbscissa;

		public Curve(Vector2 beginPoint, Vector2 endPoint)
		{
			Type = CurveType.Linear;
			Start = beginPoint;
			End = endPoint;
			Control0 = Vector2.zero;
			Control1 = Vector2.zero;
			StartAbscissa = 0f;
			EndAbscissa = 1f;
		}

		public Curve(Vector2 beginPoint, Vector2 controlPoint, Vector2 endPoint)
		{
			Type = CurveType.Quadratic;
			Start = beginPoint;
			Control0 = controlPoint;
			End = endPoint;
			Control1 = Vector2.zero;
			StartAbscissa = 0f;
			EndAbscissa = 1f;
		}

		public Curve(Vector2 beginPoint, Vector2 control0Point, Vector2 control1Point, Vector2 endPoint)
		{
			Type = CurveType.Cubic;
			Start = beginPoint;
			Control0 = control0Point;
			Control1 = control1Point;
			End = endPoint;
			StartAbscissa = 0f;
			EndAbscissa = 1f;
		}

		public float UpperBoundLength()
		{
			return Type switch
			{
				CurveType.Linear => Vector2.Distance(Start, End), 
				CurveType.Quadratic => Vector2.Distance(Start, Control0) + Vector2.Distance(End, Control0), 
				CurveType.Cubic => Vector2.Distance(Start, Control0) + Vector2.Distance(Control0, Control1) + Vector2.Distance(Control1, End), 
				_ => throw new NotImplementedException(), 
			};
		}
	}
}
