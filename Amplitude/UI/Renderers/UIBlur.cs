using System;
using Amplitude.UI.Animations;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[RequireComponent(typeof(UITransform))]
	[ExecuteInEditMode]
	public class UIBlur : UIScopedRenderer, IUIMaterialPropertyOverridesProvider
	{
		public enum PreserveAlphaPolicyEnum
		{
			Auto,
			Never,
			Always
		}

		public static readonly string MaterialProperty = "material";

		public static readonly string MaterialPropertyOverridesProperty = "materialPropertyOverrides";

		public static readonly string StyleControllerProperty = "styleController";

		public static readonly string PreserveAlphaProperty = "preserveAlpha";

		[SerializeField]
		[UIMaterialId(UIPrimitiveType.Blur)]
		private UIMaterialId material;

		[SerializeField]
		private UIMaterialPropertyOverrides materialPropertyOverrides;

		[SerializeField]
		[Tooltip("Tell the component if it should try to preserve the transparency. Auto means true if inside a ImageEffect false otherwise.")]
		private PreserveAlphaPolicyEnum preserveAlpha;

		[NonSerialized]
		private UIBlurDrawer blurDrawer;

		ref UIMaterialPropertyOverrides IUIMaterialPropertyOverridesProvider.MaterialPropertyOverrides => ref materialPropertyOverrides;

		public override void CreateAnimationItems(IUIAnimationItemsCollection animationItemsCollection)
		{
			base.CreateAnimationItems(animationItemsCollection);
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
					IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, staticString, (UIBlur t) => t.materialPropertyOverrides.GetFloatValue(elementIndex), delegate(UIBlur t, float x)
					{
						t.materialPropertyOverrides.SetFloatValue(elementIndex, x);
					});
					break;
				case UIMaterialPropertyOverride.PropertyType.Vector:
					IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, staticString, (UIBlur t) => t.materialPropertyOverrides.GetVectorValue(elementIndex), delegate(UIBlur t, Vector3 x)
					{
						t.materialPropertyOverrides.SetVectorValue(elementIndex, x);
					});
					break;
				case UIMaterialPropertyOverride.PropertyType.Color:
					IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, staticString, (UIBlur t) => t.materialPropertyOverrides.GetColorValue(elementIndex), delegate(UIBlur t, Color x)
					{
						t.materialPropertyOverrides.SetColorValue(elementIndex, x);
					});
					break;
				}
			}
		}

		protected override void Load()
		{
			base.Load();
			blurDrawer.AddToMaterialProperties(ref materialPropertyOverrides);
		}

		protected override void Unload()
		{
			blurDrawer.Dispose();
			base.Unload();
		}

		protected override void EnterRender(UIPrimitiveDrawer drawer)
		{
			Rect localRect = UITransform.LocalRect;
			Matrix4x4 localToGlobalMatrix = UITransform.LocalToGlobalMatrix;
			blurDrawer.Begin(localToGlobalMatrix, localRect, material, drawer);
		}

		protected override void LeaveRender(UIPrimitiveDrawer drawer)
		{
			bool flag = ((drawer.CurrentOffsetScreenStackSize >= 2) ? true : false);
			bool flag2 = preserveAlpha == PreserveAlphaPolicyEnum.Always || (preserveAlpha != PreserveAlphaPolicyEnum.Never && flag);
			blurDrawer.End(drawer, flag2);
		}

		protected override void OnPropertyChanged()
		{
			base.OnPropertyChanged();
			blurDrawer.AddToMaterialProperties(ref materialPropertyOverrides);
		}
	}
}
