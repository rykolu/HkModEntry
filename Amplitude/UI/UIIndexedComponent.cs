using System;
using System.Collections.Generic;
using System.Reflection;
using Amplitude.UI.Interactables;
using UnityEngine;

namespace Amplitude.UI
{
	[RequireComponent(typeof(UITransform))]
	[ExecuteInEditMode]
	public abstract class UIIndexedComponent : UIComponent
	{
		[Flags]
		public enum EventReceiverFlags
		{
			None = 0x0,
			OnSortingRangeChanged = 0x1,
			OnTransformVisibleGloballyChanged = 0x2,
			OnTransformInteractiveGloballyChanged = 0x4,
			OnTransformPositionOrSizeChanged = 0x8,
			OnTransformLocalRectChanged = 0x10,
			OnTransformGlobalPositionChanged = 0x20,
			OnTransformGlobalSizeChanged = 0x40,
			OnTransformLayerIdentifierGloballyChanged = 0x80,
			OnTransformGroupIndexGloballyChanged = 0x100,
			OnReactivityChanged = 0x200,
			IsUIControl = 0x400,
			All = 0xFFFF
		}

		private static Dictionary<Type, EventReceiverFlags> typeToEventReceiverFlags;

		private IndexRange sortingRange = IndexRange.Invalid;

		private int layerIdentifier = -1;

		private int groupIndex = -1;

		private bool uiTransformVisibleGlobally;

		private bool uiTransformInteractiveGlobally;

		private EventReceiverFlags receiverFlags;

		private int lastLayerIndex = -1;

		public long SortingIndex => sortingRange.First;

		public IndexRange SortingRange => sortingRange;

		public int LayerIdentifier => layerIdentifier;

		public int GroupIndex => groupIndex;

		public bool TransformVisibleGlobally => uiTransformVisibleGlobally;

		public bool TransformInteractiveGlobally => uiTransformInteractiveGlobally;

		public bool IsUpToDate => sortingRange.IsValid;

		public UITransform GetUITransform()
		{
			return UITransform;
		}

		public SortedResponder GetSortedResponder(UIView view, int viewGroupCullingMask, IUIResponder uiresponder)
		{
			SortedResponder result = default(SortedResponder);
			result.EventSensitivity = InputEvent.EventType.All;
			if (!base.enabled || !(sortingRange != IndexRange.Invalid) || !uiTransformVisibleGlobally || ((1 << groupIndex) & viewGroupCullingMask) == 0)
			{
				result.Responder = null;
				result.SortingIndex = 0L;
				result.LayerIndex = 0;
			}
			else
			{
				result.Responder = uiresponder;
				result.SortingIndex = sortingRange.Min;
				view.LayerOrderedIndex(layerIdentifier, ref lastLayerIndex);
				result.LayerIndex = lastLayerIndex;
			}
			return result;
		}

		internal void InternalOnReactivityChanged(ref UIReactivityState reactivityState, bool instant)
		{
			OnReactivityChanged(ref reactivityState, instant);
		}

		internal void InternalOnTransformPositionOrSizeChanged(bool positionChanged, bool sizeChanged)
		{
			OnTransformPositionOrSizeChanged(positionChanged, sizeChanged);
		}

		internal void InternalOnTransformLocalRectChanged()
		{
			OnTransformLocalRectChanged();
		}

		internal void InternalOnTransformGlobalPositionChanged()
		{
			OnTransformGlobalPositionChanged();
		}

		internal void InternalOnTransformGlobalSizeChanged()
		{
			OnTransformGlobalSizeChanged();
		}

		internal void OnTransformStateChanged(IndexRange sortingRange, bool visibleGlobally, bool interactiveGlobally, int layerIdentifier, int groupIndex)
		{
			IndexRange indexRange = this.sortingRange;
			this.sortingRange = sortingRange;
			bool flag = uiTransformVisibleGlobally;
			uiTransformVisibleGlobally = visibleGlobally;
			bool flag2 = uiTransformInteractiveGlobally;
			uiTransformInteractiveGlobally = interactiveGlobally;
			int num = this.layerIdentifier;
			this.layerIdentifier = layerIdentifier;
			int num2 = this.groupIndex;
			this.groupIndex = groupIndex;
			if (indexRange != sortingRange && (receiverFlags & EventReceiverFlags.OnSortingRangeChanged) != 0)
			{
				OnSortingRangeChanged(indexRange, this.sortingRange);
			}
			if (flag != visibleGlobally && (receiverFlags & EventReceiverFlags.OnTransformVisibleGloballyChanged) != 0)
			{
				OnTransformVisibleGloballyChanged(flag, uiTransformVisibleGlobally);
			}
			if (flag2 != interactiveGlobally && (receiverFlags & EventReceiverFlags.OnTransformInteractiveGloballyChanged) != 0)
			{
				OnTransformInteractiveGloballyChanged(flag2, uiTransformInteractiveGlobally);
			}
			if (num != layerIdentifier && (receiverFlags & EventReceiverFlags.OnTransformLayerIdentifierGloballyChanged) != 0)
			{
				OnTransformLayerIdentifierGloballyChanged(num, this.layerIdentifier);
			}
			if (num2 != groupIndex && (receiverFlags & EventReceiverFlags.OnTransformGroupIndexGloballyChanged) != 0)
			{
				OnTransformGroupIndexGloballyChanged(num2, this.groupIndex);
			}
		}

		protected override void Load()
		{
			base.Load();
			receiverFlags = GetEventReceiverFlags(GetType());
			UITransform.RegisterSibling(this, receiverFlags);
		}

		protected override void Unload()
		{
			if (UITransform.Loaded)
			{
				UITransform.UnregisterSibling(this);
			}
			receiverFlags = EventReceiverFlags.None;
			base.Unload();
		}

		protected virtual void OnSortingRangeChanged(IndexRange previousRange, IndexRange currentRange)
		{
		}

		protected virtual void OnTransformVisibleGloballyChanged(bool previouslyVisible, bool currentlyVisible)
		{
		}

		protected virtual void OnTransformInteractiveGloballyChanged(bool previouslyInteractive, bool currentlyInteractive)
		{
		}

		protected virtual void OnTransformPositionOrSizeChanged(bool positionChanged, bool sizeChanged)
		{
		}

		protected virtual void OnTransformLocalRectChanged()
		{
		}

		protected virtual void OnTransformGlobalPositionChanged()
		{
		}

		protected virtual void OnTransformGlobalSizeChanged()
		{
		}

		protected virtual void OnTransformLayerIdentifierGloballyChanged(int previousLayerIndex, int layerIndex)
		{
		}

		protected virtual void OnTransformGroupIndexGloballyChanged(int previousGroupIndex, int groupIndex)
		{
		}

		protected virtual void OnReactivityChanged(ref UIReactivityState reactivityState, bool instant)
		{
		}

		private static EventReceiverFlags GetEventReceiverFlags(Type type)
		{
			if (typeToEventReceiverFlags == null)
			{
				typeToEventReceiverFlags = new Dictionary<Type, EventReceiverFlags>();
			}
			EventReceiverFlags value = EventReceiverFlags.None;
			if (typeToEventReceiverFlags.TryGetValue(type, out value))
			{
				return value;
			}
			value = GetEventReceiverFlag(type, "OnSortingRangeChanged", EventReceiverFlags.OnSortingRangeChanged) | GetEventReceiverFlag(type, "OnTransformVisibleGloballyChanged", EventReceiverFlags.OnTransformVisibleGloballyChanged) | GetEventReceiverFlag(type, "OnTransformInteractiveGloballyChanged", EventReceiverFlags.OnTransformInteractiveGloballyChanged) | GetEventReceiverFlag(type, "OnTransformPositionOrSizeChanged", EventReceiverFlags.OnTransformPositionOrSizeChanged) | GetEventReceiverFlag(type, "OnTransformLocalRectChanged", EventReceiverFlags.OnTransformLocalRectChanged) | GetEventReceiverFlag(type, "OnTransformGlobalPositionChanged", EventReceiverFlags.OnTransformGlobalPositionChanged) | GetEventReceiverFlag(type, "OnTransformGlobalSizeChanged", EventReceiverFlags.OnTransformGlobalSizeChanged) | GetEventReceiverFlag(type, "OnTransformLayerIdentifierGloballyChanged", EventReceiverFlags.OnTransformLayerIdentifierGloballyChanged) | GetEventReceiverFlag(type, "OnTransformGroupIndexGloballyChanged", EventReceiverFlags.OnTransformGroupIndexGloballyChanged) | GetEventReceiverFlag(type, "OnReactivityChanged", EventReceiverFlags.OnReactivityChanged);
			if (typeof(IUIControl).IsAssignableFrom(type))
			{
				value |= EventReceiverFlags.IsUIControl;
			}
			typeToEventReceiverFlags.Add(type, value);
			return value;
		}

		private static EventReceiverFlags GetEventReceiverFlag(Type type, string methodName, EventReceiverFlags flag)
		{
			if (!(type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType != typeof(UIIndexedComponent)))
			{
				return EventReceiverFlags.None;
			}
			return flag;
		}
	}
}
