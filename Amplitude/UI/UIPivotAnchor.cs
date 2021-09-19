using System;

namespace Amplitude.UI
{
	[Serializable]
	public struct UIPivotAnchor
	{
		public bool Attach;

		public float Percent;

		public float MinMargin;

		public float MaxMargin;

		public float Offset;

		public UIPivotAnchor(bool attach, float percent, float minMargin, float maxMargin, float offset)
		{
			Attach = attach;
			Percent = percent;
			MinMargin = minMargin;
			MaxMargin = maxMargin;
			Offset = offset;
		}

		public static bool operator ==(UIPivotAnchor left, UIPivotAnchor right)
		{
			if (left.Attach == right.Attach && left.Percent == right.Percent && left.MinMargin == right.MinMargin && left.MaxMargin == right.MaxMargin)
			{
				return left.Offset == right.Offset;
			}
			return false;
		}

		public static bool operator !=(UIPivotAnchor left, UIPivotAnchor right)
		{
			if (left.Attach == right.Attach && left.Percent == right.Percent && left.MinMargin == right.MinMargin && left.MaxMargin == right.MaxMargin)
			{
				return left.Offset != right.Offset;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (obj is UIPivotAnchor)
			{
				return this == (UIPivotAnchor)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public UIPivotAnchor SetAttach(bool attach)
		{
			Attach = attach;
			return this;
		}

		public UIPivotAnchor SetPercent(float percent)
		{
			Percent = percent;
			return this;
		}

		public UIPivotAnchor SetMinMargin(float min)
		{
			MinMargin = min;
			return this;
		}

		public UIPivotAnchor SetMaxMargin(float max)
		{
			MaxMargin = max;
			return this;
		}

		public UIPivotAnchor SetOffset(float offset)
		{
			Offset = offset;
			return this;
		}
	}
}
