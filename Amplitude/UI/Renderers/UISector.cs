using System;
using Amplitude.UI.Animations;
using Amplitude.UI.Styles;
using Amplitude.UI.Styles.Scene;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[ExecuteInEditMode]
	public class UISector : UIRenderer
	{
		public static class Mutators
		{
			public static readonly MutatorSet<UISector, UITexture> Texture = new MutatorSet<UISector, UITexture>(ItemIdentifiers.Texture, (UISector t) => t.Texture, delegate(UISector t, UITexture value)
			{
				t.Texture = value;
			});

			public static readonly MutatorSet<UISector, Color> Color = new MutatorSet<UISector, Color>(ItemIdentifiers.Color, (UISector t) => t.Color, delegate(UISector t, Color value)
			{
				t.Color = value;
			});

			public static readonly MutatorSet<UISector, UIGradient> Gradient = new MutatorSet<UISector, UIGradient>(ItemIdentifiers.Gradient, (UISector t) => t.Gradient, delegate(UISector t, UIGradient value)
			{
				t.Gradient = value;
			});

			public static readonly MutatorSet<UISector, float> AngularPosition = new MutatorSet<UISector, float>(ItemIdentifiers.AngularPosition, (UISector t) => t.angularPosition, delegate(UISector t, float value)
			{
				t.angularPosition = Mathf.Clamp(value, (float)Math.PI * -2f, (float)Math.PI * 2f);
			});

			public static readonly MutatorSet<UISector, float> AngularThickness = new MutatorSet<UISector, float>(ItemIdentifiers.AngularThickness, (UISector t) => t.angularThickness, delegate(UISector t, float value)
			{
				t.angularThickness = Mathf.Clamp(value, (float)Math.PI * -2f, (float)Math.PI * 2f);
			});

			public static readonly MutatorSet<UISector, float> RadialThickness = new MutatorSet<UISector, float>(ItemIdentifiers.RadialThickness, (UISector t) => t.radialThickness, delegate(UISector t, float value)
			{
				t.radialThickness = Mathf.Clamp01(value);
			});
		}

		[SerializeField]
		private UITexture texture;

		[SerializeField]
		private Color color = Color.white;

		[SerializeField]
		private UIGradient.Reference gradient;

		[SerializeField]
		[UIMaterialId(UIPrimitiveType.Sector)]
		private UIMaterialId material;

		[SerializeField]
		[Range((float)Math.PI * -2f, (float)Math.PI * 2f)]
		private float angularPosition;

		[SerializeField]
		[Range((float)Math.PI * -2f, (float)Math.PI * 2f)]
		private float angularThickness = (float)Math.PI * 2f;

		[SerializeField]
		[Range(0f, 1f)]
		private float radialThickness = 0.5f;

		[SerializeField]
		private UIBlendType blendType;

		public UIBlendType BlendType
		{
			get
			{
				return blendType;
			}
			set
			{
				blendType = value;
			}
		}

		public UITexture Texture
		{
			get
			{
				return texture;
			}
			set
			{
				texture = value;
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

		public float AngularPositionInRadians
		{
			get
			{
				return angularPosition;
			}
			set
			{
				angularPosition = value;
			}
		}

		public float AngularThicknessInRadians
		{
			get
			{
				return angularThickness;
			}
			set
			{
				angularThickness = value;
			}
		}

		public float AngularPositionInDegrees
		{
			get
			{
				return angularPosition * 57.29578f;
			}
			set
			{
				angularPosition = value * ((float)Math.PI / 180f);
			}
		}

		public float AngularThicknessInDegrees
		{
			get
			{
				return angularThickness * 57.29578f;
			}
			set
			{
				angularThickness = value * ((float)Math.PI / 180f);
			}
		}

		public float RadialThickness
		{
			get
			{
				return radialThickness;
			}
			set
			{
				radialThickness = value;
			}
		}

		public override void CreateAnimationItems(IUIAnimationItemsCollection animationItemsCollection)
		{
			base.CreateAnimationItems(animationItemsCollection);
			animationItemsCollection.Add(Mutators.Color);
			animationItemsCollection.Add(Mutators.Gradient);
			animationItemsCollection.Add(Mutators.AngularPosition);
			animationItemsCollection.Add(Mutators.AngularThickness);
			animationItemsCollection.Add(Mutators.RadialThickness);
		}

		protected override void Render(UIPrimitiveDrawer drawer)
		{
			if (color.a != 0f)
			{
				UITransform uITransform = UITransform;
				Rect localRect = uITransform.LocalRect;
				_ = ref uITransform.LocalToGlobalMatrix;
				drawer.Sector(UITransform.LocalToGlobalMatrix, localRect, angularPosition, angularThickness, radialThickness, material.Id, texture, color, Gradient, null, blendType);
			}
		}

		protected override void Load()
		{
			base.Load();
		}

		protected override void OnPropertyChanged()
		{
			base.OnPropertyChanged();
			_ = texture;
			if (texture.Texture != null)
			{
				texture.Unload();
				texture.RequestAsset();
			}
		}

		[MutatorsProvider]
		private static void LoadStyles(MutatorsReceiver receiver)
		{
			receiver.Add(Mutators.Texture);
			receiver.Add(Mutators.Color);
			receiver.Add(Mutators.Gradient);
			receiver.Add(Mutators.AngularPosition);
			receiver.Add(Mutators.AngularThickness);
			receiver.Add(Mutators.RadialThickness);
		}

		private void OnGradientChanged()
		{
			gradient.GetValue()?.InvalidateBufferContent();
		}
	}
}
