using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public abstract class UIInteractable : UIIndexedComponent, IUIInteractable
	{
		private IUIResponder responder;

		public bool IsVisible
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

		public bool IsInteractive
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

		public bool IsMask => false;

		public bool Hovered => responder.Hovered;

		protected IUIResponder Responder => responder;

		protected UIInteractable()
		{
			responder = InstantiateResponder();
		}

		public static bool IsContainedWithinInscribedEllipse(UITransform uiTransform, Vector2 pointPosition)
		{
			Rect globalRect = uiTransform.GlobalRect;
			float num = globalRect.width * 0.5f;
			float num2 = globalRect.height * 0.5f;
			Vector2 a = new Vector2(globalRect.xMin + num, globalRect.yMin + num2);
			if (Mathf.Approximately(num, num2))
			{
				return Vector2.Distance(a, pointPosition) < num;
			}
			float num3 = (pointPosition.x - a.x) / num;
			float num4 = num3 * num3;
			float num5 = (pointPosition.y - a.y) / num2;
			num5 *= num5;
			return num4 + num5 <= 1f;
		}

		public virtual bool Contains(Vector2 standardizedPosition)
		{
			return UITransform.Contains(standardizedPosition);
		}

		public virtual bool TryCatchEvent(ref InputEvent inputEvent)
		{
			return UIInteractivityManager.Instance.TryCatchEventByOneResponder(responder, ref inputEvent);
		}

		public virtual bool TryCatchEvent(ref InputEvent inputEvent, Vector2 mouseScreenPosition)
		{
			return UIInteractivityManager.Instance.TryCatchEventByOneResponder(responder, ref inputEvent, mouseScreenPosition);
		}

		protected override void OnSortingRangeChanged(IndexRange previousRange, IndexRange currentRange)
		{
			if (base.Loaded && responder.ResponderIndex >= 0)
			{
				UIInteractivityManager.SortedRespondersRevisionIndex++;
			}
		}

		protected override void OnTransformVisibleGloballyChanged(bool previouslyVisible, bool currentlyVisible)
		{
			if (base.Loaded)
			{
				if (currentlyVisible)
				{
					if (responder.ResponderIndex < 0)
					{
						UIInteractivityManager.Instance.RegisterResponder(responder);
					}
				}
				else if (responder.ResponderIndex >= 0)
				{
					UIInteractivityManager.Instance.UnregisterResponder(responder);
				}
			}
			OnIsVisibleChanged();
		}

		protected override void OnTransformInteractiveGloballyChanged(bool previouslyInteractive, bool currentlyInteractive)
		{
			if (base.Loaded && responder.ResponderIndex >= 0)
			{
				UIInteractivityManager.SortedRespondersRevisionIndex++;
			}
			OnIsInteractiveChanged();
		}

		protected virtual void OnIsVisibleChanged()
		{
		}

		protected virtual void OnIsInteractiveChanged()
		{
		}

		protected override void Load()
		{
			base.Load();
			if (IsVisible)
			{
				UIInteractivityManager.Instance.RegisterResponder(responder);
			}
		}

		protected override void Unload()
		{
			if (UIInteractivityManager.Instance != null)
			{
				UIInteractivityManager.Instance.UnregisterResponder(responder);
			}
			base.Unload();
		}

		protected abstract IUIResponder InstantiateResponder();
	}
}
