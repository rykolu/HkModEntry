using Amplitude.UI.Animations;
using Amplitude.UI.Interactables;
using Amplitude.UI.Styles.Scene;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	public abstract class UIScopedRenderer : UIIndexedComponent, IUIStyleTarget, IUIAnimationTarget
	{
		private class RenderRequest : UIAbstractRenderRequest
		{
			private UIScopedRenderer renderer;

			public RenderRequest(UIScopedRenderer renderer)
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
					UIRenderCommand renderCommand = new UIRenderCommand(renderer.SortingRange.First, view, renderer.LayerIdentifier, renderer.EnterRender, owner);
					AddRenderCommand(view, ref renderCommand);
					UIRenderCommand renderCommand2 = new UIRenderCommand(renderer.SortingRange.Last, view, renderer.LayerIdentifier, renderer.LeaveRender, owner);
					AddRenderCommand(view, ref renderCommand2);
				}
			}
		}

		private RenderRequest renderRequest;

		[SerializeField]
		private UIStyleController styleController;

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

		public ref UIStyleController StyleController => ref styleController;

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

		protected abstract void EnterRender(UIPrimitiveDrawer drawer);

		protected abstract void LeaveRender(UIPrimitiveDrawer drawer);

		protected override void Load()
		{
			base.Load();
			styleController.Bind(this);
			renderRequest = new RenderRequest(this);
			UIRenderingManager.Instance.AddRenderRequest(renderRequest);
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
