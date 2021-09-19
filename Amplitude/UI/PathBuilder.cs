using System.Collections.Generic;
using UnityEngine;

namespace Amplitude.UI
{
	public struct PathBuilder
	{
		private List<Curve> curves;

		private Vector2 current;

		public List<Curve> Curves => curves;

		public void Start(Vector2 position)
		{
			current = position;
			if (curves == null)
			{
				curves = new List<Curve>();
			}
			else
			{
				curves.Clear();
			}
		}

		public void End()
		{
			current = Vector2.zero;
			float num = UpperBoundLength();
			float num2 = 0f;
			int count = curves.Count;
			for (int i = 0; i < count; i++)
			{
				Curve value = curves[i];
				value.StartAbscissa = num2 / num;
				num2 += value.UpperBoundLength();
				value.EndAbscissa = num2 / num;
				curves[i] = value;
			}
		}

		public void RelativeLineTo(Vector2 end)
		{
			if (end != Vector2.zero)
			{
				Vector2 endPoint = current + end;
				curves.Add(new Curve(current, endPoint));
				current = endPoint;
			}
		}

		public void AbsoluteLineTo(Vector2 end)
		{
			if (end != current)
			{
				curves.Add(new Curve(current, end));
				current = end;
			}
		}

		public void RelativeBezierTo(Vector2 control, Vector2 end)
		{
			if (!(end == Vector2.zero) || !(end == control))
			{
				Vector2 endPoint = current + end;
				Vector2 controlPoint = current + control;
				curves.Add(new Curve(current, controlPoint, endPoint));
				current = endPoint;
			}
		}

		public void AbsoluteBezierTo(Vector2 control, Vector2 end)
		{
			if (!(end == current) || !(end == control))
			{
				curves.Add(new Curve(current, control, end));
				current = end;
			}
		}

		public void RelativeBezierTo(Vector2 controlStart, Vector2 controlEnd, Vector2 end)
		{
			if (!(controlStart == Vector2.zero) || !(controlStart == controlEnd) || !(controlEnd == end))
			{
				Vector2 endPoint = current + end;
				Vector2 control0Point = current + controlStart;
				Vector2 control1Point = current + controlEnd;
				curves.Add(new Curve(current, control0Point, control1Point, endPoint));
				current = endPoint;
			}
		}

		public void AbsoluteBezierTo(Vector2 controlStart, Vector2 controlEnd, Vector2 end)
		{
			if (!(controlStart == current) || !(controlStart == controlEnd) || !(controlEnd == end))
			{
				curves.Add(new Curve(current, controlStart, controlEnd, end));
				current = end;
			}
		}

		public float UpperBoundLength()
		{
			float num = 0f;
			int count = curves.Count;
			for (int i = 0; i < count; i++)
			{
				num += curves[i].UpperBoundLength();
			}
			return num;
		}

		public void Clear()
		{
			current = Vector2.zero;
			curves?.Clear();
		}
	}
}
