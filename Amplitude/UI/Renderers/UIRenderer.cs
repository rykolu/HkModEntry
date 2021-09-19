using Amplitude.UI.Animations;
using Amplitude.UI.Interactables;
using Amplitude.UI.Styles.Scene;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	public abstract class UIRenderer : UIIndexedComponent, IUIStyleTarget, IUIAnimationTarget
	{
		private class RenderRequest : UIAbstractRenderRequest
		{
			private UIRenderer renderer;

			public RenderRequest(UIRenderer renderer)
				: base(renderer)
			{
				this.renderer = renderer;
			}

			public override string ToString()
			{
				return $"RenderRequest: {renderer}";
			}

			public override void BindRenderCommands(UIView view)
			{
				if (renderer.VisibleGlobally && view.ShouldBeViewed(renderer.GroupIndex, renderer.LayerIdentifier))
				{
					UIRenderCommand renderCommand = new UIRenderCommand(renderer.SortingIndex, view, renderer.LayerIdentifier, renderer.Render, owner);
					AddRenderCommand(view, ref renderCommand);
				}
			}
		}

		private RenderRequest renderRequest;

		[SerializeField]
		private UIStyleController styleController;

		public ref UIStyleController StyleController => ref styleController;

		public bool VisibleGlobally
		{
			get
			{
				if (base.enabled && base.IsUpToDate)
				{
					return base.TransformVisibleGlobally;
				}
				return false;
			}
		}

		public bool IsStyleValueApplied(StaticString identifier)
		{
			return styleController.ContainsValue(identifier);
		}

		public virtual void CreateAnimationItems(IUIAnimationItemsCollection animationItemsCollection)
		{
		}

		protected override void OnReactivityChanged(ref UIReactivityState reactivityState, bool instant)
		{
			styleController.UpdateReactivity(ref reactivityState, instant);
		}

		protected abstract void Render(UIPrimitiveDrawer drawer);

		protected override void Load()
		{
			base.Load();
			UIRenderingManager.Instance.LoadIfNecessary();
			renderRequest = new RenderRequest(this);
			UIRenderingManager.Instance.AddRenderRequest(renderRequest);
			styleController.Bind(this);
		}

		protected override void Unload()
		{
			if (UIRenderingManager.Instance != null)
			{
				UIRenderingManager.Instance.RemoveRenderRequest(renderRequest);
			}
			renderRequest = null;
			styleController.Unbind();
			base.Unload();
		}

		protected override void OnTransformVisibleGloballyChanged(bool previouslyVisible, bool currentlyVisible)
		{
			if (base.Loaded)
			{
				UIRenderingManager.Instance.RefreshRenderRequest(renderRequest);
			}
		}

		protected override void OnSortingRangeChanged(IndexRange previousRange, IndexRange currentRange)
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
	}
}
