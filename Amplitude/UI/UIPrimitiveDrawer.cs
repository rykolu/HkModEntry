using System;
using System.Collections.Generic;
using System.Diagnostics;
using Amplitude.Graphics;
using Amplitude.Graphics.Profiling;
using Amplitude.Graphics.Rendering;
using Amplitude.Graphics.Text;
using Amplitude.UI.Renderers;
using Amplitude.UI.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Amplitude.UI
{
	public class UIPrimitiveDrawer
	{
		private struct RectMaskData
		{
			public Matrix4x4 Transform;

			public Vector4 Position;
		}

		private struct DrawCall
		{
			public readonly Material Material;

			public readonly Texture Texture;

			public readonly UITextureFlags TextureFlags;

			public readonly UITextureColorFormat TextureColorFormat;

			public readonly int RangeStart;

			public int RangeEnd;

			public DrawCall(Material material, Texture texture, UITextureFlags textureFlags, UITextureColorFormat textureColorFormat, int rangeStart, int rangeCount = 1)
			{
				RangeStart = rangeStart;
				RangeEnd = rangeStart + rangeCount;
				Material = material;
				Texture = texture;
				TextureFlags = textureFlags;
				TextureColorFormat = textureColorFormat;
			}
		}

		private struct OffscreenRenderPassState
		{
			public RenderTargetIdentifier RenderTarget;

			public Vector2Int WidthHeight;

			public Matrix4x4 Projection;

			public Matrix4x4 InvertProjection;
		}

		private struct RenderState
		{
			public PixelPerfectMode PixelPerfect;
		}

		private struct IndexedPrimitive
		{
			public int Index;

			public int MaskIdStart;

			public int MaskIdCount;

			public int MaterialPropertiesId;
		}

		private static class ShaderStrings
		{
			public static ShaderString TexturePropertyId = "_Texture";

			public static ShaderString TextureFlagsPropertyId = "_TextureFlags";

			public static ShaderString TextureColorFormatPropertyId = "_TextureColorFormat";

			public static ShaderString RectMasksBufferPropertyId = "_RectMasksBuffer";

			public static ShaderString RectMaskIdsBufferPropertyId = "_RectMaskIdsBuffer";

			public static ShaderString MaterialPropertiesBufferPropertyId = "_MaterialFieldsBuffer";

			public static ShaderString VirtualWidthHeightPropertyId = "_VirtualWidthHeight";

			public static ShaderString PixelPerfectModePropertyId = "_PixelPerfectMode";

			public static ShaderString RenderTargetWidthHeight = "_RenderTargetWidthHeight";

			public static ShaderString DataBufferShaderPropertyId = "_ProceduralBuffer";

			public static ShaderString StartOffsetShaderPropertyId = "_ProceduralStartOffset";

			public static ShaderString UsedColorSpaceId = "_UsedColorSpace";

			public static ShaderString SourceTextureId = "_SourceTexture";

			public static ShaderString SourcePositionId = "_SourcePosition";

			public static ShaderString FlipId = "_Flip";

			public static ShaderString UifxInImageEffect = "_UifxInImageEffect";

			public static ShaderString ImageEffectCenter = "_ImageEffectCenter";

			public static ShaderString ImageEffectWidthHeight = "_ImageEffectWidthHeight";

			public static ShaderString CurrentFxMaskStart = "_CurrentFxMaskStart";

			public static ShaderString CurrentFxMaskCount = "_CurrentFxMaskCount";

			public static ShaderString ScreenToGlobalMatrix = "_ScreenToGlobalMatrix";

			public static ShaderString GrabRenderTarget = "_GrabRenderTarget";

			public static void Load()
			{
				TexturePropertyId.Load();
				TextureFlagsPropertyId.Load();
				TextureColorFormatPropertyId.Load();
				RectMasksBufferPropertyId.Load();
				RectMaskIdsBufferPropertyId.Load();
				MaterialPropertiesBufferPropertyId.Load();
				VirtualWidthHeightPropertyId.Load();
				PixelPerfectModePropertyId.Load();
				RenderTargetWidthHeight.Load();
				DataBufferShaderPropertyId.Load();
				StartOffsetShaderPropertyId.Load();
				UsedColorSpaceId.Load();
				SourceTextureId.Load();
				SourcePositionId.Load();
				FlipId.Load();
				UifxInImageEffect.Load();
				ImageEffectCenter.Load();
				ImageEffectWidthHeight.Load();
				CurrentFxMaskStart.Load();
				CurrentFxMaskCount.Load();
				ScreenToGlobalMatrix.Load();
				GrabRenderTarget.Load();
			}
		}

		public struct CurveAtomIds
		{
			public UIAtomId CurveId;

			public UIAtomId SegmentsId;

			public int SegmentCount;
		}

		public static readonly StaticString DistanceFieldVariantName = new StaticString("DistanceField");

		private const int QuadVertexCountPerElement = 6;

		private const int DrawCallsRegrowSize = 1000;

		[NonSerialized]
		private readonly UIView view;

		[NonSerialized]
		private readonly FontAtlasRenderer fontAtlas;

		[NonSerialized]
		private readonly List<int> activeRectMaskIds;

		[NonSerialized]
		private readonly UIMaterialProperties activeMaterialProperties;

		[NonSerialized]
		private readonly ushort gpuProfileId;

		[NonSerialized]
		private Material copyToGammaSpaceRenderTargetMaterial;

		[NonSerialized]
		private Material copyFromGammaSpaceToSDRRenderTargetMaterial;

		[NonSerialized]
		private Material copyFromGammaSpaceToHDRRenderTargetMaterial;

		[NonSerialized]
		private CommandBuffer commandBuffer;

		[NonSerialized]
		private bool commandBufferRegistered;

		private DynamicWriteBuffer1D<RectMaskData> rectMasksBuffer;

		private DynamicWriteBuffer1D<int> rectMaskIdsBuffer;

		private DynamicWriteBuffer1D<float> materialPropertiesBuffer;

		private DynamicWriteBuffer1D<IndexedPrimitive> indexedPrimitiveBuffer;

		private int materialPropertiesIndex = -1;

		private int activeRectMaskIdStart = -1;

		private PerformanceList<DrawCall> drawCalls;

		private Material lastDrawCallMaterial;

		private Texture lastDrawCallTexture;

		private PerformanceList<UIMaterialProperties> materialsStack;

		private PerformanceList<OffscreenRenderPassState> offscreenRenderPassStack;

		private MaterialPropertyBlock propertyBlock;

		private RenderState renderState;

		private RenderTexture gammaSpaceResolvedRenderTexture;

		private bool uifxInImageEffect;

		private Vector2 imageEffectCenter = Vector2.zero;

		private Vector2 imageEffectWidthHeight = Vector2.one;

		public const float SegmentLength = 5f;

		private readonly PathBuilder pathBuilder;

		private static Vector2[] localCorners = new Vector2[4];

		private static ProcessedText processedText = new ProcessedText();

		public UIView View => view;

		public PixelPerfectMode PixelPerfect
		{
			get
			{
				return renderState.PixelPerfect;
			}
			set
			{
				if (renderState.PixelPerfect != value)
				{
					FlushPendingDrawCalls();
					renderState.PixelPerfect = value;
					propertyBlock.SetFloat(ShaderStrings.PixelPerfectModePropertyId.MaterialId, (float)renderState.PixelPerfect);
				}
			}
		}

		public bool CommandBufferRegistered
		{
			get
			{
				return commandBufferRegistered;
			}
			internal set
			{
				if (commandBufferRegistered != value)
				{
					if (value)
					{
						view.RenderCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
					}
					else
					{
						view.RenderCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
					}
					commandBufferRegistered = value;
				}
			}
		}

		public CommandBuffer CommandBuffer => commandBuffer;

		public int CurrentOffsetScreenStackSize => offscreenRenderPassStack.Count;

		public Vector2 ImageEffectScreenPos
		{
			set
			{
				imageEffectCenter = value;
			}
		}

		public bool UifxInImageEffect
		{
			set
			{
				uifxInImageEffect = value;
			}
		}

		public Vector2 ImageEffectWidthHeight
		{
			set
			{
				imageEffectWidthHeight = value;
			}
		}

		private RenderTargetIdentifier RenderTargetId
		{
			get
			{
				if (view.ColorSpace == ColorSpace.Gamma)
				{
					return UIHook.GammaMainRenderTargetIdentifier;
				}
				return BuiltinRenderTextureType.CameraTarget;
			}
		}

		[Obsolete("Please use Circle with transform Atom id")]
		public void Circle(Matrix4x4 transform, Rect position, UITexture texture, UIBlendType blendType = UIBlendType.Standard)
		{
			Circle(transform, position, FillMode.Fill, -1f, UIMaterialCollection.DefaultMaterialNameId, texture, AffineTransform2d.Identity, Color.white, null, null, blendType);
		}

		public void Circle(ref UIAtomId transform, ref Rect position, UITexture texture, UIBlendType blendType = UIBlendType.Standard)
		{
			Color color = Color.white;
			Circle(ref transform, ref position, FillMode.Fill, -1f, UIMaterialCollection.DefaultMaterialNameId, texture, ref AffineTransform2d.Identity, ref color, null, null, blendType);
		}

		[Obsolete("Please use Circle with transform Atom id")]
		public void Circle(Matrix4x4 transform, Rect position, UITexture texture, Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Circle(transform, position, FillMode.Fill, -1f, UIMaterialCollection.DefaultMaterialNameId, texture, AffineTransform2d.Identity, color, null, null, blendType);
		}

		public void Circle(ref UIAtomId transform, ref Rect position, UITexture texture, ref Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Color color2 = Color.white;
			Circle(ref transform, ref position, FillMode.Fill, -1f, UIMaterialCollection.DefaultMaterialNameId, texture, ref AffineTransform2d.Identity, ref color2, null, null, blendType);
		}

		[Obsolete("Please use Circle with transform Atom id")]
		public void Circle(Matrix4x4 transform, Rect position, UITexture texture, float strokeWidth, UIBlendType blendType = UIBlendType.Standard)
		{
			Circle(transform, position, FillMode.Stroke, strokeWidth, UIMaterialCollection.DefaultMaterialNameId, texture, AffineTransform2d.Identity, Color.white, null, null, blendType);
		}

		public void Circle(ref UIAtomId transform, ref Rect position, UITexture texture, float strokeWidth, UIBlendType blendType = UIBlendType.Standard)
		{
			Color color = Color.white;
			Circle(ref transform, ref position, FillMode.Stroke, strokeWidth, UIMaterialCollection.DefaultMaterialNameId, texture, ref AffineTransform2d.Identity, ref color, null, null, blendType);
		}

		[Obsolete("Please use Circle with transform Atom id")]
		public void Circle(Matrix4x4 transform, Rect position, UITexture texture, Color color, float strokeWidth, UIBlendType blendType = UIBlendType.Standard)
		{
			Circle(transform, position, FillMode.Stroke, strokeWidth, UIMaterialCollection.DefaultMaterialNameId, texture, AffineTransform2d.Identity, color, null, null, blendType);
		}

		public void Circle(ref UIAtomId transform, ref Rect position, UITexture texture, ref Color color, float strokeWidth, UIBlendType blendType = UIBlendType.Standard)
		{
			Circle(ref transform, ref position, FillMode.Stroke, strokeWidth, UIMaterialCollection.DefaultMaterialNameId, texture, ref AffineTransform2d.Identity, ref color, null, null, blendType);
		}

		[Obsolete("Please use Circle with transform Atom id")]
		public void Circle(Matrix4x4 transform, Rect position, FillMode fillMode, float strokeWidth, StaticString materialName, UITexture texture, AffineTransform2d coordinatesTransform, Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Circle(transform, position, fillMode, strokeWidth, materialName, texture, coordinatesTransform, color, null, null, blendType);
		}

		public void Circle(ref UIAtomId transform, ref Rect position, FillMode fillMode, float strokeWidth, StaticString materialName, UITexture texture, ref AffineTransform2d coordinatesTransform, ref Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Circle(ref transform, ref position, fillMode, strokeWidth, materialName, texture, ref coordinatesTransform, ref color, null, null, blendType);
		}

		[Obsolete("Please use Circle with transform Atom id")]
		public void Circle(Matrix4x4 transform, Rect position, FillMode fillMode, float strokeWidth, StaticString materialName, UITexture texture, AffineTransform2d coordinatesTransform, Color color, UIGradient gradient, UIAtomId? optionalMaterialPropertyOverridesAtomId = null, UIBlendType blendType = UIBlendType.Standard)
		{
			texture.RequestAsset();
			UIAtomId uIAtomId = UIAtomContainer<Matrix4x4>.AllocateTemporary(ref transform);
			int gradientId = gradient?.GetAtomId().Index ?? (-1);
			UIPrimitiveCircleData value = default(UIPrimitiveCircleData);
			value.Position = position;
			value.Texcoords = texture.Coordinates;
			value.Color = color;
			value.TexTransform = coordinatesTransform;
			value.TransformId = uIAtomId.Index;
			value.GradientId = gradientId;
			value.StrokeWidth = ((fillMode == FillMode.Fill) ? (-1f) : strokeWidth);
			value.MaterialPropertyOverridesId = (optionalMaterialPropertyOverridesAtomId.HasValue ? optionalMaterialPropertyOverridesAtomId.Value.Index : (-1));
			UIAtomId circleId = UIAtomContainer<UIPrimitiveCircleData>.AllocateTemporary(ref value);
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Circle, blendType, materialName);
			Circle(ref circleId, texture, material);
		}

		public void Circle(ref UIAtomId transform, ref Rect position, FillMode fillMode, float strokeWidth, StaticString materialName, UITexture texture, ref AffineTransform2d coordinatesTransform, ref Color color, UIGradient gradient, UIAtomId? optionalMaterialPropertyOverridesAtomId = null, UIBlendType blendType = UIBlendType.Standard)
		{
			texture.RequestAsset();
			int gradientId = gradient?.GetAtomId().Index ?? (-1);
			UIPrimitiveCircleData value = default(UIPrimitiveCircleData);
			value.Position = position;
			value.Texcoords = texture.Coordinates;
			value.Color = color;
			value.TexTransform = coordinatesTransform;
			value.TransformId = transform.Index;
			value.GradientId = gradientId;
			value.StrokeWidth = ((fillMode == FillMode.Fill) ? (-1f) : strokeWidth);
			value.MaterialPropertyOverridesId = (optionalMaterialPropertyOverridesAtomId.HasValue ? optionalMaterialPropertyOverridesAtomId.Value.Index : (-1));
			UIAtomId circleId = UIAtomContainer<UIPrimitiveCircleData>.AllocateTemporary(ref value);
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Circle, blendType, materialName);
			Circle(ref circleId, texture, material);
		}

		public void Circle(ref UIAtomId circleId)
		{
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Circle, UIBlendType.Standard);
			Circle(ref circleId, UITexture.White, material);
		}

		public void Circle(ref UIAtomId circleId, UITexture texture, StaticString materialName, UIBlendType blendType = UIBlendType.Standard)
		{
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Circle, blendType, materialName);
			Circle(ref circleId, texture, material);
		}

		public void Circle(ref UIAtomId circleId, UITexture texture, Material material)
		{
			activeRectMaskIdStart = rectMaskIdsBuffer.Size;
			rectMaskIdsBuffer.WriteRange(activeRectMaskIds);
			Texture texture2 = (texture.Texture ? texture.Texture : texture.GetAsset());
			int num = AppendToDrawCalls(material, texture2, texture.Flags, texture.ColorFormat, 1);
			IndexedPrimitive[] storage = indexedPrimitiveBuffer.Storage;
			storage[num].Index = circleId.Index;
			storage[num].MaskIdStart = activeRectMaskIdStart;
			storage[num].MaskIdCount = activeRectMaskIds.Count;
			storage[num].MaterialPropertiesId = materialPropertiesIndex;
		}

		public UIPrimitiveDrawer(UIView view, FontAtlasRenderer fontAtlas)
		{
			ShaderStrings.Load();
			this.view = view;
			this.fontAtlas = fontAtlas;
			commandBuffer = new CommandBuffer();
			commandBuffer.name = "UIProceduralPrimitiveDrawer";
			activeRectMaskIds = new List<int>();
			this.view.RenderCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
			commandBufferRegistered = true;
			drawCalls = new PerformanceList<DrawCall>(view.DrawCallInitialStartSize);
			indexedPrimitiveBuffer = new DynamicWriteBuffer1D<IndexedPrimitive>("IndexedPrimitiveBuffer", view.IndexedPrimitiveBufferStartSize);
			rectMasksBuffer = new DynamicWriteBuffer1D<RectMaskData>("RectMasksBuffer", view.RectMasksBufferStartSize);
			rectMaskIdsBuffer = new DynamicWriteBuffer1D<int>("RectMaskIdsBuffer", view.RectMaskIdsBufferStartSize);
			materialPropertiesBuffer = new DynamicWriteBuffer1D<float>("MaterialFieldsBuffer", view.MaterialPropertiesBufferStartSize);
			propertyBlock = new MaterialPropertyBlock();
			activeMaterialProperties = new UIMaterialProperties();
			gpuProfileId = GPUProfiler.SampleClassId($"{view.name}.UIPrimitiveDrawer", this);
			LoadMaterials();
			AllOpen();
			AllClose();
			AddBuffersToShaderGlobal();
		}

		public void BeginMask(Matrix4x4 transform, Rect rect)
		{
			int size = rectMasksBuffer.Size;
			RectMaskData item = default(RectMaskData);
			item.Position = new Vector4(rect.position.x, rect.position.y, rect.width, rect.height);
			item.Transform = transform.inverse;
			rectMasksBuffer.Write(ref item);
			activeRectMaskIds.Add(size);
		}

		public void EndMask()
		{
			activeRectMaskIds.RemoveAt(activeRectMaskIds.Count - 1);
		}

		public void PushMaterial(UIMaterialProperties material)
		{
			materialsStack.Add(material);
			materialPropertiesIndex = WriteMaterialPropertiesReturnIndex();
		}

		public void PopMaterial()
		{
			materialsStack.RemoveAt(materialsStack.Count - 1);
			materialPropertiesIndex = WriteMaterialPropertiesReturnIndex();
		}

		public void Open()
		{
			AllOpen();
			commandBuffer.Clear();
			bool active = HDROutputSettings.main.active;
			RenderTextureFormat format = RenderTextureFormat.ARGB32;
			if (active)
			{
				format = HDROutputSettings.main.format;
			}
			ColorSpace colorSpace = view.ColorSpace;
			new RenderTargetIdentifier(BuiltinRenderTextureType.None);
			Vector2Int vector2Int = new Vector2Int(view.OutputWidth, view.OutputHeight);
			if (gammaSpaceResolvedRenderTexture == null || gammaSpaceResolvedRenderTexture.width != vector2Int.x || gammaSpaceResolvedRenderTexture.height != vector2Int.y)
			{
				gammaSpaceResolvedRenderTexture?.Release();
				RenderTextureDescriptor desc = new RenderTextureDescriptor(vector2Int.x, vector2Int.y, RenderTextureFormat.ARGB2101010, 0);
				desc.sRGB = false;
				gammaSpaceResolvedRenderTexture = new RenderTexture(desc);
				gammaSpaceResolvedRenderTexture.name = "UI.GammaSpaceResolvedRenderTexture";
				gammaSpaceResolvedRenderTexture.Create();
				UIHook.GammaMainRenderTargetIdentifier = gammaSpaceResolvedRenderTexture;
			}
			if (colorSpace == ColorSpace.Gamma)
			{
				if (UIHook.FillGammaMainRenderTargetFromSwapChain)
				{
					commandBuffer.GetTemporaryRT(ShaderStrings.GrabRenderTarget.MaterialId, vector2Int.x, vector2Int.y, 0, FilterMode.Point, format, RenderTextureReadWrite.sRGB);
					commandBuffer.Blit(null, ShaderStrings.GrabRenderTarget.MaterialId);
					commandBuffer.SetRenderTarget(gammaSpaceResolvedRenderTexture);
					commandBuffer.SetGlobalTexture(ShaderStrings.SourceTextureId.MaterialId, ShaderStrings.GrabRenderTarget.MaterialId);
					copyToGammaSpaceRenderTargetMaterial.SetVector(ShaderStrings.SourceTextureId.MaterialId, new Vector4(0f, 0f, 1f, 1f));
					bool flag = view.FlipBackground;
					if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
					{
						flag = !flag;
					}
					copyToGammaSpaceRenderTargetMaterial.SetFloat(ShaderStrings.FlipId.MaterialId, flag ? 1 : 0);
					commandBuffer.DrawProcedural(Matrix4x4.identity, copyToGammaSpaceRenderTargetMaterial, 0, MeshTopology.Triangles, 6);
					commandBuffer.ReleaseTemporaryRT(ShaderStrings.GrabRenderTarget.MaterialId);
				}
				else
				{
					commandBuffer.SetRenderTarget(gammaSpaceResolvedRenderTexture);
				}
			}
			commandBuffer.SetGlobalFloat(ShaderStrings.UsedColorSpaceId.MaterialId, (float)colorSpace);
			propertyBlock.SetFloat(ShaderStrings.PixelPerfectModePropertyId.MaterialId, (float)renderState.PixelPerfect);
			propertyBlock.SetVector(ShaderStrings.VirtualWidthHeightPropertyId.MaterialId, UIHierarchyManager.Instance.RenderedWidthHeight);
			commandBuffer.SetGlobalVector(ShaderStrings.RenderTargetWidthHeight.MaterialId, Vector2.zero);
			commandBuffer.SetGlobalInt(ShaderStrings.UifxInImageEffect.MaterialId, 0);
			commandBuffer.SetGlobalVector(ShaderStrings.ImageEffectCenter.MaterialId, Vector2.zero);
			commandBuffer.SetGlobalVector(ShaderStrings.ImageEffectWidthHeight.MaterialId, Vector2.zero);
			commandBuffer.SetGlobalInt(ShaderStrings.CurrentFxMaskStart.MaterialId, -1);
			commandBuffer.SetGlobalInt(ShaderStrings.CurrentFxMaskCount.MaterialId, -1);
			commandBuffer.SetGlobalMatrix(ShaderStrings.ScreenToGlobalMatrix.MaterialId, Matrix4x4.zero);
			drawCalls.Clear();
			activeRectMaskIds.Clear();
		}

		public void Close()
		{
			AllClose();
			AllSynchronise();
			EmitDrawCalls();
			if (view.ColorSpace == ColorSpace.Gamma)
			{
				commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
				commandBuffer.SetGlobalTexture(ShaderStrings.SourceTextureId.MaterialId, UIHook.GammaMainRenderTargetIdentifier);
				Material material = (HDROutputSettings.main.active ? copyFromGammaSpaceToHDRRenderTargetMaterial : copyFromGammaSpaceToSDRRenderTargetMaterial);
				material.SetVector(ShaderStrings.SourcePositionId.MaterialId, new Vector4(0f, 0f, 1f, 1f));
				commandBuffer.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 6);
			}
		}

		public void Release()
		{
			if (view.RenderCamera != null && commandBuffer != null && commandBufferRegistered)
			{
				view.RenderCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
			}
			commandBufferRegistered = false;
			commandBuffer?.Release();
			commandBuffer = null;
			rectMasksBuffer.Release();
			rectMaskIdsBuffer.Release();
			materialPropertiesBuffer.Release();
			gammaSpaceResolvedRenderTexture?.Release();
			gammaSpaceResolvedRenderTexture = null;
			UIHook.GammaMainRenderTargetIdentifier = new RenderTargetIdentifier(BuiltinRenderTextureType.None);
		}

		public CommandBuffer BeginCommandSubmission()
		{
			FlushPendingDrawCalls();
			return commandBuffer;
		}

		public void EndCommandSubmission()
		{
			FlushPendingDrawCalls();
		}

		public void StartOffscreenRenderPass(RenderTargetIdentifier renderTarget, int width, int height, ref Matrix4x4 projection, ref Matrix4x4 projectionInverse, bool clear, Color clearColor)
		{
			OffscreenRenderPassState item = default(OffscreenRenderPassState);
			item.RenderTarget = renderTarget;
			item.WidthHeight = new Vector2Int(width, height);
			item.Projection = projection;
			item.InvertProjection = projectionInverse;
			offscreenRenderPassStack.Add(ref item);
			commandBuffer.SetRenderTarget(renderTarget);
			if (clear)
			{
				commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, clearColor);
			}
			commandBuffer.SetProjectionMatrix(projection);
			commandBuffer.SetGlobalVector(ShaderStrings.RenderTargetWidthHeight.MaterialId, new Vector2(width, height));
			int value = (uifxInImageEffect ? 1 : 0);
			commandBuffer.SetGlobalInt(ShaderStrings.UifxInImageEffect.MaterialId, value);
			commandBuffer.SetGlobalVector(ShaderStrings.ImageEffectCenter.MaterialId, imageEffectCenter);
			commandBuffer.SetGlobalVector(ShaderStrings.ImageEffectWidthHeight.MaterialId, imageEffectWidthHeight);
		}

		public void StartOffscreenRenderPass(RenderTexture renderTarget, ref Matrix4x4 projection, ref Matrix4x4 projectionInverse, bool clear, Color clearColor)
		{
			StartOffscreenRenderPass(renderTarget, renderTarget.width, renderTarget.height, ref projection, ref projectionInverse, clear, clearColor);
		}

		public void CloseOffscreenRenderPass()
		{
			offscreenRenderPassStack.RemoveBack();
			if (offscreenRenderPassStack.Count > 0)
			{
				commandBuffer.SetRenderTarget(offscreenRenderPassStack.Data[offscreenRenderPassStack.Count - 1].RenderTarget);
				commandBuffer.SetProjectionMatrix(offscreenRenderPassStack.Data[offscreenRenderPassStack.Count - 1].Projection);
				Vector2Int widthHeight = offscreenRenderPassStack.Data[offscreenRenderPassStack.Count - 1].WidthHeight;
				commandBuffer.SetGlobalVector(ShaderStrings.RenderTargetWidthHeight.MaterialId, new Vector2(widthHeight.x, widthHeight.y));
				int value = (uifxInImageEffect ? 1 : 0);
				commandBuffer.SetGlobalInt(ShaderStrings.UifxInImageEffect.MaterialId, value);
				commandBuffer.SetGlobalVector(ShaderStrings.ImageEffectCenter.MaterialId, imageEffectCenter);
				commandBuffer.SetGlobalVector(ShaderStrings.ImageEffectWidthHeight.MaterialId, imageEffectWidthHeight);
			}
			else
			{
				commandBuffer.SetRenderTarget(RenderTargetId);
				commandBuffer.SetProjectionMatrix(View.ProjectionMatrix);
				commandBuffer.SetGlobalVector(ShaderStrings.RenderTargetWidthHeight.MaterialId, Vector2.zero);
				commandBuffer.SetGlobalInt(ShaderStrings.UifxInImageEffect.MaterialId, 0);
				commandBuffer.SetGlobalVector(ShaderStrings.ImageEffectCenter.MaterialId, Vector2.zero);
				commandBuffer.SetGlobalVector(ShaderStrings.ImageEffectWidthHeight.MaterialId, Vector2.one);
			}
			commandBuffer.SetGlobalInt(ShaderStrings.CurrentFxMaskStart.MaterialId, -1);
			commandBuffer.SetGlobalInt(ShaderStrings.CurrentFxMaskCount.MaterialId, -1);
		}

		public Vector2 StandardizedPositionToCurrentRenderTargetPixelPosition(Vector2 standardizedPosition)
		{
			if (offscreenRenderPassStack.Count == 0)
			{
				return view.StandardizedPositionToScreenPosition(standardizedPosition);
			}
			Matrix4x4 projection = offscreenRenderPassStack.Data[offscreenRenderPassStack.Count - 1].Projection;
			Vector2Int widthHeight = offscreenRenderPassStack.Data[offscreenRenderPassStack.Count - 1].WidthHeight;
			Vector4 vector = projection * new Vector4(standardizedPosition.x, standardizedPosition.y, 0f, 1f);
			float num = vector.x * 0.5f + 0.5f;
			float num2 = (0f - vector.y) * 0.5f + 0.5f;
			num *= (float)widthHeight.x;
			num2 *= (float)widthHeight.y;
			return new Vector2(num, num2);
		}

		public Vector2 RenderTargetPixelPositionToStandardizedPosition(Vector2 screenPosition)
		{
			if (offscreenRenderPassStack.Count == 0)
			{
				return view.ScreenPositionToStandardizedPosition(screenPosition);
			}
			Matrix4x4 projection = offscreenRenderPassStack.Data[offscreenRenderPassStack.Count - 1].Projection;
			Vector2Int widthHeight = offscreenRenderPassStack.Data[offscreenRenderPassStack.Count - 1].WidthHeight;
			float num = screenPosition.x / (float)widthHeight.x;
			float num2 = screenPosition.y / (float)widthHeight.y;
			float x = (num - 0.5f) / 0.5f;
			float y = (0f - (num2 - 0.5f)) / 0.5f;
			Vector4 vector = projection.inverse * new Vector4(x, y, 0f, 1f);
			return new Vector2(vector.x, vector.y);
		}

		public Vector2Int CurrentRenderTargetWidthHeight()
		{
			if (offscreenRenderPassStack.Count == 0)
			{
				return new Vector2Int(view.OutputWidth, view.OutputHeight);
			}
			return offscreenRenderPassStack.Data[offscreenRenderPassStack.Count - 1].WidthHeight;
		}

		public Material ResolveMaterial(UIPrimitiveType primitiveType, UIBlendType blendType, UIMaterialId id)
		{
			return UIMaterialCollection.ResolveMaterial(primitiveType, blendType, id.Id);
		}

		public void UpdateUifxMask()
		{
			if (activeRectMaskIds != null && activeRectMaskIds.Count > 0)
			{
				activeRectMaskIdStart = rectMaskIdsBuffer.Size;
				rectMaskIdsBuffer.WriteRange(activeRectMaskIds);
				commandBuffer.SetGlobalInt(ShaderStrings.CurrentFxMaskStart.MaterialId, activeRectMaskIdStart);
				commandBuffer.SetGlobalInt(ShaderStrings.CurrentFxMaskCount.MaterialId, activeRectMaskIds.Count);
				commandBuffer.SetGlobalMatrix(ShaderStrings.ScreenToGlobalMatrix.MaterialId, view.GlobalToScreenMatrix.inverse);
			}
			else
			{
				commandBuffer.SetGlobalInt(ShaderStrings.CurrentFxMaskStart.MaterialId, -1);
				commandBuffer.SetGlobalInt(ShaderStrings.CurrentFxMaskCount.MaterialId, -1);
			}
		}

		private void EmitDrawCalls()
		{
			int num = 0;
			int count = drawCalls.Count;
			Texture texture = null;
			UITextureFlags uITextureFlags = (UITextureFlags)(-1);
			UITextureColorFormat uITextureColorFormat = (UITextureColorFormat)(-1);
			for (int i = 0; i < count; i++)
			{
				ref DrawCall reference = ref drawCalls.Data[i];
				int vertexCount = (reference.RangeEnd - reference.RangeStart) * 6;
				propertyBlock.SetFloat(ShaderStrings.StartOffsetShaderPropertyId.MaterialId, reference.RangeStart);
				if (texture != reference.Texture)
				{
					propertyBlock.SetTexture(ShaderStrings.TexturePropertyId.MaterialId, reference.Texture);
					texture = reference.Texture;
				}
				if (uITextureFlags != reference.TextureFlags)
				{
					propertyBlock.SetInt(ShaderStrings.TextureFlagsPropertyId.MaterialId, (int)reference.TextureFlags);
					uITextureFlags = reference.TextureFlags;
				}
				if (uITextureColorFormat != reference.TextureColorFormat)
				{
					propertyBlock.SetInt(ShaderStrings.TextureColorFormatPropertyId.MaterialId, (int)reference.TextureColorFormat);
					uITextureColorFormat = reference.TextureColorFormat;
				}
				int passCount = reference.Material.passCount;
				for (int j = 0; j < passCount; j++)
				{
					commandBuffer.DrawProcedural(Matrix4x4.identity, reference.Material, j, MeshTopology.Triangles, vertexCount, 1, propertyBlock);
				}
				num += passCount;
			}
		}

		private void AllOpen()
		{
			indexedPrimitiveBuffer.Open();
			rectMaskIdsBuffer.Open();
			rectMasksBuffer.Open();
			materialPropertiesBuffer.Open();
		}

		private void AllClose()
		{
			materialPropertiesBuffer.Close();
			rectMasksBuffer.Close();
			rectMaskIdsBuffer.Close();
			indexedPrimitiveBuffer.Close();
		}

		private void AllSynchronise()
		{
			UIAtomContainer<UIPrimitiveRectData>.Global.SynchronizeComputeBuffer();
			UIAtomContainer<UIPrimitiveSquircleData>.Global.SynchronizeComputeBuffer();
			UIAtomContainer<UIPrimitiveCircleData>.Global.SynchronizeComputeBuffer();
			UIAtomContainer<UIPrimitiveGlyphData>.Global.SynchronizeComputeBuffer();
			UIAtomContainer<UIPrimitiveSectorData>.Global.SynchronizeComputeBuffer();
			UIAtomContainer<UIPrimitiveCurveData>.Global.SynchronizeComputeBuffer();
			UIAtomContainer<UIPrimitiveCurveSegmentData>.Global.SynchronizeComputeBuffer();
			UIAtomContainer<Matrix4x4>.Global.SynchronizeComputeBuffer();
			UIAtomContainer<uint>.Global.SynchronizeComputeBuffer();
		}

		private void AddBuffersToShaderGlobal()
		{
			rectMasksBuffer.AddToShaderGlobal(ShaderStrings.RectMasksBufferPropertyId);
			rectMaskIdsBuffer.AddToShaderGlobal(ShaderStrings.RectMaskIdsBufferPropertyId);
			materialPropertiesBuffer.AddToShaderGlobal(ShaderStrings.MaterialPropertiesBufferPropertyId);
			indexedPrimitiveBuffer.AddToShaderGlobal(ShaderStrings.DataBufferShaderPropertyId);
		}

		private int AppendToDrawCalls(Material material, Texture texture, UITextureFlags textureFlags, UITextureColorFormat textureColorFormat, int count)
		{
			if (lastDrawCallMaterial != material || lastDrawCallTexture != texture || drawCalls.Count == 0)
			{
				int num = drawCalls.Count + 1;
				if (drawCalls.Capacity <= num)
				{
					drawCalls.Reserve(num + 1000);
				}
				drawCalls.Data[drawCalls.Count] = new DrawCall(material, texture, textureFlags, textureColorFormat, indexedPrimitiveBuffer.Size, count);
				drawCalls.Count = num;
				lastDrawCallMaterial = material;
				lastDrawCallTexture = texture;
			}
			else
			{
				drawCalls.Data[drawCalls.Count - 1].RangeEnd += count;
			}
			int size = indexedPrimitiveBuffer.Size;
			int num2 = size + count;
			if (indexedPrimitiveBuffer.Capacity <= num2)
			{
				indexedPrimitiveBuffer.GrowFor(count);
				return size;
			}
			indexedPrimitiveBuffer.Size = num2;
			return size;
		}

		private void FlushPendingDrawCalls()
		{
			if (drawCalls.Count != 0)
			{
				EmitDrawCalls();
				drawCalls.Clear();
				lastDrawCallMaterial = null;
				lastDrawCallTexture = null;
			}
		}

		private void LoadMaterials()
		{
			UIRenderingManager instance = UIRenderingManager.Instance;
			instance.LoadIfNecessary();
			if (copyToGammaSpaceRenderTargetMaterial == null)
			{
				Shader copyTextureShader = instance.MaterialCollection.GetCopyTextureShader();
				copyToGammaSpaceRenderTargetMaterial = new Material(copyTextureShader);
				copyToGammaSpaceRenderTargetMaterial.EnableKeyword("_READ_LINEAR_TO_SRGB");
				copyToGammaSpaceRenderTargetMaterial.SetVector(ShaderStrings.SourcePositionId.MaterialId, new Vector4(0f, 0f, 1f, 1f));
				copyFromGammaSpaceToSDRRenderTargetMaterial = new Material(copyTextureShader);
				copyFromGammaSpaceToSDRRenderTargetMaterial.EnableKeyword("_WRITE_SRGB_TO_LINEAR");
				copyFromGammaSpaceToSDRRenderTargetMaterial.SetVector(ShaderStrings.SourcePositionId.MaterialId, new Vector4(0f, 0f, 1f, 1f));
				copyFromGammaSpaceToHDRRenderTargetMaterial = new Material(copyTextureShader);
				copyFromGammaSpaceToHDRRenderTargetMaterial.EnableKeyword("_WRITE_SRGB_TO_HDR");
				copyFromGammaSpaceToHDRRenderTargetMaterial.SetVector(ShaderStrings.SourcePositionId.MaterialId, new Vector4(0f, 0f, 1f, 1f));
			}
		}

		[Conditional("UNITY_EDITOR")]
		private void FixupLoadStatusIFN()
		{
			if (copyFromGammaSpaceToSDRRenderTargetMaterial == null || (bool)copyToGammaSpaceRenderTargetMaterial || copyFromGammaSpaceToSDRRenderTargetMaterial.shader == null || copyToGammaSpaceRenderTargetMaterial.shader == null || !copyFromGammaSpaceToSDRRenderTargetMaterial.shader.isSupported || !copyToGammaSpaceRenderTargetMaterial.shader.isSupported)
			{
				LoadMaterials();
			}
		}

		private int WriteMaterialPropertiesReturnIndex()
		{
			if (materialsStack.Count > 0)
			{
				activeMaterialProperties.Reset();
				int count = materialsStack.Count;
				for (int i = 0; i < count; i++)
				{
					activeMaterialProperties.ApplyPropertiesFrom(materialsStack.Data[i]);
				}
				int size = materialPropertiesBuffer.Size;
				activeMaterialProperties.WriteTo(ref materialPropertiesBuffer);
				return size;
			}
			return -1;
		}

		private void WriteActiveRectMasks()
		{
			activeRectMaskIdStart = rectMaskIdsBuffer.Size;
			rectMaskIdsBuffer.WriteRange(activeRectMaskIds);
		}

		public void Bezier(Matrix4x4 transform, Vector2 start, Vector2 control0, Vector2 control1, Vector2 end, float thickness, UITexture texture, UIBlendType blendType = UIBlendType.Standard)
		{
			Curve curve = new Curve(start, control0, control1, end);
			Curve(transform, curve, thickness, UIMaterialCollection.DefaultMaterialNameId, texture, Color.white, null, null, blendType);
		}

		public void Bezier(Matrix4x4 transform, Vector2 start, Vector2 control0, Vector2 control1, Vector2 end, float thickness, UITexture texture, Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Curve curve = new Curve(start, control0, control1, end);
			Curve(transform, curve, thickness, UIMaterialCollection.DefaultMaterialNameId, texture, color, null, null, blendType);
		}

		public void Bezier(Matrix4x4 transform, Vector2 start, Vector2 control0, Vector2 control1, Vector2 end, float thickness, StaticString materialName, UITexture texture, Color color, UIGradient gradient, UIBlendType blendType = UIBlendType.Standard)
		{
			Curve curve = new Curve(start, control0, control1, end);
			Curve(transform, curve, thickness, materialName, texture, color, gradient, null, blendType);
		}

		public void Line(Matrix4x4 transform, Vector2 start, Vector2 end, float thickness, UITexture texture)
		{
			Curve curve = new Curve(start, end);
			Curve(transform, curve, thickness, UIMaterialCollection.DefaultMaterialNameId, texture, Color.white, null);
		}

		public void Line(Matrix4x4 transform, Vector2 start, Vector2 end, float thickness, UITexture texture, Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Curve curve = new Curve(start, end);
			Curve(transform, curve, thickness, UIMaterialCollection.DefaultMaterialNameId, texture, color, null, null, blendType);
		}

		public void Line(Matrix4x4 transform, Vector2 start, Vector2 end, float thickness, StaticString materialName, UITexture texture, Color color, UIGradient gradient, UIBlendType blendType = UIBlendType.Standard)
		{
			Curve curve = new Curve(start, end);
			Curve(transform, curve, thickness, materialName, texture, color, gradient, null, blendType);
		}

		public void Polyline(Matrix4x4 transform, Vector2[] positions, float thickness, UITexture texture, UIBlendType blendType = UIBlendType.Standard)
		{
			Polyline(transform, positions, thickness, UIMaterialCollection.DefaultMaterialNameId, texture, Color.white, null, blendType);
		}

		public void Polyline(Matrix4x4 transform, Vector2[] positions, float thickness, UITexture texture, Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Polyline(transform, positions, thickness, UIMaterialCollection.DefaultMaterialNameId, texture, color, null, blendType);
		}

		public void Polyline(Matrix4x4 transform, Vector2[] positions, float thickness, StaticString materialName, UITexture texture, Color color, UIGradient gradient, UIBlendType blendType = UIBlendType.Standard)
		{
			int num = positions.Length;
			if (num > 0)
			{
				pathBuilder.Clear();
				pathBuilder.Start(positions[0]);
				for (int i = 1; i < num; i++)
				{
					pathBuilder.AbsoluteLineTo(positions[i]);
				}
				int count = pathBuilder.Curves.Count;
				for (int j = 0; j < count; j++)
				{
					Curve(transform, pathBuilder.Curves[j], thickness, materialName, texture, color, gradient, null, blendType);
				}
			}
		}

		public void Path(Matrix4x4 transform, ref PathBuilder path, float thickness, UITexture texture, UIBlendType blendType = UIBlendType.Standard)
		{
			Path(transform, ref path, thickness, UIMaterialCollection.DefaultMaterialNameId, texture, Color.white, null, blendType);
		}

		public void Path(Matrix4x4 transform, ref PathBuilder path, float thickness, UITexture texture, Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Path(transform, ref path, thickness, UIMaterialCollection.DefaultMaterialNameId, texture, color, null, blendType);
		}

		public void Path(Matrix4x4 transform, ref PathBuilder path, float thickness, StaticString materialName, UITexture texture, Color color, UIGradient gradient, UIBlendType blendType = UIBlendType.Standard)
		{
			if (path.Curves != null)
			{
				int count = path.Curves.Count;
				for (int i = 0; i < count; i++)
				{
					Curve(transform, path.Curves[i], thickness, materialName, texture, color, gradient, null, blendType);
				}
			}
		}

		public void Curve(Matrix4x4 transform, Curve curve, float thickness, StaticString materialName, UITexture texture, Color color, UIGradient gradient, MaterialPropertyBlock optionalMaterialPropertyBlock = null, UIBlendType blendType = UIBlendType.Standard)
		{
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Curve, blendType, materialName);
			if (material != null)
			{
				texture.RequestAsset();
				UIAtomId uIAtomId = UIAtomContainer<Matrix4x4>.AllocateTemporary(ref transform);
				int gradientId = gradient?.GetAtomId().Index ?? (-1);
				UIPrimitiveCurveData value = default(UIPrimitiveCurveData);
				value.Type = curve.Type;
				value.Start = curve.Start;
				value.End = curve.End;
				value.Control0 = curve.Control0;
				value.Control1 = curve.Control1;
				value.Texcoords = new Vector4(texture.Coordinates.x, texture.Coordinates.y, texture.Coordinates.width, texture.Coordinates.height);
				value.Abscissas.x = curve.StartAbscissa;
				value.Abscissas.y = curve.EndAbscissa;
				value.TransformId = uIAtomId.Index;
				value.GradientId = gradientId;
				value.Color = color;
				value.Thickness = thickness;
				UIAtomId curveId = UIAtomContainer<UIPrimitiveCurveData>.AllocateTemporary(ref value);
				UIPrimitiveCurveSegmentData data = default(UIPrimitiveCurveSegmentData);
				data.CurveId = curveId.Index;
				int num = Mathf.CeilToInt(curve.UpperBoundLength() / 5f * Mathf.Max(1f, thickness / 5f));
				UIAtomId id = UIAtomContainer<UIPrimitiveCurveSegmentData>.AllocateTemporary(num);
				for (int i = 0; i < num; i++)
				{
					float x = (float)i / (float)num;
					float y = (float)(i + 1) / (float)num;
					data.PositionOnCurve.x = x;
					data.PositionOnCurve.y = y;
					UIAtomContainer<UIPrimitiveCurveSegmentData>.SetData(ref id, i, ref data);
				}
				CurveAtomIds curveAtomIds = default(CurveAtomIds);
				curveAtomIds.CurveId = curveId;
				curveAtomIds.SegmentsId = id;
				curveAtomIds.SegmentCount = num;
				CurveAtomIds atomIds = curveAtomIds;
				Curve(ref atomIds, texture, material);
			}
		}

		public void Curve(ref CurveAtomIds atomIds, UITexture texture, StaticString materialName, UIBlendType blendType = UIBlendType.Standard)
		{
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Curve, blendType, materialName);
			Curve(ref atomIds, texture, material);
		}

		public void Curve(ref CurveAtomIds atomIds, UITexture texture, Material material)
		{
			activeRectMaskIdStart = rectMaskIdsBuffer.Size;
			rectMaskIdsBuffer.WriteRange(activeRectMaskIds);
			int num = AppendToDrawCalls(material, texture.GetAsset(), texture.Flags, texture.ColorFormat, atomIds.SegmentCount);
			for (int i = 0; i < atomIds.SegmentCount; i++)
			{
				int num2 = num + i;
				IndexedPrimitive[] storage = indexedPrimitiveBuffer.Storage;
				storage[num2].Index = atomIds.SegmentsId.Index + i;
				storage[num2].MaskIdStart = activeRectMaskIdStart;
				storage[num2].MaskIdCount = activeRectMaskIds.Count;
				storage[num2].MaterialPropertiesId = materialPropertiesIndex;
			}
		}

		public static void FillProjectionMatrixForOffscreenRender(float left, float right, float top, float bottom, out Matrix4x4 projection, out Matrix4x4 projectionInverse)
		{
			float num = -1000f;
			float num2 = 1000f;
			projection = Matrix4x4.zero;
			projection.m00 = 2f / (right - left);
			projection.m11 = 2f / (top - bottom);
			projection.m22 = -2f / (num2 - num);
			projection.m03 = (0f - (right + left)) / (right - left);
			projection.m13 = (top + bottom) / (bottom - top);
			projection.m23 = (0f - (num2 + num)) / (num2 - num);
			projection.m33 = 1f;
			projectionInverse = Matrix4x4.zero;
			projectionInverse.m00 = (right - left) / 2f;
			projectionInverse.m11 = (top - bottom) / 2f;
			projectionInverse.m22 = (0f - (num2 - num)) / 2f;
			projectionInverse.m03 = (0f - projection.m03) * projectionInverse.m00;
			projectionInverse.m13 = (0f - projection.m13) * projectionInverse.m11;
			projectionInverse.m23 = (0f - projection.m23) * projectionInverse.m22;
			projectionInverse.m33 = 1f;
		}

		public static void GetEnclosingVirtualCorners(ref Matrix4x4 localToGlobalMatrix, ref Rect localRect, out Vector2 topLeft, out Vector2 bottomRight)
		{
			localCorners[0] = new Vector2(localRect.xMin, localRect.yMin);
			localCorners[1] = new Vector2(localRect.xMax, localRect.yMin);
			localCorners[2] = new Vector2(localRect.xMin, localRect.yMax);
			localCorners[3] = new Vector2(localRect.xMax, localRect.yMax);
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
			for (int i = 0; i < localCorners.Length; i++)
			{
				Vector2 vector3 = localCorners[i];
				Vector4 vector4 = localToGlobalMatrix * new Vector4(vector3.x, vector3.y, 0f, 1f);
				vector.x = Mathf.Min(vector.x, vector4.x);
				vector.y = Mathf.Min(vector.y, vector4.y);
				vector2.x = Mathf.Max(vector2.x, vector4.x);
				vector2.y = Mathf.Max(vector2.y, vector4.y);
			}
			topLeft = vector;
			bottomRight = vector2;
		}

		public void Label(Matrix4x4 transform, string text, FontFamily fontFamily, uint fontSize, Color color, UIAtomId? optionalMaterialPropertyOverridesAtomId = null, UIBlendType blendType = UIBlendType.Standard)
		{
			Rect unscaledLocalRect = new Rect(0f, 0f, float.MaxValue, float.MaxValue);
			processedText.Clear();
			processedText.UnscaledLocalRect = unscaledLocalRect;
			processedText.RawText = text;
			processedText.FontFamily = fontFamily;
			processedText.UnscaledFontSize = fontSize;
			processedText.DefaultColor = color;
			processedText.UpdateView(view);
			processedText.UpdateLocalToGlobalMatrix(transform);
			Label(UIMaterialCollection.DefaultMaterialNameId, processedText, blendType);
		}

		public void Label(StaticString materialName, ProcessedText processedText, UIBlendType blendType = UIBlendType.Standard)
		{
			FontRenderingMode usedRenderingMode = processedText.UsedRenderingMode;
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Glyph, blendType, materialName, (usedRenderingMode == FontRenderingMode.DistanceField) ? DistanceFieldVariantName : StaticString.Empty);
			if (material != null)
			{
				Label(material, processedText, blendType);
			}
		}

		public void Label(Material material, ProcessedText processedText, UIBlendType blendType = UIBlendType.Standard)
		{
			UIAtomId glyphsAtomId;
			int glyphCount;
			int symbolCount;
			ProcessedText.Data[] glyphDataCache = processedText.GetGlyphDataCache(fontAtlas, out glyphsAtomId, out glyphCount, out symbolCount);
			if (glyphDataCache.Length != 0)
			{
				FontRenderingMode usedRenderingMode = processedText.UsedRenderingMode;
				Texture texture = fontAtlas.Texture(usedRenderingMode);
				activeRectMaskIdStart = rectMaskIdsBuffer.Size;
				rectMaskIdsBuffer.WriteRange(activeRectMaskIds);
				int startWriteIndex = AppendToDrawCalls(material, texture, UITextureFlags.AlphaStraight, UITextureColorFormat.Srgb, glyphCount);
				if (symbolCount != 0)
				{
					PushLabelWithSymbols(processedText.LocalToGlobalMatrix, glyphDataCache, ref indexedPrimitiveBuffer, startWriteIndex, glyphsAtomId, usedRenderingMode, blendType);
				}
				else
				{
					PushSubRangeLabel(glyphsAtomId, ref indexedPrimitiveBuffer, startWriteIndex, 0, glyphCount);
				}
			}
		}

		private void PushLabelWithSymbols(Matrix4x4 transform, ProcessedText.Data[] glyphs, ref DynamicWriteBuffer1D<IndexedPrimitive> writeBuffer, int startWriteIndex, UIAtomId glyphsAtomId, FontRenderingMode renderingMode, UIBlendType blendType = UIBlendType.Standard)
		{
			int num = -1;
			int num2 = -1;
			int num3 = startWriteIndex;
			int num4 = glyphs.Length;
			for (int i = 0; i < num4; i++)
			{
				if (!glyphs[i].IsImageSymbol)
				{
					if (num == -1)
					{
						num = i;
						num2 = 0;
					}
					num2++;
					continue;
				}
				if (num != -1)
				{
					PushSubRangeLabel(glyphsAtomId, ref writeBuffer, num3, num, num2);
					num3 += num2;
					num = -1;
					num2 = -1;
				}
				ref ProcessedText.Data reference = ref glyphs[i];
				SymbolToImageMapper symbolToImageMapper = UIRenderingManager.Instance.SymbolMapperController.GetSymbolMapper(reference.SymbolMapperId) as SymbolToImageMapper;
				if (symbolToImageMapper != null)
				{
					Matrix4x4 transform2;
					Rect position;
					if (renderingMode == FontRenderingMode.DistanceField)
					{
						transform2 = transform;
						position = new Rect(reference.Position.x, reference.Position.y, reference.Position.z, reference.Position.w);
					}
					else
					{
						transform2 = Matrix4x4.identity;
						position = new Rect(reference.Position.x, reference.Position.y, reference.Position.z, reference.Position.w);
						position = view.ScreenRectToStandardizedRect(position);
					}
					symbolToImageMapper.Render(this, transform2, position, reference.Color, blendType);
				}
			}
			if (num != -1)
			{
				PushSubRangeLabel(glyphsAtomId, ref writeBuffer, num3, num, num2);
			}
		}

		private void PushSubRangeLabel(UIAtomId glyphsAtomId, ref DynamicWriteBuffer1D<IndexedPrimitive> writeBuffer, int startWriteIndex, int readStartIndex, int count)
		{
			IndexedPrimitive[] storage = writeBuffer.Storage;
			int maskIdStart = activeRectMaskIdStart;
			int count2 = activeRectMaskIds.Count;
			for (int i = 0; i < count; i++)
			{
				int num = readStartIndex + i;
				int num2 = startWriteIndex + i;
				storage[num2].Index = glyphsAtomId.Index + num;
				storage[num2].MaskIdStart = maskIdStart;
				storage[num2].MaskIdCount = count2;
				storage[num2].MaterialPropertiesId = materialPropertiesIndex;
			}
		}

		public void Rect(ref UIAtomId transform, ref Rect position, UITexture texture, UIBlendType blendType = UIBlendType.Standard)
		{
			Color color = Color.white;
			Rect(ref transform, ref position, UIMaterialCollection.DefaultMaterialNameId, texture, ref AffineTransform2d.Identity, ref color, null, blendType);
		}

		public void Rect(ref UIAtomId transform, ref Rect position, UITexture texture, ref Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Rect(ref transform, ref position, UIMaterialCollection.DefaultMaterialNameId, texture, ref AffineTransform2d.Identity, ref color, null, blendType);
		}

		public void Rect(ref UIAtomId transform, ref Rect position, UITexture texture, ref AffineTransform2d coordinatesTransform, ref Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Rect(ref transform, ref position, UIMaterialCollection.DefaultMaterialNameId, texture, ref coordinatesTransform, ref color, null, blendType);
		}

		public void Rect(ref UIAtomId transform, ref Rect position, StaticString materialName, UITexture texture, ref AffineTransform2d coordinatesTransform, ref Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Rect(ref transform, ref position, materialName, texture, ref coordinatesTransform, ref color, null, blendType);
		}

		public void Rect(ref UIAtomId transform, ref Rect position, StaticString materialName, UITexture texture, ref AffineTransform2d coordinatesTransform, ref Color color, UIGradient gradient, UIBlendType blendType = UIBlendType.Standard)
		{
			MaterialPropertyFieldInfo[] materialPropertiesInfo;
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Rect, blendType, materialName, out materialPropertiesInfo);
			Rect(ref transform, ref position, FillMode.Fill, -1f, material, texture, ref coordinatesTransform, ref color, gradient);
		}

		public void Rect(ref UIAtomId transform, ref Rect position, StaticString materialName, UITexture texture, ref AffineTransform2d coordinatesTransform, ref Color color, UIGradient gradient, ref UIMaterialPropertyOverrides materialPropertyOverrides, UIBlendType blendType = UIBlendType.Standard)
		{
			MaterialPropertyFieldInfo[] materialPropertiesInfo;
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Rect, blendType, materialName, out materialPropertiesInfo);
			UIAtomId atomId = materialPropertyOverrides.GetAtomId(materialPropertiesInfo);
			Rect(ref transform, ref position, FillMode.Fill, -1f, material, texture, ref coordinatesTransform, ref color, gradient, atomId);
		}

		public void Rect(ref UIAtomId transform, ref Rect position, Material material, UITexture texture, ref AffineTransform2d coordinatesTransform, ref Color color, UIGradient gradient, UIAtomId? optionalMaterialPropertyOverridesAtomId = null)
		{
			Rect(ref transform, ref position, FillMode.Fill, -1f, material, texture, ref coordinatesTransform, ref color, gradient, optionalMaterialPropertyOverridesAtomId);
		}

		public void Rect(ref UIAtomId transform, ref Rect position, FillMode fillMode, float strokeWidth, Material material, UITexture texture, ref AffineTransform2d coordinatesTransform, ref Color color, UIGradient gradient, UIAtomId? optionalMaterialPropertyOverridesAtomId = null)
		{
			texture.RequestAsset();
			int gradientId = gradient?.GetAtomId().Index ?? (-1);
			UIPrimitiveRectData value = default(UIPrimitiveRectData);
			value.Position = position;
			value.Texcoords = texture.Coordinates;
			value.Color = color;
			value.TexTransform = coordinatesTransform;
			value.TexSlicingSize = default(RectMargins);
			value.TexSlicingUvs = default(RectMargins);
			value.TransformId = transform.Index;
			value.GradientId = gradientId;
			value.StrokeWidth = ((fillMode == FillMode.Fill) ? (-1f) : strokeWidth);
			value.MaterialPropertyOverridesId = (optionalMaterialPropertyOverridesAtomId.HasValue ? optionalMaterialPropertyOverridesAtomId.Value.Index : (-1));
			value.Flags = 0u;
			UIAtomId rectId = UIAtomContainer<UIPrimitiveRectData>.AllocateTemporary(ref value);
			Rect(ref rectId, texture, material);
		}

		public void Rect(ref UIAtomId rectId)
		{
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Rect, UIBlendType.Standard);
			Rect(ref rectId, UITexture.White, material);
		}

		public void Rect(ref UIAtomId rectId, UITexture texture, StaticString materialName, UIBlendType blendType = UIBlendType.Standard)
		{
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Rect, blendType, materialName);
			Rect(ref rectId, texture, material);
		}

		public void Rect(ref UIAtomId rectId, UITexture texture, Material material)
		{
			activeRectMaskIdStart = rectMaskIdsBuffer.Size;
			rectMaskIdsBuffer.WriteRange(activeRectMaskIds);
			Texture texture2 = (texture.Texture ? texture.Texture : texture.GetAsset());
			int num = AppendToDrawCalls(material, texture2, texture.Flags, texture.ColorFormat, 1);
			IndexedPrimitive[] storage = indexedPrimitiveBuffer.Storage;
			storage[num].Index = rectId.Index;
			storage[num].MaskIdStart = activeRectMaskIdStart;
			storage[num].MaskIdCount = activeRectMaskIds.Count;
			storage[num].MaterialPropertiesId = materialPropertiesIndex;
		}

		public void Sector(Matrix4x4 transform, Rect position, float angularPosition, float angularThickness, float radialThickness, UITexture texture, UIBlendType blendType = UIBlendType.Standard)
		{
			Sector(transform, position, angularPosition, angularThickness, radialThickness, UIMaterialCollection.DefaultMaterialNameId, texture, Color.white, null, null, blendType);
		}

		public void Sector(Matrix4x4 transform, Rect position, float angularPosition, float angularThickness, float radialThickness, UITexture texture, Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Sector(transform, position, angularPosition, angularThickness, radialThickness, UIMaterialCollection.DefaultMaterialNameId, texture, color, null, null, blendType);
		}

		public void Sector(Matrix4x4 transform, Rect position, float angularPosition, float angularThickness, float radialThickness, StaticString materialName, UITexture texture, Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Sector(transform, position, angularPosition, angularThickness, radialThickness, materialName, texture, color, null, null, blendType);
		}

		public void Sector(Matrix4x4 transform, Rect position, float angularPosition, float angularThickness, float radialThickness, StaticString materialName, UITexture texture, Color color, UIGradient gradient, MaterialPropertyBlock optionalMaterialPropertyBlock = null, UIBlendType blendType = UIBlendType.Standard)
		{
			if (angularThickness == 0f || radialThickness <= 0f)
			{
				return;
			}
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Sector, blendType, materialName);
			if (material != null)
			{
				texture.RequestAsset();
				UIAtomId uIAtomId = UIAtomContainer<Matrix4x4>.AllocateTemporary(ref transform);
				UIPrimitiveSectorData data = default(UIPrimitiveSectorData);
				data.Position = new Vector4(position.x, position.y, position.width, position.height);
				data.Texcoords = new Vector4(texture.Coordinates.x, texture.Coordinates.y, texture.Coordinates.width, texture.Coordinates.height);
				data.Color = color;
				data.TransformId = uIAtomId.Index;
				int num = (data.GradientId = gradient?.GetAtomId().Index ?? (-1));
				data.RadialThickness = radialThickness;
				float num2 = (float)Math.PI / 15f;
				float num3 = Mathf.Abs(angularThickness);
				int num4 = Mathf.CeilToInt(num3 / num2);
				UIAtomId id = UIAtomContainer<UIPrimitiveSectorData>.AllocateTemporary(num4);
				float num5 = ((angularThickness < 0f) ? (angularPosition + angularThickness) : angularPosition);
				for (int i = 0; i < num4 - 1; i++)
				{
					data.AngularPosition = num5 + (float)i * num2;
					data.AngularThickness = num2;
					UIAtomContainer<UIPrimitiveSectorData>.SetData(ref id, i, ref data);
				}
				data.AngularPosition = num5 + (float)(num4 - 1) * num2;
				data.AngularThickness = num5 + num3 - data.AngularPosition;
				UIAtomContainer<UIPrimitiveSectorData>.SetData(ref id, num4 - 1, ref data);
				Sector(id, num4, texture, material);
			}
		}

		public void Sector(UIAtomId slicesId, int sliceCount, UITexture texture, StaticString materialName, UIBlendType blendType = UIBlendType.Standard)
		{
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Curve, blendType, materialName);
			Sector(slicesId, sliceCount, texture, material);
		}

		public void Sector(UIAtomId slicesId, int sliceCount, UITexture texture, Material material)
		{
			activeRectMaskIdStart = rectMaskIdsBuffer.Size;
			rectMaskIdsBuffer.WriteRange(activeRectMaskIds);
			Texture texture2 = (texture.Texture ? texture.Texture : texture.GetAsset());
			int num = AppendToDrawCalls(material, texture2, texture.Flags, texture.ColorFormat, sliceCount);
			for (int i = 0; i < sliceCount; i++)
			{
				int num2 = num + i;
				IndexedPrimitive[] storage = indexedPrimitiveBuffer.Storage;
				storage[num2].Index = slicesId.Index + i;
				storage[num2].MaskIdStart = activeRectMaskIdStart;
				storage[num2].MaskIdCount = activeRectMaskIds.Count;
				storage[num2].MaterialPropertiesId = materialPropertiesIndex;
			}
		}

		public void Squircle(ref UIAtomId transform, ref Rect position, float cornersRadius, UITexture texture, UIBlendType blendType = UIBlendType.Standard)
		{
			Color color = Color.white;
			Squircle(ref transform, ref position, cornersRadius, cornersRadius, cornersRadius, cornersRadius, FillMode.Fill, -1f, UIMaterialCollection.DefaultMaterialNameId, texture, ref AffineTransform2d.Identity, ref color, null, blendType);
		}

		public void Squircle(ref UIAtomId transform, ref Rect position, float cornersRadius, UITexture texture, ref Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Squircle(ref transform, ref position, cornersRadius, cornersRadius, cornersRadius, cornersRadius, FillMode.Fill, -1f, UIMaterialCollection.DefaultMaterialNameId, texture, ref AffineTransform2d.Identity, ref color, null, blendType);
		}

		public void Squircle(ref UIAtomId transform, ref Rect position, float cornersRadius, UITexture texture, float strokeWidth, UIBlendType blendType = UIBlendType.Standard)
		{
			Color color = Color.white;
			Squircle(ref transform, ref position, cornersRadius, cornersRadius, cornersRadius, cornersRadius, FillMode.Stroke, strokeWidth, UIMaterialCollection.DefaultMaterialNameId, texture, ref AffineTransform2d.Identity, ref color, null, blendType);
		}

		public void Squircle(ref UIAtomId transform, ref Rect position, float cornersRadius, UITexture texture, ref Color color, float strokeWidth, UIBlendType blendType = UIBlendType.Standard)
		{
			Squircle(ref transform, ref position, cornersRadius, cornersRadius, cornersRadius, cornersRadius, FillMode.Stroke, strokeWidth, UIMaterialCollection.DefaultMaterialNameId, texture, ref AffineTransform2d.Identity, ref color, null, blendType);
		}

		public void Squircle(ref UIAtomId transform, ref Rect position, float topLeftRadius, float topRightRadius, float bottomLeftRadius, float bottomRightRadius, UITexture texture, ref Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Squircle(ref transform, ref position, topLeftRadius, topRightRadius, bottomLeftRadius, bottomRightRadius, FillMode.Fill, -1f, UIMaterialCollection.DefaultMaterialNameId, texture, ref AffineTransform2d.Identity, ref color, null, blendType);
		}

		public void Squircle(ref UIAtomId transform, ref Rect position, float topLeftRadius, float topRightRadius, float bottomLeftRadius, float bottomRightRadius, FillMode fillMode, float strokeWidth, StaticString materialName, UITexture texture, ref AffineTransform2d coordinatesTransform, ref Color color, UIBlendType blendType = UIBlendType.Standard)
		{
			Squircle(ref transform, ref position, topLeftRadius, topRightRadius, bottomLeftRadius, bottomRightRadius, fillMode, strokeWidth, materialName, texture, ref coordinatesTransform, ref color, null, blendType);
		}

		public void Squircle(ref UIAtomId transform, ref Rect position, float topLeftRadius, float topRightRadius, float bottomLeftRadius, float bottomRightRadius, FillMode fillMode, float strokeWidth, StaticString materialName, UITexture texture, ref AffineTransform2d coordinatesTransform, ref Color color, UIGradient gradient, UIBlendType blendType = UIBlendType.Standard)
		{
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Squircle, blendType, materialName);
			Squircle(ref transform, ref position, topLeftRadius, topRightRadius, bottomLeftRadius, bottomRightRadius, fillMode, strokeWidth, material, texture, ref coordinatesTransform, ref color, gradient);
		}

		public void Squircle(ref UIAtomId transform, ref Rect position, float topLeftRadius, float topRightRadius, float bottomLeftRadius, float bottomRightRadius, FillMode fillMode, float strokeWidth, StaticString materialName, UITexture texture, ref AffineTransform2d coordinatesTransform, ref Color color, UIGradient gradient, ref UIMaterialPropertyOverrides materialPropertyOverrides, UIBlendType blendType = UIBlendType.Standard)
		{
			MaterialPropertyFieldInfo[] materialPropertiesInfo;
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Squircle, blendType, materialName, out materialPropertiesInfo);
			UIAtomId atomId = materialPropertyOverrides.GetAtomId(materialPropertiesInfo);
			Squircle(ref transform, ref position, topLeftRadius, topRightRadius, bottomLeftRadius, bottomRightRadius, fillMode, strokeWidth, material, texture, ref coordinatesTransform, ref color, gradient, atomId);
		}

		public void Squircle(ref UIAtomId transform, ref Rect position, float topLeftRadius, float topRightRadius, float bottomLeftRadius, float bottomRightRadius, FillMode fillMode, float strokeWidth, Material material, UITexture texture, ref AffineTransform2d coordinatesTransform, ref Color color, UIGradient gradient, UIAtomId? optionalMaterialPropertyOverridesAtomId = null)
		{
			texture.RequestAsset();
			int gradientId = gradient?.GetAtomId().Index ?? (-1);
			UIPrimitiveSquircleData value = default(UIPrimitiveSquircleData);
			value.Position = position;
			value.Texcoords = texture.Coordinates;
			value.Color = color;
			value.TexTransform = coordinatesTransform;
			value.TransformId = transform.Index;
			value.GradientId = gradientId;
			value.CornerRadiuses = new Vector4(topLeftRadius, topRightRadius, bottomLeftRadius, bottomRightRadius);
			value.StrokeWidth = ((fillMode == FillMode.Fill) ? (-1f) : strokeWidth);
			value.MaterialPropertyOverridesId = (optionalMaterialPropertyOverridesAtomId.HasValue ? optionalMaterialPropertyOverridesAtomId.Value.Index : (-1));
			UIAtomId squircleId = UIAtomContainer<UIPrimitiveSquircleData>.AllocateTemporary(ref value);
			Squircle(ref squircleId, texture, material);
		}

		public void Squircle(ref UIAtomId squircleId)
		{
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Squircle, UIBlendType.Standard);
			Squircle(ref squircleId, UITexture.White, material);
		}

		public void Squircle(ref UIAtomId squircleId, UITexture texture, StaticString materialName, UIBlendType blendType = UIBlendType.Standard)
		{
			Material material = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Squircle, blendType, materialName);
			Squircle(ref squircleId, texture, material);
		}

		public void Squircle(ref UIAtomId squircleId, UITexture texture, Material material)
		{
			activeRectMaskIdStart = rectMaskIdsBuffer.Size;
			rectMaskIdsBuffer.WriteRange(activeRectMaskIds);
			Texture texture2 = (texture.Texture ? texture.Texture : texture.GetAsset());
			int num = AppendToDrawCalls(material, texture2, texture.Flags, texture.ColorFormat, 1);
			IndexedPrimitive[] storage = indexedPrimitiveBuffer.Storage;
			storage[num].Index = squircleId.Index;
			storage[num].MaskIdStart = activeRectMaskIdStart;
			storage[num].MaskIdCount = activeRectMaskIds.Count;
			storage[num].MaterialPropertiesId = materialPropertiesIndex;
		}
	}
}
