using System;

namespace Amplitude.UI
{
	[Serializable]
	public struct RectMargins : IEquatable<RectMargins>
	{
		public static RectMargins None;

		public float Left;

		public float Right;

		public float Top;

		public float Bottom;

		public RectMargins(float left, float right, float top, float bottom)
		{
			Left = left;
			Right = right;
			Top = top;
			Bottom = bottom;
		}

		public static bool operator ==(RectMargins left, RectMargins right)
		{
			if (left.Left == right.Left && left.Right == right.Right && left.Top == right.Top)
			{
				return left.Bottom == right.Bottom;
			}
			return false;
		}

		public static bool operator !=(RectMargins left, RectMargins right)
		{
			return !(left == right);
		}

		public bool Equals(RectMargins other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			if (obj is RectMargins)
			{
				return this == (RectMargins)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public RectMargins SetLeft(float left)
		{
			Left = left;
			return this;
		}

		public RectMargins SetRight(float right)
		{
			Right = right;
			return this;
		}

		public RectMargins SetTop(float top)
		{
			Top = top;
			return this;
		}

		public RectMargins SetBottom(float bottom)
		{
			Bottom = bottom;
			return this;
		}
	}
}
