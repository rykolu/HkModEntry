using Amplitude.UI.Renderers;

namespace Amplitude.UI.Boards
{
	public class UIBoardStringCell : UIBoardReflectionCell<string>
	{
		public UILabel Label;

		protected override void Refresh()
		{
			Label.Text = GetPropertyValue();
		}
	}
}
