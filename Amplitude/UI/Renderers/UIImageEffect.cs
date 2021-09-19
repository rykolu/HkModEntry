using Amplitude.UI.Animations;
using Amplitude.UI.Styles;
using Amplitude.UI.Styles.Scene;
using Amplitude.UI.Traits;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	public class UIImageEffect : UIScopedRenderer, IUITraitColor, IUITrait<Color>
	{
		public static class Mutators
		{
			public static readonly MutatorSet<UIImageEffect, Color> Color = new MutatorSet<UIImageEffect, Color>(ItemIdentifiers.Color, (UIImageEffect t) => t.Color, delegate(UIImageEffect t, Color value)
			{
				t.Color = value;
			});

			public static readonly MutatorSet<UIImageEffect, UIGradient> Gradient = new MutatorSet<UIImageEffect, UIGradient>(ItemIdentifiers.Gradient, (UIImageEffect t) => t.Gradient, delegate(UIImageEffect t, UIGradient value)
			{
				t.Gradient = value;
			});
		}

		[SerializeField]
		[UIMaterialId(UIPrimitiveType.Rect)]
		private UIMaterialId material;

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

		private UIGrabber grabber;

		public UIMaterialId Material
		{
			get
			{
				return material;
			}
			set
			{
				material = value;
			}
		}

		public Color Color
		{
			get
			{
				return color;
			}
			set
			{
				color = value;
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
				textureTranslation = value.Translation;
				textureRotation = value.Rotation;
				textureScale = value.Scale;
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
				textureTranslation = value;
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
				textureScale = value;
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
				textureRotation = value;
			}
		}

		public override void CreateAnimationItems(IUIAnimationItemsCollection animationItemsCollection)
		{
			base.CreateAnimationItems(animationItemsCollection);
			animationItemsCollection.Add(Mutators.Color);
			animationItemsCollection.Add(Mutators.Gradient);
		}

		protected override void EnterRender(UIPrimitiveDrawer drawer)
		{
			Rect localRect = UITransform.LocalRect;
			if (!(localRect.width <= 0f) && !(localRect.height <= 0f))
			{
				Vector2 imageEffectCenterGlobalPos = UITransform.GlobalRect.center;
				grabber.Begin(UITransform, drawer, ref imageEffectCenterGlobalPos);
			}
		}

		protected override void LeaveRender(UIPrimitiveDrawer drawer)
		{
			Rect localRect = UITransform.LocalRect;
			if (!(localRect.width <= 0f) && !(localRect.height <= 0f))
			{
				UITexture texture = grabber.End(drawer);
				_ = UITransform.LocalToGlobalMatrixFlags;
				Vector2Int screenFlooredTopLeft = grabber.ScreenFlooredTopLeft;
				Vector2Int screenCeiledBottomRight = grabber.ScreenCeiledBottomRight;
				Vector2 vector = drawer.RenderTargetPixelPositionToStandardizedPosition(screenFlooredTopLeft);
				Vector2 vector2 = drawer.RenderTargetPixelPositionToStandardizedPosition(screenCeiledBottomRight);
				Rect position = new Rect(vector, vector2 - vector);
				AffineTransform2d coordinatesTransform = TextureTransformation;
				Matrix4x4 value = Matrix4x4.identity;
				UIAtomId uIAtomId = UIAtomContainer<Matrix4x4>.AllocateTemporary(ref value);
				drawer.Rect(ref uIAtomId, ref position, material.Id, texture, ref coordinatesTransform, ref color, Gradient);
			}
		}

		protected override void Unload()
		{
			base.Unload();
			grabber.Dispose();
		}

		[MutatorsProvider]
		private static void LoadStyles(MutatorsReceiver receiver)
		{
			receiver.Add(Mutators.Color);
			receiver.Add(Mutators.Gradient);
		}

		private void OnGradientChanged()
		{
			gradient.GetValue()?.InvalidateBufferContent();
		}
	}
}
