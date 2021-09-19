using System;
using Amplitude.Graphics;
using Amplitude.UI.Renderers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Amplitude.UI
{
	public struct UIBlurDrawer : IDisposable
	{
		private static class ShaderStrings
		{
			public static ShaderString BlurSourcePosition = "_BlurSourcePosition";

			public static ShaderString SourceTexture = "_SourceTexture";

			public static ShaderString PassIndex = "_PassIndex";

			public static ShaderString PreserveAlpha = "_PreserveAlpha";

			public static ShaderString BlurInput = "_BlurInput";

			public static ShaderString SourcePosition = "_SourcePosition";

			public static ShaderString BlurMask = "_BlurMask";

			public static ShaderString BlurGrab = "_BlurGrab";

			public static ShaderString BlurFront = "_BlurFront";

			public static ShaderString BlurBack = "_BlurBack";

			public static ShaderString FullscreenGrab = "_FullscreenGrab";

			private static bool loaded = false;

			public static void Load()
			{
				if (!loaded)
				{
					BlurSourcePosition.Load();
					SourceTexture.Load();
					PassIndex.Load();
					PreserveAlpha.Load();
					BlurInput.Load();
					SourcePosition.Load();
					BlurMask.Load();
					BlurGrab.Load();
					BlurFront.Load();
					BlurBack.Load();
					FullscreenGrab.Load();
					loaded = true;
				}
			}
		}

		[NonSerialized]
		private static Material subRectCopyMaterial;

		[NonSerialized]
		private static MaterialPropertyBlock subRectCopyMaterialProperties;

		[NonSerialized]
		private Material material;

		[NonSerialized]
		private MaterialPropertyBlock materialProperties;

		[NonSerialized]
		private Vector2Int blurAreaWidthHeight;

		public void Dispose()
		{
			materialProperties = null;
			material = null;
			blurAreaWidthHeight = Vector2Int.zero;
		}

		public void Begin(Matrix4x4 transform, Rect position, UIMaterialId material, UIPrimitiveDrawer drawer)
		{
			ShaderStrings.Load();
			if (subRectCopyMaterial == null)
			{
				_ = UIRenderingManager.Instance;
				subRectCopyMaterial = new Material(UIRenderingManager.Instance.MaterialCollection.GetCopyTextureShader());
				subRectCopyMaterial.EnableKeyword("_SUB_RECT");
				subRectCopyMaterialProperties = new MaterialPropertyBlock();
			}
			this.material = drawer.ResolveMaterial(UIPrimitiveType.Blur, UIBlendType.Standard, material);
			UIPrimitiveDrawer.GetEnclosingVirtualCorners(ref transform, ref position, out var topLeft, out var bottomRight);
			Vector2 v = drawer.StandardizedPositionToCurrentRenderTargetPixelPosition(topLeft);
			Vector2 v2 = drawer.StandardizedPositionToCurrentRenderTargetPixelPosition(bottomRight);
			Vector2Int vector2Int = Vector2Int.FloorToInt(v);
			Vector2Int vector2Int2 = Vector2Int.CeilToInt(v2);
			blurAreaWidthHeight = vector2Int2 - vector2Int;
			if (blurAreaWidthHeight.x != 0 && blurAreaWidthHeight.y != 0)
			{
				Color color = new Color(0f, 0f, 0f, 0f);
				Vector2Int vector2Int3 = drawer.CurrentRenderTargetWidthHeight();
				Vector4 value = new Vector4(v.x / (float)vector2Int3.x, v.y / (float)vector2Int3.y, v2.x / (float)vector2Int3.x, v2.y / (float)vector2Int3.y);
				if (materialProperties == null)
				{
					materialProperties = new MaterialPropertyBlock();
				}
				materialProperties.SetVector(ShaderStrings.BlurSourcePosition.MaterialId, value);
				CommandBuffer commandBuffer = drawer.BeginCommandSubmission();
				subRectCopyMaterialProperties.SetVector(ShaderStrings.SourcePosition.MaterialId, value);
				commandBuffer.SetGlobalTexture(ShaderStrings.SourceTexture.MaterialId, BuiltinRenderTextureType.CurrentActive);
				commandBuffer.GetTemporaryRT(ShaderStrings.BlurGrab.MaterialId, blurAreaWidthHeight.x, blurAreaWidthHeight.y, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
				commandBuffer.SetRenderTarget(ShaderStrings.BlurGrab.MaterialId);
				commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, color);
				commandBuffer.DrawProcedural(Matrix4x4.identity, subRectCopyMaterial, 0, MeshTopology.Triangles, 6, 1, subRectCopyMaterialProperties);
				commandBuffer.GetTemporaryRT(ShaderStrings.BlurMask.MaterialId, blurAreaWidthHeight.x, blurAreaWidthHeight.y, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
				UIPrimitiveDrawer.FillProjectionMatrixForOffscreenRender(topLeft.x, bottomRight.x, bottomRight.y, topLeft.y, out var projection, out var projectionInverse);
				drawer.StartOffscreenRenderPass(ShaderStrings.BlurMask.MaterialId, blurAreaWidthHeight.x, blurAreaWidthHeight.y, ref projection, ref projectionInverse, clear: true, color);
				drawer.EndCommandSubmission();
			}
		}

		public void End(UIPrimitiveDrawer drawer, bool preserveAlpha)
		{
			if (blurAreaWidthHeight.x == 0 || blurAreaWidthHeight.y == 0)
			{
				return;
			}
			CommandBuffer commandBuffer = drawer.BeginCommandSubmission();
			if ((bool)material)
			{
				int passCount = material.passCount;
				commandBuffer.SetGlobalTexture(ShaderStrings.BlurGrab.MaterialId, ShaderStrings.BlurGrab.MaterialId);
				commandBuffer.SetGlobalTexture(ShaderStrings.BlurMask.MaterialId, ShaderStrings.BlurMask.MaterialId);
				commandBuffer.SetGlobalTexture(ShaderStrings.BlurInput.MaterialId, ShaderStrings.BlurGrab.MaterialId);
				materialProperties.SetFloat(ShaderStrings.PreserveAlpha.MaterialId, preserveAlpha ? 1 : 0);
				if (passCount > 1)
				{
					Color backgroundColor = new Color(0f, 0f, 0f, 1f);
					commandBuffer.GetTemporaryRT(ShaderStrings.BlurFront.MaterialId, blurAreaWidthHeight.x, blurAreaWidthHeight.y, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
					if (passCount > 2)
					{
						commandBuffer.GetTemporaryRT(ShaderStrings.BlurBack.MaterialId, blurAreaWidthHeight.x, blurAreaWidthHeight.y, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
					}
					int num = -1;
					for (int i = 0; i + 1 < passCount; i++)
					{
						num = ((i % 2 == 0) ? ShaderStrings.BlurFront.MaterialId : ShaderStrings.BlurBack.MaterialId);
						commandBuffer.SetRenderTarget(num);
						commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, backgroundColor);
						materialProperties.SetFloat(ShaderStrings.PassIndex.MaterialId, i);
						commandBuffer.DrawProcedural(Matrix4x4.identity, material, i, MeshTopology.Triangles, 6, 1, materialProperties);
						commandBuffer.SetGlobalTexture(ShaderStrings.BlurInput.MaterialId, num);
					}
				}
				drawer.CloseOffscreenRenderPass();
				materialProperties.SetFloat(ShaderStrings.PassIndex.MaterialId, passCount - 1);
				commandBuffer.DrawProcedural(Matrix4x4.identity, material, passCount - 1, MeshTopology.Triangles, 6, 1, materialProperties);
			}
			else
			{
				drawer.CloseOffscreenRenderPass();
			}
			commandBuffer.ReleaseTemporaryRT(ShaderStrings.FullscreenGrab.MaterialId);
			commandBuffer.ReleaseTemporaryRT(ShaderStrings.BlurFront.MaterialId);
			commandBuffer.ReleaseTemporaryRT(ShaderStrings.BlurBack.MaterialId);
			commandBuffer.ReleaseTemporaryRT(ShaderStrings.BlurGrab.MaterialId);
			commandBuffer.ReleaseTemporaryRT(ShaderStrings.BlurMask.MaterialId);
			drawer.EndCommandSubmission();
		}

		public void AddToMaterialProperties(ref UIMaterialPropertyOverrides materialPropertyOverrides)
		{
			if (materialProperties == null)
			{
				materialProperties = new MaterialPropertyBlock();
			}
			materialPropertyOverrides.AddToMaterialPropertyBlock(materialProperties);
		}
	}
}
