using System;
using UnityEngine;

namespace Amplitude.UI.Text
{
	[Serializable]
	public class SymbolToRectMapper : SymbolToImageMapper
	{
		[SerializeField]
		[UIMaterialId(UIPrimitiveType.Rect)]
		private UIMaterialId material;

		public override void Render(UIPrimitiveDrawer drawer, Matrix4x4 transform, Rect position, Color color, UIBlendType blendType)
		{
			float width = position.width;
			position.x += width * base.Offset.x;
			position.y += width * base.Offset.y;
			position.width *= base.WidthHeight.x;
			position.height *= base.WidthHeight.y;
			UIAtomId transform2 = UIAtomContainer<Matrix4x4>.AllocateTemporary(ref transform);
			drawer.Rect(ref transform2, ref position, material.Id, base.Texture, ref AffineTransform2d.Identity, ref color, null, blendType);
		}
	}
}
