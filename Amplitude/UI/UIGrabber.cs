using System;
using Amplitude.Framework;
using Amplitude.Graphics;
using Amplitude.UI.Renderers;
using UnityEngine;

namespace Amplitude.UI
{
	public struct UIGrabber : IDisposable
	{
		private static class ShaderStrings
		{
			public static ShaderString Grab = "_Grab";

			private static bool loaded = false;

			public static void Load()
			{
				if (!loaded)
				{
					Grab.Load();
					loaded = true;
				}
			}
		}

		private static readonly Color TransparentBlack = new Color(0f, 0f, 0f, 0f);

		private RenderTexture renderTexture;

		private UITexture uiTexture;

		private ColorSpace usedColorSpace;

		private bool active;

		private Vector2Int screenFlooredTopLeft;

		private Vector2Int screenCeiledBottomRight;

		public Vector2Int ScreenFlooredTopLeft => screenFlooredTopLeft;

		public Vector2Int ScreenCeiledBottomRight => screenCeiledBottomRight;

		public void Begin(UITransform uiTransform, UIPrimitiveDrawer drawer, ref Vector2 imageEffectCenterGlobalPos)
		{
			UITransform.MatrixFlags localToGlobalMatrixFlags = uiTransform.LocalToGlobalMatrixFlags;
			Matrix4x4 localToGlobalMatrix = uiTransform.LocalToGlobalMatrix;
			Rect localRect = uiTransform.LocalRect;
			Vector2 topLeft;
			Vector2 bottomRight;
			if ((localToGlobalMatrixFlags & (UITransform.MatrixFlags.HasRotation | UITransform.MatrixFlags.HasScale)) == 0)
			{
				Vector2 vector = new Vector2(localToGlobalMatrix.m03, localToGlobalMatrix.m13);
				topLeft = localRect.min + vector;
				bottomRight = localRect.max + vector;
			}
			else
			{
				UIPrimitiveDrawer.GetEnclosingVirtualCorners(ref localToGlobalMatrix, ref localRect, out topLeft, out bottomRight);
			}
			Vector2 vector2 = new Vector2(0.001f, 0.001f);
			Vector2Int.FloorToInt(topLeft + vector2);
			Vector2Int.CeilToInt(bottomRight - vector2);
			Vector2 vector3 = drawer.StandardizedPositionToCurrentRenderTargetPixelPosition(topLeft);
			Vector2 vector4 = drawer.StandardizedPositionToCurrentRenderTargetPixelPosition(bottomRight);
			screenFlooredTopLeft = Vector2Int.FloorToInt(vector3 + vector2);
			screenCeiledBottomRight = Vector2Int.CeilToInt(vector4 - vector2);
			Vector2 vector5 = drawer.RenderTargetPixelPositionToStandardizedPosition(screenFlooredTopLeft);
			Vector2 vector6 = drawer.RenderTargetPixelPositionToStandardizedPosition(screenCeiledBottomRight);
			Vector2Int vector2Int = screenCeiledBottomRight - screenFlooredTopLeft;
			active = vector2Int.x != 0 && vector2Int.y != 0;
			if (active)
			{
				drawer.BeginCommandSubmission();
				drawer.UifxInImageEffect = true;
				drawer.ImageEffectScreenPos = drawer.View.StandardizedPositionToScreenPosition(imageEffectCenterGlobalPos);
				drawer.ImageEffectWidthHeight = vector2Int;
				drawer.UpdateUifxMask();
				UpdateOrCreateRenderTexture(vector2Int, drawer.View.ColorSpace);
				float x = vector5.x;
				float y = vector5.y;
				float num = (vector6.x - vector5.x) / uiTexture.SubCoordinates.width;
				float num2 = (vector6.y - vector5.y) / uiTexture.SubCoordinates.height;
				float right = x + num;
				float bottom = y + num2;
				UIPrimitiveDrawer.FillProjectionMatrixForOffscreenRender(x, right, y, bottom, out var projection, out var projectionInverse);
				drawer.StartOffscreenRenderPass(renderTexture, ref projection, ref projectionInverse, clear: true, TransparentBlack);
				drawer.EndCommandSubmission();
			}
		}

		public UITexture End(UIPrimitiveDrawer drawer)
		{
			if (active)
			{
				drawer.BeginCommandSubmission();
				drawer.CloseOffscreenRenderPass();
				drawer.EndCommandSubmission();
				active = false;
			}
			return uiTexture;
		}

		public void Dispose()
		{
			if ((bool)renderTexture)
			{
				renderTexture.Release();
				UIRenderingManager.Instance?.UnregisterTexture(renderTexture);
				renderTexture = null;
				uiTexture = UITexture.White;
			}
		}

		private void UpdateOrCreateRenderTexture(Vector2Int widthHeight, ColorSpace colorSpace)
		{
			if (renderTexture == null || renderTexture.width < widthHeight.x || renderTexture.height < widthHeight.y || colorSpace != usedColorSpace)
			{
				if ((bool)renderTexture)
				{
					renderTexture.Release();
					UIRenderingManager.Instance.UnregisterTexture(renderTexture);
				}
				Vector2Int vector2Int = widthHeight;
				if (renderTexture != null)
				{
					vector2Int.x = Mathf.CeilToInt(Mathf.Max((float)renderTexture.width * 1.2f, vector2Int.x));
					vector2Int.y = Mathf.CeilToInt(Mathf.Max((float)renderTexture.height * 1.2f, vector2Int.y));
				}
				usedColorSpace = colorSpace;
				renderTexture = new RenderTexture(vector2Int.x, vector2Int.y, 0, RenderTextureFormat.ARGB32, (colorSpace != ColorSpace.Linear) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
				renderTexture.name = "UIGrabber render texture";
				renderTexture.Create();
				Amplitude.Framework.Guid guid = UIRenderingManager.Instance.RegisterTexture(renderTexture);
				UITextureColorFormat colorFormat = ((colorSpace != ColorSpace.Linear) ? UITextureColorFormat.Linear : UITextureColorFormat.Srgb);
				uiTexture = new UITexture(guid, UITextureFlags.AlphaPreMultiplied, colorFormat, renderTexture);
			}
			Rect subCoordinates = new Rect(0f, 0f, (float)widthHeight.x / (float)renderTexture.width, (float)widthHeight.y / (float)renderTexture.height);
			uiTexture.SubCoordinates = subCoordinates;
		}
	}
}
