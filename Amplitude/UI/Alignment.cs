using System;

namespace Amplitude.UI
{
	[Serializable]
	public struct Alignment
	{
		public HorizontalAlignment Horizontal;

		public VerticalAlignment Vertical;

		public float HorizontalOffset;

		public float VerticalOffset;

		public static Alignment CenterCenter => new Alignment(HorizontalAlignment.Center, VerticalAlignment.Center);

		public Alignment(HorizontalAlignment horizontal = HorizontalAlignment.Left, VerticalAlignment vertical = VerticalAlignment.Top)
		{
			Horizontal = horizontal;
			Vertical = vertical;
			HorizontalOffset = 0f;
			VerticalOffset = 0f;
		}

		public static bool operator ==(Alignment left, Alignment right)
		{
			if (left.Horizontal == right.Horizontal && left.Vertical == right.Vertical && left.HorizontalOffset == right.HorizontalOffset)
			{
				return left.VerticalOffset == right.VerticalOffset;
			}
			return false;
		}

		public static bool operator !=(Alignment left, Alignment right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (obj is Alignment)
			{
				return this == (Alignment)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return $"Alignment:{Horizontal}{Vertical}";
		}
	}
}
