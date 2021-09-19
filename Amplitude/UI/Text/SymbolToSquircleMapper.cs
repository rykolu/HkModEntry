using System;
using UnityEngine;

namespace Amplitude.UI.Text
{
	[Serializable]
	public class SymbolToSquircleMapper : SymbolToImageMapper
	{
		[SerializeField]
		private float topLeftRadius = 1f;

		[SerializeField]
		private float topRightRadius = 1f;

		[SerializeField]
		private float bottomLeftRadius = 1f;

		[SerializeField]
		private float bottomRightRadius = 1f;

		[SerializeField]
		private FillMode fillMode;

		[SerializeField]
		private float strokeWidth = -1f;

		[SerializeField]
		[UIMaterialId(UIPrimitiveType.Squircle)]
		private UIMaterialId material;

		public override void Render(UIPrimitiveDrawer drawer, Matrix4x4 transform, Rect position, Color color, UIBlendType blendType)
		{
			float width = position.width;
			position.x += width * base.Offset.x;
			position.y += width * base.Offset.y;
			position.width *= base.WidthHeight.x;
			position.height *= base.WidthHeight.y;
			UIAtomId transform2 = UIAtomContainer<Matrix4x4>.AllocateTemporary(ref transform);
			drawer.Squircle(ref transform2, ref position, topLeftRadius, topRightRadius, bottomLeftRadius, bottomRightRadius, fillMode, strokeWidth, material.Id, base.Texture, ref AffineTransform2d.Identity, ref color, null, blendType);
		}
	}
}
