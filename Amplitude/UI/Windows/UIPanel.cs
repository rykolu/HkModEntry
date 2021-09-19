namespace Amplitude.UI.Windows
{
	public class UIPanel : UIContainer
	{
		public void Show(bool instant = false)
		{
			if (!base.Shown)
			{
				RequestShow(instant);
			}
			else
			{
				Dirtyfy();
			}
		}

		public void Hide(bool instant = false)
		{
			if (base.Shown || (instant && base.Visibility == VisibilityState.Hiding))
			{
				RequestHide(instant);
			}
		}

		public void UpdateVisibility(bool value, bool instant = false)
		{
			if (value)
			{
				Show(instant);
			}
			else
			{
				Hide(instant);
			}
		}
	}
}
