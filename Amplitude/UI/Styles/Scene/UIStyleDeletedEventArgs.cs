using Amplitude.UI.Styles.Data;

namespace Amplitude.UI.Styles.Scene
{
	internal class UIStyleDeletedEventArgs : UIStylesEventArg
	{
		internal readonly UIStyle Style;

		internal UIStyleDeletedEventArgs(UIStyle style)
			: base(Type.StyleDeleted)
		{
			Style = style;
		}
	}
}
