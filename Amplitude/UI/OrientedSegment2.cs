using System;
using UnityEngine;

namespace Amplitude.UI
{
	[Serializable]
	public struct OrientedSegment2
	{
		public Vector2 Origin;

		public Vector2 End;

		public static OrientedSegment2 Zero => default(OrientedSegment2);

		public float Length => AsVector().magnitude;

		public OrientedSegment2(Vector2 origin, Vector2 end)
		{
			Origin = origin;
			End = end;
		}

		public static bool operator ==(OrientedSegment2 left, OrientedSegment2 right)
		{
			if (left.Origin == right.Origin)
			{
				return left.End == right.End;
			}
			return false;
		}

		public static bool operator !=(OrientedSegment2 left, OrientedSegment2 right)
		{
			if (!(left.Origin != right.Origin))
			{
				return left.End != right.End;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (obj is OrientedSegment2)
			{
				return this == (OrientedSegment2)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public Vector2 AsVector()
		{
			return new Vector2(End.x - Origin.x, End.y - Origin.y);
		}

		public override string ToString()
		{
			return AsVector().ToString();
		}
	}
}
