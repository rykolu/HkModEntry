using System.Collections;

namespace Amplitude.UI.Windows
{
	internal interface IUIManagedWindow
	{
		UIWindowsGroup Group { get; set; }

		IEnumerator DoPostLoad(UIWindowsGroup group);

		void PreUnload();

		void Show(bool instant);

		void Hide(bool instant);
	}
}
