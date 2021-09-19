using System;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public abstract class UIResponder : IUIResponder
	{
		[NonSerialized]
		protected bool hovered;

		protected InputEvent.EventType eventSensitivity = InputEvent.EventType.AllButTooltip;

		private IUIInteractable interactable;

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

		public bool IsInteractive => interactable.IsInteractive;

		public bool IsMask => interactable.IsMask;

		public bool Hovered => hovered;

		protected IUIInteractable Interactable => interactable;

		public UIResponder(IUIInteractable interactable)
		{
			this.interactable = interactable;
		}

		public SortedResponder GetSortedResponder(UIView view, int viewGroupCullingMask)
		{
			SortedResponder sortedResponder = interactable.GetSortedResponder(view, viewGroupCullingMask, this);
			sortedResponder.EventSensitivity &= eventSensitivity;
			return sortedResponder;
		}

		public abstract bool TryCatchEvent(ref InputEvent inputEvent);

		public virtual bool Contains(Vector2 pointPosition)
		{
			return interactable.Contains(pointPosition);
		}

		public override string ToString()
		{
			if (Interactable != null && Interactable.GetUITransform() != null)
			{
				return $"{Interactable.GetUITransform()}|{GetType().Name}";
			}
			return $"???|{GetType().Name}";
		}
	}
}
