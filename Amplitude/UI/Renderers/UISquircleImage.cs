using System;
using Amplitude.UI.Animations;
using Amplitude.UI.Styles;
using Amplitude.UI.Styles.Scene;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[ExecuteInEditMode]
	public class UISquircleImage : UIAbstractImage
	{
		public new static class Mutators
		{
			public static readonly MutatorSet<UISquircleImage, float> TopLeftCornerRadius = new MutatorSet<UISquircleImage, float>(ItemIdentifiers.TopLeftCornerRadius, (UISquircleImage t) => t.topLeftRadius, delegate(UISquircleImage t, float value)
			{
				t.TopLeftRadius = value;
			});

			public static readonly MutatorSet<UISquircleImage, float> TopRightCornerRadius = new MutatorSet<UISquircleImage, float>(ItemIdentifiers.TopRightCornerRadius, (UISquircleImage t) => t.topRightRadius, delegate(UISquircleImage t, float value)
			{
				t.TopRightRadius = value;
			});

			public static readonly MutatorSet<UISquircleImage, float> BottomLeftCornerRadius = new MutatorSet<UISquircleImage, float>(ItemIdentifiers.BottomLeftCornerRadius, (UISquircleImage t) => t.bottomLeftRadius, delegate(UISquircleImage t, float value)
			{
				t.BottomLeftRadius = value;
			});

			public static readonly MutatorSet<UISquircleImage, float> BottomRightCornerRadius = new MutatorSet<UISquircleImage, float>(ItemIdentifiers.BottomRightCornerRadius, (UISquircleImage t) => t.bottomRightRadius, delegate(UISquircleImage t, float value)
			{
				t.BottomRightRadius = value;
			});
		}

		[SerializeField]
		[UIMaterialId(UIPrimitiveType.Squircle)]
		private UIMaterialId material;

		[SerializeField]
		private float topLeftRadius;

		[SerializeField]
		private float topRightRadius;

		[SerializeField]
		private float bottomLeftRadius;

		[SerializeField]
		private float bottomRightRadius;

		[SerializeField]
		private FillMode fillMode;

		[SerializeField]
		[Range(0f, 20f)]
		private float strokeWidth;

		[NonSerialized]
		private UIAtomId squircleAtomId = UIAtomId.Invalid;

		[NonSerialized]
		private Material resolvedMaterial;

		public float TopLeftRadius
		{
			get
			{
				return topLeftRadius;
			}
			set
			{
				if (topLeftRadius != value)
				{
					topLeftRadius = value;
					dirtyness = 2;
				}
			}
		}

		public float TopRightRadius
		{
			get
			{
				return topRightRadius;
			}
			set
			{
				if (topRightRadius != value)
				{
					topRightRadius = value;
					dirtyness = 2;
				}
			}
		}

		public float BottomLeftRadius
		{
			get
			{
				return bottomLeftRadius;
			}
			set
			{
				if (bottomLeftRadius != value)
				{
					bottomLeftRadius = value;
					dirtyness = 2;
				}
			}
		}

		public float BottomRightRadius
		{
			get
			{
				return bottomRightRadius;
			}
			set
			{
				if (bottomRightRadius != value)
				{
					bottomRightRadius = value;
					dirtyness = 2;
				}
			}
		}

		[SerializeField]
		public FillMode Fill
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

		public override void CreateAnimationItems(IUIAnimationItemsCollection animationItemsCollection)
		{
			base.CreateAnimationItems(animationItemsCollection);
			animationItemsCollection.Add(Mutators.TopLeftCornerRadius);
			animationItemsCollection.Add(Mutators.TopRightCornerRadius);
			animationItemsCollection.Add(Mutators.BottomLeftCornerRadius);
			animationItemsCollection.Add(Mutators.BottomRightCornerRadius);
		}

		protected override void Unload()
		{
			if (squircleAtomId.IsValid)
			{
				UIAtomContainer<UIPrimitiveSquircleData>.Deallocate(ref squircleAtomId);
			}
			base.Unload();
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
					UIPrimitiveSquircleData data = UIAtomContainer<UIPrimitiveSquircleData>.GetData(ref squircleAtomId);
					data.Position = position;
					UIAtomContainer<UIPrimitiveSquircleData>.SetData(ref squircleAtomId, ref data);
					dirtyness = 0;
				}
				drawer.Squircle(ref squircleAtomId, texture, resolvedMaterial);
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

		protected void UpdateRenderParameters()
		{
			texture.RequestAsset();
			resolvedMaterial = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Squircle, blendType, material.Id, out var materialPropertiesInfo);
			UIPrimitiveSquircleData data = default(UIPrimitiveSquircleData);
			data.Position = ComputeLocalRect();
			data.Texcoords = texture.Coordinates;
			data.Color = base.Color;
			data.TexTransform = base.TextureTransformation;
			data.TransformId = UITransform.MatrixAtomId.Index;
			data.GradientId = ((base.Gradient != null) ? base.Gradient.GetAtomId().Index : (-1));
			data.CornerRadiuses = new Vector4(topLeftRadius, topRightRadius, bottomLeftRadius, bottomRightRadius);
			data.StrokeWidth = ((fillMode == FillMode.Fill) ? (-1f) : strokeWidth);
			data.MaterialPropertyOverridesId = GetMaterialPropertiesAtomId(materialPropertiesInfo).Index;
			ComputeLocalRect();
			if (squircleAtomId.IsValid)
			{
				UIAtomContainer<UIPrimitiveSquircleData>.SetData(ref squircleAtomId, ref data);
			}
			else
			{
				squircleAtomId = UIAtomContainer<UIPrimitiveSquircleData>.Allocate(ref data);
			}
			dirtyness = 0;
		}

		[MutatorsProvider]
		private static void LoadStyles(MutatorsReceiver receiver)
		{
			receiver.Add(Mutators.TopLeftCornerRadius);
			receiver.Add(Mutators.TopRightCornerRadius);
			receiver.Add(Mutators.BottomLeftCornerRadius);
			receiver.Add(Mutators.BottomRightCornerRadius);
			receiver.Add(UIAbstractImage.Mutators.SquircleImageStrokeWidth);
		}
	}
}
