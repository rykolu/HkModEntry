using System;

namespace Amplitude.UI
{
	[Serializable]
	public struct UIBorderAnchor
	{
		public bool Attach;

		public float Percent;

		public float Margin;

		public float Offset;

		public UIBorderAnchor(UIBorderAnchor other)
		{
			Attach = other.Attach;
			Percent = other.Percent;
			Margin = other.Margin;
			Offset = other.Offset;
		}

		public UIBorderAnchor(bool attach, float percent, float margin, float offset)
		{
			Attach = attach;
			Percent = percent;
			Margin = margin;
			Offset = offset;
		}

		public static bool operator ==(UIBorderAnchor left, UIBorderAnchor right)
		{
			if (left.Attach == right.Attach && left.Percent == right.Percent && left.Margin == right.Margin)
			{
				return left.Offset == right.Offset;
			}
			return false;
		}

		public static bool operator !=(UIBorderAnchor left, UIBorderAnchor right)
		{
			if (left.Attach == right.Attach && left.Percent == right.Percent && left.Margin == right.Margin)
			{
				return left.Offset != right.Offset;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (obj is UIBorderAnchor)
			{
				return this == (UIBorderAnchor)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public UIBorderAnchor SetAttach(bool b)
		{
			Attach = b;
			return this;
		}

		public UIBorderAnchor SetPercent(float percent)
		{
			Percent = percent;
			return this;
		}

		public UIBorderAnchor SetMargin(float margin)
		{
			Margin = margin;
			return this;
		}

		public UIBorderAnchor SetOffset(float offset)
		{
			Offset = offset;
			return this;
		}
	}
}
