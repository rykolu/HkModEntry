using System;
using Amplitude.UI.Styles.Scene;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[ExecuteInEditMode]
	public class UIRoundImage : UIAbstractImage
	{
		[SerializeField]
		[UIMaterialId(UIPrimitiveType.Circle)]
		private UIMaterialId material;

		[SerializeField]
		private FillMode fillMode;

		[SerializeField]
		[Range(0f, 20f)]
		private float strokeWidth = 1f;

		[NonSerialized]
		private UIAtomId circleAtomId = UIAtomId.Invalid;

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
					UIPrimitiveCircleData data = UIAtomContainer<UIPrimitiveCircleData>.GetData(ref circleAtomId);
					data.Position = position;
					UIAtomContainer<UIPrimitiveCircleData>.SetData(ref circleAtomId, ref data);
					dirtyness = 0;
				}
				drawer.Circle(ref circleAtomId, texture, resolvedMaterial);
			}
		}

		protected override void OnTransformVisibleGloballyChanged(bool previouslyVisible, bool currentlyVisible)
		{
			base.OnTransformVisibleGloballyChanged(previouslyVisible, currentlyVisible);
			if (currentlyVisible)
			{
				dirtyness = 2;
			}
		}

		protected override void OnTransformLocalRectChanged()
		{
			dirtyness = ((dirtyness < 1) ? 1 : dirtyness);
		}

		protected override void Unload()
		{
			if (circleAtomId.IsValid)
			{
				UIAtomContainer<UIPrimitiveCircleData>.Deallocate(ref circleAtomId);
			}
			base.Unload();
		}

		protected void UpdateRenderParameters()
		{
			texture.RequestAsset();
			resolvedMaterial = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Circle, blendType, material.Id, out var materialPropertiesInfo);
			UIPrimitiveCircleData value = default(UIPrimitiveCircleData);
			Rect rect = (value.Position = ComputeLocalRect());
			value.Texcoords = texture.Coordinates;
			value.Color = base.Color;
			value.TexTransform = base.TextureTransformation;
			value.TransformId = UITransform.MatrixAtomId.Index;
			value.GradientId = ((base.Gradient != null) ? base.Gradient.GetAtomId().Index : (-1));
			value.StrokeWidth = ((fillMode == FillMode.Fill) ? (-1f) : strokeWidth);
			value.MaterialPropertyOverridesId = GetMaterialPropertiesAtomId(materialPropertiesInfo).Index;
			if (circleAtomId.IsValid)
			{
				UIAtomContainer<UIPrimitiveCircleData>.SetData(ref circleAtomId, ref value);
			}
			else
			{
				circleAtomId = UIAtomContainer<UIPrimitiveCircleData>.Allocate(ref value);
			}
			dirtyness = 0;
		}

		[MutatorsProvider]
		private static void LoadStyles(MutatorsReceiver receiver)
		{
			receiver.Add(Mutators.RoundImageStrokeWidth);
		}
	}
}
