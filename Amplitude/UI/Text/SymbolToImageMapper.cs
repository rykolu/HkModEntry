using System;
using UnityEngine;

namespace Amplitude.UI.Text
{
	[Serializable]
	public abstract class SymbolToImageMapper : SymbolMapper
	{
		[SerializeField]
		private UITexture texture;

		[SerializeField]
		private Vector2 widthHeight = Vector2.one;

		[SerializeField]
		private Vector2 offset = Vector2.zero;

		[SerializeField]
		private float advance = 1f;

		protected UITexture Texture
		{
			get
			{
				texture.RequestAsset();
				return texture;
			}
		}

		protected Vector2 WidthHeight => widthHeight;

		protected Vector2 Offset => offset;

		public abstract void Render(UIPrimitiveDrawer drawer, Matrix4x4 transform, Rect position, Color color, UIBlendType blendType);

		public override void Unload()
		{
			_ = texture;
			texture.Unload();
		}

		public virtual short GetAdvance(float squareSize)
		{
			return (short)Mathf.CeilToInt(advance * squareSize);
		}
	}
}
