using Amplitude.UI.Styles.Data;

namespace Amplitude.UI.Styles.Scene
{
	internal class UIStylesItemDeletedEventArgs : UIStylesEventArg
	{
		internal readonly UIStyle Style;

		internal readonly StaticString ItemIdentifier = StaticString.Empty;

		internal UIStylesItemDeletedEventArgs(UIStyle style, StaticString identifier)
			: base(Type.ItemDeleted)
		{
			Style = style;
			ItemIdentifier = identifier;
		}
	}
}
