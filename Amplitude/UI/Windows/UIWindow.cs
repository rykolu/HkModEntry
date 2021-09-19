using System;
using System.Collections;
using Amplitude.Framework.Game;

namespace Amplitude.UI.Windows
{
	public class UIWindow : UIContainer, IUIManagedWindow
	{
		[NonSerialized]
		protected LoadingState loadingState;

		UIWindowsGroup IUIManagedWindow.Group { get; set; }

		public LoadingState LoadingState => loadingState;

		public virtual bool CatchInputEvent(ref InputEvent inputEvent)
		{
			return false;
		}

		public virtual IEnumerator OnGameStarted(Game game)
		{
			yield break;
		}

		public virtual void OnGameShuttingDown()
		{
		}

		protected virtual IEnumerator PostLoad()
		{
			yield break;
		}

		protected virtual void PreUnload()
		{
		}

		IEnumerator IUIManagedWindow.DoPostLoad(UIWindowsGroup windowsGroup)
		{
			loadingState = LoadingState.Loading;
			yield return PostLoad();
			loadingState = LoadingState.Loaded;
		}

		void IUIManagedWindow.PreUnload()
		{
			loadingState = LoadingState.Unloading;
			PreUnload();
			loadingState = LoadingState.Unloaded;
		}

		void IUIManagedWindow.Show(bool instant)
		{
			RequestShow(instant);
		}

		void IUIManagedWindow.Hide(bool instant)
		{
			RequestHide(instant);
		}
	}
}
