using System;
using Amplitude.UI.Animations;
using Amplitude.UI.Interactables;
using Amplitude.UI.Styles.Scene;
using Amplitude.UI.Traits;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[ExecuteInEditMode]
	public class UIMaterialModifier : UIIndexedComponent, IUITraitMaterialModifier, IUITrait<float>, IUIStyleTarget, IUIAnimationTarget
	{
		private static class Mutators
		{
			public static bool Initialized;

			public static MutatorSet<UIMaterialModifier, float>[] FloatValueMutators;

			public static MutatorSet<UIMaterialModifier, Color>[] ColorValueMutators;

			public static void InitalizeStaticsIfNecessary()
			{
				if (Initialized)
				{
					return;
				}
				UIRenderingManager instance = UIRenderingManager.Instance;
				if (instance == null)
				{
					Diagnostics.LogWarning("RenderingManager not found.");
					return;
				}
				_ = instance.MaterialCollection;
				int num = 2;
				int[] array = new int[num];
				MaterialPropertyFieldInfo[] availableMaterialProperties = MaterialPropertyFieldInfo.AvailableMaterialProperties;
				int num2 = availableMaterialProperties.Length;
				for (int i = 0; i < num2; i++)
				{
					switch (availableMaterialProperties[i].Type)
					{
					case MaterialPropertyType.Float:
					case MaterialPropertyType.Percent:
						array[0]++;
						break;
					case MaterialPropertyType.Color:
						array[1]++;
						break;
					}
				}
				FloatValueMutators = new MutatorSet<UIMaterialModifier, float>[array[0]];
				ColorValueMutators = new MutatorSet<UIMaterialModifier, Color>[array[1]];
				Array.Clear(array, 0, num);
				for (int j = 0; j < num2; j++)
				{
					StaticString propertyName = new StaticString(availableMaterialProperties[j].Name);
					switch (availableMaterialProperties[j].Type)
					{
					case MaterialPropertyType.Float:
					case MaterialPropertyType.Percent:
					{
						int num4 = array[0]++;
						FloatValueMutators[num4] = new MutatorSet<UIMaterialModifier, float>(propertyName, (UIMaterialModifier t) => t.GetFloat(propertyName), delegate(UIMaterialModifier t, float value)
						{
							t.SetValue(propertyName, value);
						});
						break;
					}
					case MaterialPropertyType.Color:
					{
						int num3 = array[1]++;
						ColorValueMutators[num3] = new MutatorSet<UIMaterialModifier, Color>(propertyName, (UIMaterialModifier t) => t.GetVector4(propertyName), delegate(UIMaterialModifier t, Color value)
						{
							t.SetVector4(propertyName, value);
						});
						break;
					}
					}
				}
				Initialized = true;
			}
		}

		private class RenderRequest : UIAbstractRenderRequest
		{
			private UIMaterialModifier materialModifier;

			public RenderRequest(UIMaterialModifier materialModifier)
				: base(materialModifier)
			{
				this.materialModifier = materialModifier;
			}

			public override void BindRenderCommands(UIView view)
			{
				if (materialModifier.VisibleGlobally && view.ShouldBeViewed(materialModifier.GroupIndex, materialModifier.LayerIdentifier))
				{
					UIRenderCommand renderCommand = new UIRenderCommand(materialModifier.SortingRange.First, view, materialModifier.LayerIdentifier, materialModifier.PushRender, owner);
					UIRenderCommand renderCommand2 = new UIRenderCommand(materialModifier.SortingRange.Last, view, materialModifier.LayerIdentifier, materialModifier.PopRender, owner);
					AddRenderCommand(view, ref renderCommand);
					AddRenderCommand(view, ref renderCommand2);
				}
			}
		}

		[SerializeField]
		private UIMaterialPropertiesEntry[] propertyEntries = new UIMaterialPropertiesEntry[0];

		private RenderRequest renderRequest;

		[SerializeField]
		private UIStyleController styleController;

		private UIMaterialProperties material;

		public bool VisibleGlobally
		{
			get
			{
				if (base.enabled && base.IsUpToDate)
				{
					return UITransform.VisibleGlobally;
				}
				return false;
			}
		}

		public ref UIStyleController StyleController => ref styleController;

		public float GetFloat(StaticString id)
		{
			return material.GetFloat(id);
		}

		public void SetValue(StaticString id, float value)
		{
			material.SetFloat(id, value);
		}

		public Vector3 GetVector3(StaticString id)
		{
			return material.GetVector3(id);
		}

		public void SetVector3(StaticString id, Vector3 value)
		{
			material.SetVector3(id, value);
		}

		public Vector4 GetVector4(StaticString id)
		{
			return material.GetVector4(id);
		}

		public void SetVector4(StaticString id, Vector4 value)
		{
			material.SetVector4(id, value);
		}

		public bool IsStyleValueApplied(StaticString identifier)
		{
			return styleController.ContainsValue(identifier);
		}

		public void CreateAnimationItems(IUIAnimationItemsCollection animationItemsCollection)
		{
			Mutators.InitalizeStaticsIfNecessary();
			if (Mutators.Initialized)
			{
				int num = Mutators.FloatValueMutators.Length;
				for (int i = 0; i < num; i++)
				{
					animationItemsCollection.Add(Mutators.FloatValueMutators[i]);
				}
				int num2 = Mutators.ColorValueMutators.Length;
				for (int j = 0; j < num2; j++)
				{
					animationItemsCollection.Add(Mutators.ColorValueMutators[j]);
				}
			}
		}

		protected override void OnSortingRangeChanged(IndexRange previousRange, IndexRange currentRange)
		{
			if (base.Loaded)
			{
				UIRenderingManager.Instance.RefreshRenderRequest(renderRequest);
			}
		}

		protected override void OnTransformVisibleGloballyChanged(bool previouslyVisible, bool currentlyVisible)
		{
			if (base.Loaded)
			{
				UIRenderingManager.Instance.RefreshRenderRequest(renderRequest);
			}
		}

		protected override void OnTransformLayerIdentifierGloballyChanged(int previousLayerIndex, int layerIndex)
		{
			if (base.Loaded)
			{
				UIRenderingManager.Instance.RefreshRenderRequest(renderRequest);
			}
		}

		protected override void OnTransformGroupIndexGloballyChanged(int previousGroupIndex, int groupIndex)
		{
			if (base.Loaded)
			{
				UIRenderingManager.Instance.RefreshRenderRequest(renderRequest);
			}
		}

		protected override void OnReactivityChanged(ref UIReactivityState reactivityState, bool instant)
		{
			styleController.UpdateReactivity(ref reactivityState, instant);
		}

		protected override void Load()
		{
			base.Load();
			renderRequest = new RenderRequest(this);
			material = new UIMaterialProperties(propertyEntries);
			UIRenderingManager.Instance.AddRenderRequest(renderRequest);
			styleController.Bind(this);
		}

		protected override void Unload()
		{
			styleController.Unbind();
			material = null;
			if (UIRenderingManager.Instance != null)
			{
				UIRenderingManager.Instance.RemoveRenderRequest(renderRequest);
			}
			renderRequest = null;
			base.Unload();
		}

		[MutatorsProvider]
		private static void LoadStyles(MutatorsReceiver receiver)
		{
			Mutators.InitalizeStaticsIfNecessary();
			if (Mutators.Initialized)
			{
				int num = Mutators.FloatValueMutators.Length;
				for (int i = 0; i < num; i++)
				{
					receiver.Add(Mutators.FloatValueMutators[i]);
				}
				int num2 = Mutators.ColorValueMutators.Length;
				for (int j = 0; j < num2; j++)
				{
					receiver.Add(Mutators.ColorValueMutators[j]);
				}
			}
		}

		private void OnPropertyEntriesChanged()
		{
			material = new UIMaterialProperties(propertyEntries);
		}

		private void PushRender(UIPrimitiveDrawer drawer)
		{
			drawer.PushMaterial(material);
		}

		private void PopRender(UIPrimitiveDrawer drawer)
		{
			drawer.PopMaterial();
		}
	}
}
