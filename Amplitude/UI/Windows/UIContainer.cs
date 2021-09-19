using System;

namespace Amplitude.UI.Windows
{
	public abstract class UIContainer : UIAbstractShowable
	{
		[NonSerialized]
		private bool dirty;

		public void Dirtyfy()
		{
			if (base.Visibility == VisibilityState.Visible || base.Visibility == VisibilityState.Showing)
			{
				dirty = true;
			}
		}

		public void RefreshNow()
		{
			dirty = false;
			Refresh();
		}

		internal void UpdateAndCheckDirty()
		{
			if (!base.Shown)
			{
				return;
			}
			if (base.Visibility == VisibilityState.PreShowing)
			{
				if (IsReadyForShowing())
				{
					InternalRequestShow();
				}
				return;
			}
			SpecificUpdate();
			if (base.Shown && dirty)
			{
				RefreshNow();
			}
		}

		protected virtual void SpecificUpdate()
		{
		}

		protected virtual void Refresh()
		{
		}

		protected sealed override void RequestShow(bool instant)
		{
			UIContainerManager.Instance.Register(this);
			base.RequestShow(instant);
		}

		protected sealed override void RequestHide(bool instant)
		{
			base.RequestHide(instant);
			UIContainerManager.Instance.Unregister(this);
		}

		protected sealed override void InternalRequestShow()
		{
			base.InternalRequestShow();
			if (base.Visibility != VisibilityState.Hiding && base.Visibility != VisibilityState.Invisible)
			{
				RefreshNow();
			}
		}

		protected sealed override void ApplyInitialVisibility()
		{
			base.ApplyInitialVisibility();
			if (base.Shown)
			{
				UIContainerManager.Instance?.Register(this);
			}
		}
	}
}
