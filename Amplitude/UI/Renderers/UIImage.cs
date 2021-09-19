using System;
using System.Diagnostics;
using Amplitude.UI.Animations;
using Amplitude.UI.Styles.Scene;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[RequireComponent(typeof(UITransform))]
	[ExecuteInEditMode]
	public class UIImage : UIAbstractImage, IUIAnimationTarget
	{
		public enum TextureSlicingModeEnum
		{
			TransformSpace,
			TextureSpace,
			PreserveAspectRatio
		}

		public enum FloatValueMode
		{
			Percent,
			Pixels
		}

		[SerializeField]
		[UIMaterialId(UIPrimitiveType.Rect)]
		private UIMaterialId material;

		[SerializeField]
		private FillMode fillMode;

		[SerializeField]
		[Range(0f, 20f)]
		private float strokeWidth = 1f;

		[SerializeField]
		private TextureSlicingModeEnum textureSlicingMode = TextureSlicingModeEnum.TextureSpace;

		[SerializeField]
		private FloatValueMode textureSlicingUvValueMode;

		[SerializeField]
		private RectMargins textureSlicingUvs;

		[SerializeField]
		private RectMargins textureSlicingSize = new RectMargins(1f, 1f, 1f, 1f);

		[NonSerialized]
		private UIAtomId rectAtomId = UIAtomId.Invalid;

		[NonSerialized]
		private Material resolvedMaterial;

		public FillMode FillMode
		{
			get
			{
				return fillMode;
			}
			set
			{
				if (fillMode != value)
				{
					fillMode = value;
					dirtyness = 2;
				}
			}
		}

		public float StrokeWidth
		{
			get
			{
				return strokeWidth;
			}
			set
			{
				if (strokeWidth != value)
				{
					strokeWidth = value;
					dirtyness = 2;
				}
			}
		}

		public RectMargins TextureSlicingUvs
		{
			get
			{
				return textureSlicingUvs;
			}
			set
			{
				if (textureSlicingUvs != value)
				{
					textureSlicingUvs = value;
					dirtyness = 2;
				}
			}
		}

		public RectMargins TextureSlicingSize
		{
			get
			{
				return textureSlicingSize;
			}
			set
			{
				if (textureSlicingSize != value)
				{
					textureSlicingSize = value;
					dirtyness = 2;
				}
			}
		}

		public override UIMaterialId Material
		{
			get
			{
				return material;
			}
			set
			{
				material = value;
				dirtyness = 2;
			}
		}

		protected override void Render(UIPrimitiveDrawer drawer)
		{
			if (base.Color.a != 0f)
			{
				if (dirtyness == 2)
				{
					UpdateRenderParameters();
				}
				else if (dirtyness == 1)
				{
					Rect position = ComputeLocalRect();
					UIPrimitiveRectData data = UIAtomContainer<UIPrimitiveRectData>.GetData(ref rectAtomId);
					data.Position = position;
					UIAtomContainer<UIPrimitiveRectData>.SetData(ref rectAtomId, ref data);
					dirtyness = 0;
				}
				drawer.Rect(ref rectAtomId, texture, resolvedMaterial);
			}
		}

		protected override void Unload()
		{
			if (rectAtomId.IsValid)
			{
				UIAtomContainer<UIPrimitiveRectData>.Deallocate(ref rectAtomId);
			}
			base.Unload();
		}

		protected override void OnTransformVisibleGloballyChanged(bool previouslyVisible, bool currentlyVisible)
		{
			base.OnTransformVisibleGloballyChanged(previouslyVisible, currentlyVisible);
			if (currentlyVisible)
			{
				texture.RequestAsset();
				dirtyness = 2;
			}
		}

		protected override void OnTransformLocalRectChanged()
		{
			dirtyness = ((dirtyness < 1) ? 1 : dirtyness);
		}

		protected void UpdateRenderParameters()
		{
			texture.RequestAsset();
			StaticString x = material.Id;
			if (StaticString.IsNullOrEmpty(x))
			{
				x = UIMaterialCollection.DefaultMaterialNameId;
			}
			resolvedMaterial = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Rect, blendType, x, out var materialPropertiesInfo);
			UIPrimitiveRectData value = default(UIPrimitiveRectData);
			Rect rect = (value.Position = ComputeLocalRect());
			value.Texcoords = texture.Coordinates;
			value.Color = base.Color;
			value.TexTransform = base.TextureTransformation;
			value.TexSlicingSize = textureSlicingSize;
			value.TexSlicingUvs = textureSlicingUvs;
			value.TransformId = UITransform.MatrixAtomId.Index;
			value.GradientId = ((base.Gradient != null) ? base.Gradient.GetAtomId().Index : (-1));
			value.StrokeWidth = ((fillMode == FillMode.Fill) ? (-1f) : strokeWidth);
			value.MaterialPropertyOverridesId = GetMaterialPropertiesAtomId(materialPropertiesInfo).Index;
			value.Flags = 0u;
			value.SetSlicingMode(textureSlicingMode);
			if (rectAtomId.IsValid)
			{
				UIAtomContainer<UIPrimitiveRectData>.SetData(ref rectAtomId, ref value);
			}
			else
			{
				rectAtomId = UIAtomContainer<UIPrimitiveRectData>.Allocate(ref value);
			}
			dirtyness = 0;
		}

		[MutatorsProvider]
		private static void LoadStyles(MutatorsReceiver receiver)
		{
			receiver.Add(Mutators.ImageStrokeWidth);
		}

		[Conditional("UNITY_EDITOR")]
		private void EditorRenderSlicingLines(UIPrimitiveDrawer drawer)
		{
		}

		private RectMargins ComputeLocalMargins()
		{
			RectMargins result = default(RectMargins);
			Rect localRect = UITransform.LocalRect;
			Vector2 widthHeight = texture.WidthHeight;
			switch (textureSlicingMode)
			{
			case TextureSlicingModeEnum.TransformSpace:
				result = textureSlicingSize;
				break;
			case TextureSlicingModeEnum.PreserveAspectRatio:
			{
				Vector2 vector = new Vector2(localRect.height * texture.Aspect, localRect.width / texture.Aspect);
				result.Left = textureSlicingUvs.Left * vector.x;
				result.Right = textureSlicingUvs.Right * vector.x;
				result.Top = textureSlicingUvs.Top * vector.y;
				result.Bottom = textureSlicingUvs.Bottom * vector.y;
				break;
			}
			case TextureSlicingModeEnum.TextureSpace:
				result.Left = textureSlicingSize.Left * textureSlicingUvs.Left * widthHeight.x;
				result.Right = textureSlicingSize.Right * textureSlicingUvs.Right * widthHeight.x;
				result.Top = textureSlicingSize.Top * textureSlicingUvs.Top * widthHeight.y;
				result.Bottom = textureSlicingSize.Bottom * textureSlicingUvs.Bottom * widthHeight.y;
				break;
			}
			result.Left = localRect.xMin + result.Left;
			result.Right = localRect.xMax - result.Right;
			result.Top = localRect.yMin + result.Top;
			result.Bottom = localRect.yMax - result.Bottom;
			return result;
		}
	}
}
