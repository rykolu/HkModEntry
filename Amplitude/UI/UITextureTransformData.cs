using Amplitude.Framework;
using UnityEngine;

namespace Amplitude.UI
{
	public struct UITextureTransformData
	{
		public Vector4 Data0;

		public Vector2 Data1;

		public static implicit operator UITextureTransformData(AffineTransform2d affineTextureTransform)
		{
			UITextureTransformData result = default(UITextureTransformData);
			Math.SinCos(affineTextureTransform.Rotation, out result.Data0.x, out result.Data0.y);
			result.Data0.z = affineTextureTransform.Translation.x;
			result.Data0.w = affineTextureTransform.Translation.y;
			result.Data1 = affineTextureTransform.Scale;
			return result;
		}
	}
}
