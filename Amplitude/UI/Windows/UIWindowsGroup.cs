using System;
using System.Collections;
using Amplitude.Extensions;
using Amplitude.Framework;
using Amplitude.Framework.Game;
using UnityEngine;

namespace Amplitude.UI.Windows
{
	public abstract class UIWindowsGroup
	{
		private struct InstantiateWindowTask : UIBehaviourAsynchronousLoader.ITask
		{
			private readonly UIWindowsGroup windowsGroup;

			private readonly Transform prefab;

			private readonly int windowIndex;

			public InstantiateWindowTask(UIWindowsGroup windowsGroup, Transform prefab, int windowIndex)
			{
				this.windowsGroup = windowsGroup;
				this.prefab = prefab;
				this.windowIndex = windowIndex;
			}

			public IEnumerator Run()
			{
				Stopwatch stopwatch = default(Stopwatch);
				stopwatch.Start();
				UITransform uITransform = windowsGroup.root.InstantiateChild(prefab, prefab.gameObject.ToString());
				uITransform.gameObject.name = uITransform.gameObject.name.Replace(" (UnityEngine.GameObject)", string.Empty);
				UIWindow component = uITransform.GetComponent<UIWindow>();
				if (component == null)
				{
					Diagnostics.LogError("The prefab '{0}' is not a UIWindow.", prefab.name);
					UnityEngine.Object.Destroy(uITransform.gameObject);
					yield break;
				}
				if (UISettings.BudgetPerInstantiationWarning.Value > 0f && stopwatch.ElapsedMilliseconds > (double)UISettings.BudgetPerInstantiationWarning.Value)
				{
					Diagnostics.LogWarning("Instantiating prefab '{0}' took {1:0.0} ms.", uITransform.ToString(), stopwatch.ElapsedMilliseconds);
				}
				((IUIManagedWindow)component).Group = windowsGroup;
				windowsGroup.windows[windowIndex] = component;
			}
		}

		protected UIWindow[] windows;

		protected IUIWindowsGroupClient client;

		private LoadingState loadingState;

		private UITransform root;

		public UIWindowsGroupDefinition Definition { get; private set; }

		public int Length
		{
			get
			{
				if (windows == null)
				{
					return 0;
				}
				return windows.Length;
			}
		}

		public int Criticity { get; private set; }

		public LoadingState LoadingState => loadingState;

		public static UIWindowsGroup Create(UIWindowsGroupDefinition definition, IUIWindowsGroupClient client, UITransform root)
		{
			Type groupType = definition.GroupType;
			if (groupType != null && typeof(UIWindowsGroup).IsAssignableFrom(groupType))
			{
				UIWindowsGroup obj = Activator.CreateInstance(groupType) as UIWindowsGroup;
				obj.Definition = definition;
				obj.Criticity = definition.Criticity;
				obj.client = client;
				obj.root = root;
				obj.windows = new UIWindow[definition.Length];
				if (definition.RootLayerIndex >= 0)
				{
					root.LayerIdentifierSelf = definition.RootLayerIndex;
				}
				return obj;
			}
			return null;
		}

		public virtual WindowType GetWindow<WindowType>() where WindowType : UIWindow
		{
			if (TryGetWindow<WindowType>(out var window))
			{
				return window;
			}
			Diagnostics.LogError("Could not find window of type '{0}' in group '{1}'.", typeof(WindowType), this);
			return null;
		}

		public virtual bool TryGetWindow<WindowType>(out WindowType window) where WindowType : UIWindow
		{
			int num = ((windows != null) ? windows.Length : 0);
			for (int i = 0; i < num; i++)
			{
				if (windows[i] is WindowType)
				{
					window = windows[i] as WindowType;
					return true;
				}
			}
			window = null;
			return false;
		}

		public virtual void ShowWindow(UIWindow window, bool instant = false)
		{
			InternalShowWindow(window, instant);
		}

		public virtual void HideWindow(UIWindow window, bool instant = false)
		{
			InternalHideWindow(window, instant);
		}

		public virtual void PostLoad()
		{
			loadingState = LoadingState.Loaded;
		}

		public virtual void PreUnload()
		{
			loadingState = LoadingState.Unloading;
		}

		public virtual IEnumerator OnGameCreated(Game game)
		{
			while (loadingState != LoadingState.Loaded)
			{
				yield return null;
			}
			if (Services.GetService<IGameService>().Game == game)
			{
				UIWindow[] array = windows;
				foreach (UIWindow uIWindow in array)
				{
					yield return uIWindow.OnGameStarted(game);
				}
			}
		}

		public virtual void OnGameShuttingDown()
		{
			if (loadingState == LoadingState.Loaded)
			{
				UIWindow[] array = windows;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].OnGameShuttingDown();
				}
			}
		}

		public virtual void OnRuntimeStateChanged(FiniteState newState)
		{
		}

		public virtual bool CatchInputEvent(ref InputEvent inputEvent)
		{
			if (loadingState != LoadingState.Loaded)
			{
				return false;
			}
			for (int num = windows.Length - 1; num >= 0; num--)
			{
				UIWindow uIWindow = windows[num];
				if (uIWindow.Shown && uIWindow.CatchInputEvent(ref inputEvent))
				{
					return true;
				}
			}
			return false;
		}

		public UIWindow GetWindow(int index)
		{
			if (windows == null)
			{
				return null;
			}
			if (index < 0 || index >= windows.Length)
			{
				return null;
			}
			return windows[index];
		}

		internal IEnumerator DoInstantiateWindows()
		{
			loadingState = LoadingState.Loading;
			int length = Definition.Length;
			int windowIndex = 0;
			while (windowIndex < length)
			{
				if (windows[windowIndex] == null)
				{
					if (Definition == null)
					{
						Diagnostics.LogError($"WindowsGroup index '{windowIndex}': Empty Definition");
					}
					else
					{
						Transform prefab = Definition.GetPrefab(windowIndex);
						if (prefab == null)
						{
							Diagnostics.LogError($"WindowsGroup '{Definition.name}': Empty PrefabReference at index '{windowIndex}'");
						}
						else
						{
							if (prefab.GetComponent<UIWindow>() != null)
							{
								yield return UIBehaviourAsynchronousLoader.StartAsyncLoading(new InstantiateWindowTask(this, prefab, windowIndex));
							}
							else
							{
								Diagnostics.LogError("WindowsGroup '" + Definition.name + "': Could not find any component 'UIWindow' in prefab '" + prefab.name + "'.");
							}
							if (client == null)
							{
								yield break;
							}
							client.AdvanceLoadingProgress();
						}
					}
				}
				int num = windowIndex + 1;
				windowIndex = num;
			}
			SanitizeWindowsOrder();
		}

		internal UITransform DestroyWindows()
		{
			int length = Length;
			for (int i = 0; i < length; i++)
			{
				UIWindow uIWindow = windows[i];
				if (uIWindow != null)
				{
					((IUIManagedWindow)uIWindow).Group = null;
					uIWindow.gameObject.SetActive(value: false);
					UnityEngine.Object.DestroyImmediate(uIWindow.gameObject);
				}
			}
			if (length > 0)
			{
				Array.Clear(windows, 0, length);
			}
			windows = null;
			client = null;
			UITransform result = root;
			root = null;
			loadingState = LoadingState.Unloaded;
			return result;
		}

		internal IEnumerator DoPostLoadWindows()
		{
			int length = Length;
			int i = 0;
			while (i < length)
			{
				UIWindow uIWindow = windows[i];
				if (uIWindow != null)
				{
					yield return ((IUIManagedWindow)uIWindow).DoPostLoad(this);
					client.AdvanceLoadingProgress();
				}
				int num = i + 1;
				i = num;
			}
		}

		internal void HideAndPreUnloadWindows()
		{
			int length = Length;
			for (int i = 0; i < length; i++)
			{
				UIWindow uIWindow = windows[i];
				if (uIWindow != null && uIWindow.LoadingState >= LoadingState.Loading)
				{
					if (uIWindow.Shown)
					{
						InternalHideWindow(uIWindow, instant: true);
					}
					((IUIManagedWindow)uIWindow).PreUnload();
				}
			}
		}

		protected void InternalShowWindow(UIWindow window, bool instant = false)
		{
			if (window.LoadingState < LoadingState.Loaded)
			{
				Diagnostics.LogError("Window '{0}' is not fully loaded yet.", window);
			}
			else if (!window.Shown)
			{
				((IUIManagedWindow)window).Show(instant);
				if (loadingState == LoadingState.Loaded)
				{
					client.Dirtyfy();
				}
			}
		}

		protected void InternalHideWindow(UIWindow window, bool instant = false)
		{
			if (window.LoadingState < LoadingState.Loaded)
			{
				Diagnostics.LogError("Window '{0}' is not fully loaded yet.", window);
			}
			else if (!window.Hidden || (instant && window.Visibility == UIAbstractShowable.VisibilityState.Hiding))
			{
				((IUIManagedWindow)window).Hide(instant);
				if (loadingState == LoadingState.Loaded)
				{
					client.Dirtyfy();
				}
			}
		}

		private void SanitizeWindowsOrder()
		{
			if (windows != null)
			{
				windows = windows.RemoveNullValuesAndResize();
				for (int i = 0; i < windows.Length; i++)
				{
					windows[i].transform.SetSiblingIndex(i);
				}
			}
		}
	}
}
