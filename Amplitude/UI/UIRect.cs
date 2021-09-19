using System;

namespace Amplitude.UI
{
	public struct UIRect
	{
		public float xMin;

		public float yMin;

		public float width;

		public float height;

		[Obsolete("use xMin")]
		public float x => xMin;

		[Obsolete("use xMin")]
		public float y => yMin;

		public float xMax => xMin + width;

		public float yMax => yMin + height;
	}
}
