using System;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[ExecuteInEditMode]
	public class UIRenderState : UIScopedRenderer
	{
		private struct RenderState
		{
			public PixelPerfectMode PixelPerfect;
		}

		[SerializeField]
		private PixelPerfectMode pixelPerfect;

		[NonSerialized]
		private RenderState savedRenderState;

		public PixelPerfectMode PixelPerfect
		{
			get
			{
				return pixelPerfect;
			}
			set
			{
				pixelPerfect = value;
			}
		}

		protected override void EnterRender(UIPrimitiveDrawer drawer)
		{
			savedRenderState.PixelPerfect = drawer.PixelPerfect;
			drawer.PixelPerfect = pixelPerfect;
		}

		protected override void LeaveRender(UIPrimitiveDrawer drawer)
		{
			drawer.PixelPerfect = savedRenderState.PixelPerfect;
		}
	}
}
