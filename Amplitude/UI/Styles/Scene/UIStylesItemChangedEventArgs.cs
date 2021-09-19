using Amplitude.UI.Styles.Data;

namespace Amplitude.UI.Styles.Scene
{
	internal class UIStylesItemChangedEventArgs : UIStylesEventArg
	{
		internal readonly UIStyleItem Item;

		internal UIStylesItemChangedEventArgs(UIStyleItem item)
			: base(Type.ItemChanged)
		{
			Item = item;
		}
	}
}
