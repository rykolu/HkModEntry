using Amplitude.UI.Interactables;
using Amplitude.UI.Renderers;
using UnityEngine;

namespace Amplitude.UI.Masks
{
	[ExecuteInEditMode]
	public class UIRectMask : UIMask
	{
		public class StartRectMaskResponder : IUIResponder
		{
			private readonly UIRectMask mask;

			private PerformanceList<ulong> maskedInputEventsIds;

			private int lastLayerIndex = -1;

			private int responderIndex = -1;

			public int ResponderIndex
			{
				get
				{
					return responderIndex;
				}
				set
				{
					responderIndex = value;
				}
			}

			public bool IsInteractive => mask.IsInteractive;

			public bool IsMask => true;

			public bool Focused => false;

			public bool Hovered => false;

			public StartRectMaskResponder(UIRectMask mask)
			{
				this.mask = mask;
			}

			public SortedResponder GetSortedResponder(UIView view, int viewGroupCullingMask)
			{
				SortedResponder result = default(SortedResponder);
				result.EventSensitivity = EventSensibility;
				if (!mask.IsVisible || ((1 << mask.UITransform.GroupIndexGlobally) & viewGroupCullingMask) == 0)
				{
					result.Responder = null;
					result.SortingIndex = 0L;
					result.LayerIndex = 0;
				}
				else
				{
					result.Responder = this;
					result.SortingIndex = mask.SortingRange.First;
					view.LayerOrderedIndex(mask.UITransform.LayerIdentifierGlobally, ref lastLayerIndex);
					result.LayerIndex = lastLayerIndex;
				}
				return result;
			}

			public bool TryCatchEvent(ref InputEvent inputEvent)
			{
				if (inputEvent.Catched)
				{
					int count = maskedInputEventsIds.Count;
					for (int i = 0; i < count; i++)
					{
						if (maskedInputEventsIds.Data[i] == inputEvent.UniqueId)
						{
							UIInteractivityManager.Instance.UncatchInputEvent(inputEvent.UniqueId, this);
							maskedInputEventsIds.RemoveAt(i);
							return false;
						}
					}
				}
				return false;
			}

			public bool Contains(Vector2 pointPosition)
			{
				return true;
			}

			public void AddEventToUnmask(ref InputEvent inputEvent)
			{
				maskedInputEventsIds.Add(inputEvent.UniqueId);
			}

			public override string ToString()
			{
				if (mask.UITransform != null && (bool)mask.UITransform)
				{
					return $"{mask.UITransform}|{GetType().Name}";
				}
				return $"???|{GetType().Name}";
			}
		}

		public class EndRectMaskResponder : IUIResponder
		{
			private readonly UIRectMask mask;

			private readonly StartRectMaskResponder start;

			private int lastLayerIndex = -1;

			private int responderIndex = -1;

			public int ResponderIndex
			{
				get
				{
					return responderIndex;
				}
				set
				{
					responderIndex = value;
				}
			}

			public bool IsInteractive => mask.IsInteractive;

			public bool IsMask => true;

			public bool Focused => false;

			public bool Hovered => false;

			public EndRectMaskResponder(UIRectMask mask, StartRectMaskResponder start)
			{
				this.mask = mask;
				this.start = start;
			}

			public SortedResponder GetSortedResponder(UIView view, int viewGroupCullingMask)
			{
				SortedResponder result = default(SortedResponder);
				result.EventSensitivity = EventSensibility;
				if (!mask.IsVisible || ((1 << mask.UITransform.GroupIndexGlobally) & viewGroupCullingMask) == 0)
				{
					result.Responder = null;
					result.SortingIndex = 0L;
					result.LayerIndex = 0;
				}
				else
				{
					result.Responder = this;
					result.SortingIndex = mask.SortingRange.Last;
					view.LayerOrderedIndex(mask.UITransform.LayerIdentifierGlobally, ref lastLayerIndex);
					result.LayerIndex = lastLayerIndex;
				}
				return result;
			}

			public bool TryCatchEvent(ref InputEvent inputEvent)
			{
				if (inputEvent.Catched)
				{
					return false;
				}
				if (inputEvent.Type == InputEvent.EventType.KeyDown || inputEvent.Type == InputEvent.EventType.KeyUp)
				{
					return false;
				}
				if (!Contains(inputEvent.MousePosition) && (inputEvent.Type & EventSensibility) != 0)
				{
					start.AddEventToUnmask(ref inputEvent);
					return true;
				}
				return false;
			}

			public bool Contains(Vector2 pointPosition)
			{
				return mask.Contains(pointPosition);
			}

			public override string ToString()
			{
				if (mask.UITransform != null && (bool)mask.UITransform)
				{
					return $"{mask.UITransform}|{GetType().Name}";
				}
				return $"???|{GetType().Name}";
			}
		}

		private class RenderRequest : UIAbstractRenderRequest
		{
			private UIRectMask mask;

			public RenderRequest(UIRectMask mask)
				: base(mask)
			{
				this.mask = mask;
			}

			public override void BindRenderCommands(UIView view)
			{
				if (mask.VisibleGlobally && view.ShouldBeViewed(mask.GroupIndex, mask.LayerIdentifier))
				{
					UIRenderCommand renderCommand = new UIRenderCommand(mask.SortingRange.First, view, mask.LayerIdentifier, mask.BeginRender, owner);
					AddRenderCommand(view, ref renderCommand);
					UIRenderCommand renderCommand2 = new UIRenderCommand(mask.SortingRange.Last, view, mask.LayerIdentifier, mask.EndRender, owner);
					AddRenderCommand(view, ref renderCommand2);
				}
			}
		}

		private static readonly InputEvent.EventType EventSensibility = InputEvent.EventType.MouseHoverTooltip | InputEvent.EventType.MouseHover | InputEvent.EventType.MouseDown | InputEvent.EventType.MouseUp | InputEvent.EventType.MouseScroll | InputEvent.EventType.AxisUpdate2D;

		private RenderRequest renderRequest;

		private StartRectMaskResponder startMaskResponder;

		private EndRectMaskResponder endMaskResponder;

		private bool IsVisible
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

		private bool IsInteractive
		{
			get
			{
				if (base.enabled && base.IsUpToDate && base.TransformVisibleGlobally)
				{
					return base.TransformInteractiveGlobally;
				}
				return false;
			}
		}

		public override bool Contains(Vector2 pointPosition)
		{
			return UITransform.Contains(pointPosition);
		}

		protected override void OnSortingRangeChanged(IndexRange previousRange, IndexRange currentRange)
		{
			if (base.Loaded)
			{
				if ((base.Filter & MaskFilter.Renderers) != 0)
				{
					UIRenderingManager.Instance.RefreshRenderRequest(renderRequest);
				}
				if ((base.Filter & MaskFilter.Interactables) != 0)
				{
					UIInteractivityManager.SortedRespondersRevisionIndex++;
				}
			}
		}

		protected override void OnTransformVisibleGloballyChanged(bool previouslyVisible, bool currentlyVisible)
		{
			if (!base.Loaded)
			{
				return;
			}
			if ((base.Filter & MaskFilter.Renderers) != 0)
			{
				UIRenderingManager.Instance.RefreshRenderRequest(renderRequest);
			}
			if ((base.Filter & MaskFilter.Interactables) == 0)
			{
				return;
			}
			if (currentlyVisible)
			{
				if (startMaskResponder.ResponderIndex < 0)
				{
					UIInteractivityManager.Instance.RegisterResponder(startMaskResponder);
				}
				if (endMaskResponder.ResponderIndex < 0)
				{
					UIInteractivityManager.Instance.RegisterResponder(endMaskResponder);
				}
			}
			else
			{
				if (startMaskResponder.ResponderIndex >= 0)
				{
					UIInteractivityManager.Instance.UnregisterResponder(startMaskResponder);
				}
				if (endMaskResponder.ResponderIndex >= 0)
				{
					UIInteractivityManager.Instance.UnregisterResponder(endMaskResponder);
				}
			}
			UIInteractivityManager.SortedRespondersRevisionIndex++;
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

		protected override void Load()
		{
			base.Load();
			startMaskResponder = new StartRectMaskResponder(this);
			endMaskResponder = new EndRectMaskResponder(this, startMaskResponder);
			if ((base.Filter & MaskFilter.Renderers) != 0)
			{
				renderRequest = new RenderRequest(this);
				UIRenderingManager.Instance.AddRenderRequest(renderRequest);
			}
			if (IsVisible && (base.Filter & MaskFilter.Interactables) != 0)
			{
				UIInteractivityManager.Instance.RegisterResponder(startMaskResponder);
				UIInteractivityManager.Instance.RegisterResponder(endMaskResponder);
			}
		}

		protected override void Unload()
		{
			if ((base.Filter & MaskFilter.Interactables) != 0 && UIInteractivityManager.Instance != null)
			{
				UIInteractivityManager.Instance.UnregisterResponder(endMaskResponder);
				UIInteractivityManager.Instance.UnregisterResponder(startMaskResponder);
			}
			if ((base.Filter & MaskFilter.Renderers) != 0 && UIRenderingManager.Instance != null)
			{
				UIRenderingManager.Instance.RemoveRenderRequest(renderRequest);
				renderRequest = null;
			}
			endMaskResponder = null;
			startMaskResponder = null;
			base.Unload();
		}

		protected override void OnFilterChanged(MaskFilter previousFilter, MaskFilter filter)
		{
			base.OnFilterChanged(previousFilter, filter);
			if ((previousFilter & MaskFilter.Interactables) != 0)
			{
				UIInteractivityManager.Instance.UnregisterResponder(endMaskResponder);
				UIInteractivityManager.Instance.UnregisterResponder(startMaskResponder);
			}
			if ((filter & MaskFilter.Interactables) != 0)
			{
				UIInteractivityManager.Instance.RegisterResponder(startMaskResponder);
				UIInteractivityManager.Instance.RegisterResponder(endMaskResponder);
			}
			if ((previousFilter & MaskFilter.Renderers) != 0)
			{
				UIRenderingManager.Instance.RemoveRenderRequest(renderRequest);
			}
			if ((filter & MaskFilter.Renderers) != 0)
			{
				UIRenderingManager.Instance.AddRenderRequest(renderRequest);
			}
		}

		private void BeginRender(UIPrimitiveDrawer drawer)
		{
			UITransform uITransform = UITransform;
			drawer.BeginMask(uITransform.LocalToGlobalMatrix, uITransform.LocalRect);
		}

		private void EndRender(UIPrimitiveDrawer drawer)
		{
			drawer.EndMask();
		}
	}
}
