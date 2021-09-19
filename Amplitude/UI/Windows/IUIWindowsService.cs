using System;
using System.Collections;
using Amplitude.Framework;

namespace Amplitude.UI.Windows
{
	public interface IUIWindowsService : IService
	{
		UITransform WindowsRoot { get; }

		void InstantiateAllGroups();

		IEnumerator DoLoadGroupsAndWindows(int criticity = -1, string criticityName = null, Action<float> progressChanged = null);

		void UnloadGroupsAndWindows();

		IEnumerator DoReloadGroupsAndWindows(int criticity = -1, string criticityName = null, Action<float> progressChanged = null);

		void PauseGroupsAndWindowLoading(bool pause);

		WindowType GetWindow<WindowType>() where WindowType : UIWindow;

		void ShowWindow(UIWindow window, bool instant = false);

		void HideWindow(UIWindow window, bool instant = false);

		void UpdateWindowVisibility(UIWindow window, bool visible, bool instant = false);

		void UpdateWindowsGroupVisibility(string windowsGroupName, bool visible, bool instant = false);

		void Dirtyfy();

		void SpecificUpdate();

		bool CatchInputEvent(ref InputEvent inputEvent);
	}
}
