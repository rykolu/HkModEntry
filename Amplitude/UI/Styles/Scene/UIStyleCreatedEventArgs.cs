using Amplitude.UI.Styles.Data;

namespace Amplitude.UI.Styles.Scene
{
	internal class UIStyleCreatedEventArgs : UIStylesEventArg
	{
		internal readonly UIStyle Style;

		internal UIStyleCreatedEventArgs(UIStyle style)
			: base(Type.StyleCreated)
		{
			Style = style;
		}
	}
}
