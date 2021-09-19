using Amplitude.UI.Styles.Data;

namespace Amplitude.UI.Styles.Scene
{
	internal class UIStylesItemCreatedEventArgs : UIStylesEventArg
	{
		internal readonly UIStyle Style;

		internal readonly UIStyleItem Item;

		internal UIStylesItemCreatedEventArgs(UIStyle style, UIStyleItem item)
			: base(Type.ItemCreated)
		{
			Style = style;
			Item = item;
		}
	}
}
