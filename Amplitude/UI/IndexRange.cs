using System;

namespace Amplitude.UI
{
	public struct IndexRange : IComparable<IndexRange>
	{
		public static readonly IndexRange Invalid = new IndexRange(-1L, -1L);

		public long Min;

		public long Max;

		public long First => Min;

		public long Last => Max - 1;

		public long Extent => Max - Min;

		public bool IsValid => this != Invalid;

		public IndexRange(long min, long max)
		{
			Min = min;
			Max = max;
		}

		public static bool operator ==(IndexRange left, IndexRange right)
		{
			if (left.Min == right.Min)
			{
				return left.Max == right.Max;
			}
			return false;
		}

		public static bool operator !=(IndexRange left, IndexRange right)
		{
			if (left.Min == right.Min)
			{
				return left.Max != right.Max;
			}
			return true;
		}

		public bool Contains(long value)
		{
			if (Min <= value)
			{
				return value < Max;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is IndexRange)
			{
				return this == (IndexRange)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			if (this == Invalid)
			{
				return "[invalid]";
			}
			return $"[{Min}-{Max}]";
		}

		public int CompareTo(IndexRange other)
		{
			return Min.CompareTo(other.Min);
		}
	}
}
