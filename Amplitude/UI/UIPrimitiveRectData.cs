using Amplitude.UI.Renderers;
using UnityEngine;

namespace Amplitude.UI
{
	public struct UIPrimitiveRectData
	{
		public Rect Position;

		public Rect Texcoords;

		public Color Color;

		public UITextureTransformData TexTransform;

		public RectMargins TexSlicingSize;

		public RectMargins TexSlicingUvs;

		public int TransformId;

		public int GradientId;

		public float StrokeWidth;

		public int MaterialPropertyOverridesId;

		public uint Flags;

		private static readonly int FlagsSlicingModeOffset = 0;

		private static readonly uint FlagsSlicingModeMask = 3u;

		public void SetSlicingMode(UIImage.TextureSlicingModeEnum slicingMode)
		{
			uint num = Flags & ~(FlagsSlicingModeMask << FlagsSlicingModeOffset);
			Flags = num | (((uint)slicingMode & FlagsSlicingModeMask) << FlagsSlicingModeOffset);
		}
	}
}
