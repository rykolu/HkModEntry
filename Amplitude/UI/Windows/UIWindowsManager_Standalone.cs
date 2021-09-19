using System.Collections;
using Amplitude.Framework;

namespace Amplitude.UI.Windows
{
	public abstract class UIWindowsManager_Standalone<WindowsGroupType, SharedDataType> : UIWindowsManager_Base<WindowsGroupType, SharedDataType>, IUIWindowsGroupClient where WindowsGroupType : UIWindowsGroup, IUIWindowGroupUpdatable<SharedDataType>
	{
		private UnityCoroutine windowsLoadingCoroutine;

		protected override void Load()
		{
			base.Load();
			windowsLoadingCoroutine = UnityCoroutine.StartCoroutine(this, DoLoadWindows, WindowsLoadingCoroutineException);
		}

		protected override void Unload()
		{
			if (windowsLoadingCoroutine != null && windowsLoadingCoroutine.Coroutine != null)
			{
				StopCoroutine(windowsLoadingCoroutine.Coroutine);
				windowsLoadingCoroutine = null;
			}
			UnloadGroupsAndWindows();
			base.Unload();
		}

		protected virtual IEnumerator DoLoadWindows()
		{
			InstantiateAllGroups();
			yield return DoInternalLoadAllGroupsAndWindows();
			windowsLoadingCoroutine = null;
		}

		private void WindowsLoadingCoroutineException(object sender, CoroutineExceptionEventArgs args)
		{
			Diagnostics.LogException(args.Exception, 1);
			windowsLoadingCoroutine = null;
			Application.Quit();
		}
	}
}
