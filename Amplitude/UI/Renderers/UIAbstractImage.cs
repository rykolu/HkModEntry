using System;
using Amplitude.UI.Animations;
using Amplitude.UI.Styles;
using Amplitude.UI.Styles.Scene;
using Amplitude.UI.Traits;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[RequireComponent(typeof(UITransform))]
	public abstract class UIAbstractImage : UIRenderer, IUIMaterialPropertyOverridesProvider, IUITraitColor, IUITrait<Color>, IUITraitTexture, IUITrait<UITexture>, IUITexturesReloaded
	{
		public enum AspectRatioOptionEnum
		{
			Default,
			PreserveWidth,
			PreserveHeight
		}

		public static class Mutators
		{
			public static readonly MutatorSet<UIAbstractImage, UITexture> Texture = new MutatorSet<UIAbstractImage, UITexture>(ItemIdentifiers.Texture, (UIAbstractImage t) => t.TextureRef, delegate(UIAbstractImage t, UITexture value)
			{
				t.SetTexture(ref value);
			});

			public static readonly MutatorSet<UIAbstractImage, Color> Color = new MutatorSet<UIAbstractImage, Color>(ItemIdentifiers.Color, (UIAbstractImage t) => t.Color, delegate(UIAbstractImage t, Color value)
			{
				t.Color = value;
			});

			public static readonly MutatorSet<UIAbstractImage, UIGradient> Gradient = new MutatorSet<UIAbstractImage, UIGradient>(ItemIdentifiers.Gradient, (UIAbstractImage t) => t.Gradient, delegate(UIAbstractImage t, UIGradient value)
			{
				t.Gradient = value;
			});

			public static readonly MutatorSet<UIImage, float> ImageStrokeWidth = new MutatorSet<UIImage, float>(ItemIdentifiers.StrokeWidth, (UIImage t) => t.StrokeWidth, delegate(UIImage t, float value)
			{
				t.StrokeWidth = value;
			});

			public static readonly MutatorSet<UISquircleImage, float> SquircleImageStrokeWidth = new MutatorSet<UISquircleImage, float>(ItemIdentifiers.StrokeWidth, (UISquircleImage t) => t.StrokeWidth, delegate(UISquircleImage t, float value)
			{
				t.StrokeWidth = value;
			});

			public static readonly MutatorSet<UIRoundImage, float> RoundImageStrokeWidth = new MutatorSet<UIRoundImage, float>(ItemIdentifiers.StrokeWidth, (UIRoundImage t) => t.StrokeWidth, delegate(UIRoundImage t, float value)
			{
				t.StrokeWidth = value;
			});
		}

		protected const int NotDirty = 0;

		protected const int PositionAndBoundsDirty = 1;

		protected const int RenderParametersDirty = 2;

		[SerializeField]
		protected UIBlendType blendType;

		[NonSerialized]
		protected int dirtyness;

		[SerializeField]
		protected UITexture texture;

		[SerializeField]
		private Color color = Color.white;

		[SerializeField]
		private UIGradient.Reference gradient;

		[SerializeField]
		private Vector2 textureTranslation = Vector2.zero;

		[SerializeField]
		private Vector2 textureScale = Vector2.one;

		[SerializeField]
		private float textureRotation;

		[SerializeField]
		private Alignment alignment = Alignment.CenterCenter;

		[SerializeField]
		private bool stretch = true;

		[SerializeField]
		private AspectRatioOptionEnum aspectRatioOption;

		[SerializeField]
		private UIMaterialPropertyOverrides materialPropertyOverrides;

		public UIBlendType BlendType
		{
			get
			{
				return blendType;
			}
			set
			{
				blendType = value;
				dirtyness = 2;
			}
		}

		public ref UITexture TextureRef => ref texture;

		public Color Color
		{
			get
			{
				return color;
			}
			set
			{
				if (color != value)
				{
					color = value;
					dirtyness = 2;
				}
			}
		}

		public UIGradient Gradient
		{
			get
			{
				return gradient.GetValue();
			}
			set
			{
				if (value != gradient.GetValue())
				{
					gradient.SetValue(value);
					OnGradientChanged();
					dirtyness = 2;
				}
			}
		}

		public AffineTransform2d TextureTransformation
		{
			get
			{
				return new AffineTransform2d(textureTranslation, textureRotation, textureScale);
			}
			set
			{
				if (textureTranslation != value.Translation || textureRotation != value.Rotation || textureScale != value.Scale)
				{
					textureTranslation = value.Translation;
					textureRotation = value.Rotation;
					textureScale = value.Scale;
					dirtyness = 2;
				}
			}
		}

		public Vector2 TextureTranslation
		{
			get
			{
				return textureTranslation;
			}
			set
			{
				if (textureTranslation != value)
				{
					textureTranslation = value;
					dirtyness = 2;
				}
			}
		}

		public Vector2 TextureScale
		{
			get
			{
				return textureScale;
			}
			set
			{
				if (textureScale != value)
				{
					textureScale = value;
					dirtyness = 2;
				}
			}
		}

		public float TextureRotation
		{
			get
			{
				return textureRotation;
			}
			set
			{
				if (textureRotation != value)
				{
					textureRotation = value;
					dirtyness = 2;
				}
			}
		}

		public Alignment Alignment
		{
			get
			{
				return alignment;
			}
			set
			{
				if (alignment != value)
				{
					alignment = value;
					dirtyness = 1;
				}
			}
		}

		public bool Stretch
		{
			get
			{
				return stretch;
			}
			set
			{
				if (stretch != value)
				{
					stretch = value;
					dirtyness = 1;
				}
			}
		}

		public AspectRatioOptionEnum AspectRatioOption
		{
			get
			{
				return aspectRatioOption;
			}
			set
			{
				if (aspectRatioOption != value)
				{
					aspectRatioOption = value;
					dirtyness = 1;
				}
			}
		}

		public UITexture Texture
		{
			set
			{
				SetTexture(ref value);
			}
		}

		UITexture IUITraitTexture.Texture
		{
			get
			{
				return texture;
			}
			set
			{
				SetTexture(ref value);
			}
		}

		ref UIMaterialPropertyOverrides IUIMaterialPropertyOverridesProvider.MaterialPropertyOverrides => ref materialPropertyOverrides;

		public abstract UIMaterialId Material { get; set; }

		public void SetTexture(ref UITexture value)
		{
			if (texture != value)
			{
				texture = value;
				dirtyness = 2;
			}
		}

		public UIAtomId GetMaterialPropertiesAtomId(MaterialPropertyFieldInfo[] materialPropertiesInfo)
		{
			return materialPropertyOverrides.GetAtomId(materialPropertiesInfo);
		}

		public override void CreateAnimationItems(IUIAnimationItemsCollection animationItemsCollection)
		{
			base.CreateAnimationItems(animationItemsCollection);
			animationItemsCollection.Add(Mutators.Color);
			animationItemsCollection.Add(Mutators.Gradient);
			if (materialPropertyOverrides.Empty)
			{
				return;
			}
			int num = materialPropertyOverrides.Items.Length;
			for (int i = 0; i < num; i++)
			{
				UIMaterialPropertyOverride uIMaterialPropertyOverride = materialPropertyOverrides.Items[i];
				StaticString staticString = new StaticString(uIMaterialPropertyOverride.Name);
				int elementIndex = i;
				switch (uIMaterialPropertyOverride.Type)
				{
				case UIMaterialPropertyOverride.PropertyType.Float:
					IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, staticString, (UIAbstractImage t) => t.materialPropertyOverrides.GetFloatValue(elementIndex), delegate(UIAbstractImage t, float x)
					{
						t.materialPropertyOverrides.SetFloatValue(elementIndex, x);
					});
					break;
				case UIMaterialPropertyOverride.PropertyType.Vector:
					IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, staticString, (UIAbstractImage t) => t.materialPropertyOverrides.GetVectorValue(elementIndex), delegate(UIAbstractImage t, Vector3 x)
					{
						t.materialPropertyOverrides.SetVectorValue(elementIndex, x);
					});
					break;
				case UIMaterialPropertyOverride.PropertyType.Color:
					IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, staticString, (UIAbstractImage t) => t.materialPropertyOverrides.GetColorValue(elementIndex), delegate(UIAbstractImage t, Color x)
					{
						t.materialPropertyOverrides.SetColorValue(elementIndex, x);
					});
					break;
				}
			}
		}

		void IUITexturesReloaded.OnTexturesReloaded()
		{
			if (base.Loaded)
			{
				dirtyness = 2;
			}
		}

		protected override void Load()
		{
			base.Load();
			materialPropertyOverrides.OnLoad();
			dirtyness = 2;
		}

		protected override void Unload()
		{
			materialPropertyOverrides.OnUnload();
			dirtyness = 0;
			base.Unload();
		}

		protected override void OnPropertyChanged()
		{
			base.OnPropertyChanged();
			dirtyness = 2;
			if (texture.Loaded)
			{
				texture.Unload();
				texture.RequestAsset();
			}
		}

		protected Rect ComputeLocalRect()
		{
			Rect localRect = UITransform.LocalRect;
			if (!stretch && texture.IsDefined)
			{
				texture.RequestAsset();
				Vector2 widthHeight = texture.WidthHeight;
				if (aspectRatioOption == AspectRatioOptionEnum.PreserveWidth)
				{
					Vector2 vector = widthHeight;
					widthHeight.x = localRect.width;
					widthHeight.y = localRect.width * vector.y / vector.x;
				}
				else if (aspectRatioOption == AspectRatioOptionEnum.PreserveHeight)
				{
					Vector2 vector2 = widthHeight;
					widthHeight.x = localRect.height * vector2.x / vector2.y;
					widthHeight.y = localRect.height;
				}
				float num = alignment.Horizontal switch
				{
					HorizontalAlignment.Center => localRect.center.x - widthHeight.x * 0.5f, 
					HorizontalAlignment.Left => localRect.xMin, 
					HorizontalAlignment.Right => localRect.xMax - widthHeight.x, 
					_ => throw new NotImplementedException(), 
				};
				float num2 = alignment.Vertical switch
				{
					VerticalAlignment.Center => localRect.center.y - widthHeight.y * 0.5f, 
					VerticalAlignment.Top => localRect.yMin, 
					VerticalAlignment.Bottom => localRect.yMax - widthHeight.y, 
					_ => throw new NotImplementedException(), 
				};
				num += alignment.HorizontalOffset;
				num2 += alignment.VerticalOffset;
				return new Rect(num, num2, widthHeight.x, widthHeight.y);
			}
			return localRect;
		}

		[MutatorsProvider]
		private static void LoadStyles(MutatorsReceiver receiver)
		{
			receiver.Add(Mutators.Texture);
			receiver.Add(Mutators.Color);
			receiver.Add(Mutators.Gradient);
		}

		private void OnMaterialPropertyOverridesChanged()
		{
			TryReload();
		}

		private void OnGradientChanged()
		{
			gradient.GetValue()?.InvalidateBufferContent();
		}
	}
}
