using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amplitude.UI.Windows
{
	public abstract class UIWindowsManager_Base<WindowsGroupType, SharedDataType> : UIBehaviour, IUIWindowsGroupClient where WindowsGroupType : UIWindowsGroup, IUIWindowGroupUpdatable<SharedDataType>
	{
		private class CacheItem
		{
			public WindowsGroupType Group;

			public UIWindow Window;

			public CacheItem(WindowsGroupType group, UIWindow window)
			{
				Group = group;
				Window = window;
			}
		}

		protected List<WindowsGroupType> groupsStack = new List<WindowsGroupType>();

		[SerializeField]
		private UIWindowsGroupDefinition[] groupDefinitionsStack;

		[SerializeField]
		private UITransform windowsRoot;

		private Dictionary<Type, CacheItem> windowsCache = new Dictionary<Type, CacheItem>();

		private Coroutine budgetedCoroutine;

		private bool loadingPaused;

		private bool dirty;

		private Action<float> progressChangedCallback;

		private float progressRatio;

		private int progressStepsCount;

		public UITransform WindowsRoot => windowsRoot;

		public void InstantiateAllGroups()
		{
			if (groupDefinitionsStack != null)
			{
				int num = groupDefinitionsStack.Length;
				for (int i = 0; i < num; i++)
				{
					UIWindowsGroupDefinition obj = groupDefinitionsStack[i];
					GameObject obj2 = new GameObject(obj.name);
					obj2.transform.SetParent(windowsRoot.transform);
					UITransform uITransform = obj2.AddComponent<UITransform>();
					uITransform.LeftAnchor = uITransform.LeftAnchor.SetAttach(b: true);
					uITransform.RightAnchor = uITransform.RightAnchor.SetAttach(b: true);
					uITransform.TopAnchor = uITransform.TopAnchor.SetAttach(b: true);
					uITransform.BottomAnchor = uITransform.BottomAnchor.SetAttach(b: true);
					UIWindowsGroup uIWindowsGroup = UIWindowsGroup.Create(obj, this, uITransform);
					groupsStack.Add((WindowsGroupType)uIWindowsGroup);
				}
			}
		}

		public IEnumerator DoLoadGroupsAndWindows(int criticity, string criticityName = null, Action<float> progressChanged = null)
		{
			criticityName = ((!string.IsNullOrEmpty(criticityName)) ? criticityName : "windows");
			Diagnostics.Log("Start loading {0}.", criticityName);
			double totalMilliseconds = 0.0;
			budgetedCoroutine = Coroutine.StartCoroutine(DoInternalLoadGroupsAndWindows, criticity, progressChanged, CoroutineException);
			Stopwatch stopWatch = default(Stopwatch);
			stopWatch.Start();
			while (budgetedCoroutine != null && !budgetedCoroutine.IsFinished)
			{
				if (loadingPaused)
				{
					yield return Coroutine.DoWaitForSeconds(0.1f);
					continue;
				}
				budgetedCoroutine.Run();
				if (budgetedCoroutine.IsFinished)
				{
					totalMilliseconds += stopWatch.ElapsedMilliseconds;
					break;
				}
				if (stopWatch.ElapsedMilliseconds > (double)UIBehaviourAsynchronousLoader.CurrentBudget)
				{
					totalMilliseconds += stopWatch.ElapsedMilliseconds;
					yield return null;
					stopWatch.Start();
				}
			}
			if (budgetedCoroutine != null)
			{
				Diagnostics.Log("Finished loading {0} windows in {1:0.0} seconds.", criticityName, totalMilliseconds / 1000.0);
			}
			else
			{
				Diagnostics.LogWarning("Aborted loading windows.");
			}
		}

		public void UnloadGroupsAndWindows()
		{
			dirty = false;
			if (budgetedCoroutine != null)
			{
				budgetedCoroutine = null;
			}
			PreUnload();
			UnloadAndDestroyGroups();
			groupsStack.Clear();
		}

		public IEnumerator DoReloadGroupsAndWindows(int criticity, string criticityName = null, Action<float> progressChanged = null)
		{
			criticityName = ((!string.IsNullOrEmpty(criticityName)) ? criticityName : "windows");
			Diagnostics.Log("Start reloading {0}.", criticityName);
			double totalMilliseconds = 0.0;
			budgetedCoroutine = Coroutine.StartCoroutine(DoInternalReloadGroupsAndWindows, criticity, progressChanged, CoroutineException);
			Stopwatch stopWatch = default(Stopwatch);
			stopWatch.Start();
			while (budgetedCoroutine != null && !budgetedCoroutine.IsFinished)
			{
				if (loadingPaused)
				{
					yield return Coroutine.DoWaitForSeconds(0.1f);
					continue;
				}
				budgetedCoroutine.Run();
				if (budgetedCoroutine.IsFinished)
				{
					totalMilliseconds += stopWatch.ElapsedMilliseconds;
					break;
				}
				if (stopWatch.ElapsedMilliseconds > (double)UIBehaviourAsynchronousLoader.CurrentBudget)
				{
					totalMilliseconds += stopWatch.ElapsedMilliseconds;
					yield return null;
					stopWatch.Start();
				}
			}
			if (budgetedCoroutine != null)
			{
				Diagnostics.Log("Finished reloading {0} windows in {1:0.0} seconds.", criticityName, totalMilliseconds / 1000.0);
			}
			else
			{
				Diagnostics.LogWarning("Aborted reloading windows.");
			}
		}

		public void PauseGroupsAndWindowLoading(bool pause)
		{
			loadingPaused = pause;
		}

		public void Dirtyfy()
		{
			dirty = true;
		}

		void IUIWindowsGroupClient.AdvanceLoadingProgress()
		{
			if (progressChangedCallback != null && progressStepsCount > 0 && progressRatio < 1f)
			{
				progressRatio += 1f / (float)progressStepsCount;
				progressChangedCallback(progressRatio);
			}
		}

		public WindowType GetWindow<WindowType>() where WindowType : UIWindow
		{
			WindowType window = null;
			WindowsGroupType windowGroup = null;
			if (FindWindow<WindowType>(out window, out windowGroup))
			{
				return window;
			}
			Diagnostics.LogError("Could not find window of type '{0}''.", typeof(WindowType), this);
			return null;
		}

		public bool TryGetWindow<WindowType>(out WindowType window) where WindowType : UIWindow
		{
			window = null;
			WindowsGroupType windowGroup = null;
			return FindWindow<WindowType>(out window, out windowGroup);
		}

		public void ShowWindow(UIWindow window, bool instant = false)
		{
			if (!(window == null) && window.Loaded && window.Visibility != UIAbstractShowable.VisibilityState.Visible && ((window.Visibility != UIAbstractShowable.VisibilityState.Showing && window.Visibility != UIAbstractShowable.VisibilityState.PreShowing) || instant))
			{
				((IUIManagedWindow)window).Group.ShowWindow(window, instant);
				Dirtyfy();
			}
		}

		public void HideWindow(UIWindow window, bool instant = false)
		{
			if (!(window == null) && window.Loaded && window.Visibility != UIAbstractShowable.VisibilityState.Invisible && (window.Visibility != UIAbstractShowable.VisibilityState.Hiding || instant))
			{
				((IUIManagedWindow)window).Group.HideWindow(window, instant);
				Dirtyfy();
			}
		}

		public void UpdateWindowVisibility(UIWindow window, bool visibility, bool instant = false)
		{
			if (visibility)
			{
				ShowWindow(window, instant);
			}
			else
			{
				HideWindow(window, instant);
			}
		}

		public void UpdateWindowsGroupVisibility(string windowsGroupName, bool visible, bool instant = false)
		{
			for (int i = 0; i < groupsStack.Count; i++)
			{
				WindowsGroupType val = groupsStack[i];
				if (val.Definition.name == windowsGroupName)
				{
					for (int j = 0; j < val.Length; j++)
					{
						UpdateWindowVisibility(val.GetWindow(j), visible, instant);
					}
				}
			}
		}

		public void SpecificUpdate()
		{
			for (int num = groupsStack.Count - 1; num >= 0; num--)
			{
				if (groupsStack[num].IsReady)
				{
					groupsStack[num].SpecificUpdate();
				}
			}
			if (dirty)
			{
				RefreshWindowsGroups();
			}
		}

		public virtual void RefreshWindowsGroups()
		{
			dirty = false;
			SharedDataType data = CreateSharedData();
			for (int num = groupsStack.Count - 1; num >= 0; num--)
			{
				if (groupsStack[num].IsReady)
				{
					groupsStack[num].Refresh(data);
				}
			}
		}

		public virtual bool CatchInputEvent(ref InputEvent inputEvent)
		{
			for (int num = groupsStack.Count - 1; num >= 0; num--)
			{
				if (groupsStack[num].CatchInputEvent(ref inputEvent))
				{
					return true;
				}
			}
			return false;
		}

		protected override void Load()
		{
			base.Load();
			UIContainerManager.Instance?.LoadIfNecessary();
		}

		protected virtual void PostLoad(int criticity)
		{
		}

		protected virtual void PreUnload()
		{
		}

		protected abstract SharedDataType CreateSharedData();

		protected virtual bool FindWindow<WindowType>(out WindowType window, out WindowsGroupType windowGroup) where WindowType : UIWindow
		{
			Type typeFromHandle = typeof(WindowType);
			CacheItem value = null;
			if (windowsCache.TryGetValue(typeFromHandle, out value))
			{
				windowGroup = value.Group;
				window = value.Window as WindowType;
				return true;
			}
			int count = groupsStack.Count;
			for (int i = 0; i < count; i++)
			{
				if (groupsStack[i].TryGetWindow<WindowType>(out window))
				{
					windowGroup = groupsStack[i];
					value = new CacheItem(windowGroup, window);
					return true;
				}
			}
			window = null;
			windowGroup = null;
			return false;
		}

		protected IEnumerator DoInternalLoadAllGroupsAndWindows()
		{
			int currentCriticity = 0;
			while (groupsStack.Count < groupDefinitionsStack.Length)
			{
				yield return DoLoadGroupsAndWindows(currentCriticity);
				int num = currentCriticity + 1;
				currentCriticity = num;
			}
		}

		private IEnumerator DoInternalLoadGroupsAndWindows(int criticity, Action<float> progressChanged)
		{
			yield return DoLoadGroups(criticity, progressChanged);
			PostLoad(criticity);
			Dirtyfy();
		}

		private IEnumerator DoLoadGroups(int criticity, Action<float> progressChanged = null)
		{
			progressChangedCallback = progressChanged;
			progressRatio = 0f;
			progressStepsCount = GetWindowsCount(criticity) * 2;
			foreach (WindowsGroupType item in groupsStack)
			{
				if (item.Criticity == criticity)
				{
					yield return item.DoInstantiateWindows();
				}
			}
			foreach (WindowsGroupType item2 in groupsStack)
			{
				if (item2.Criticity == criticity)
				{
					yield return item2.DoPostLoadWindows();
				}
			}
			foreach (WindowsGroupType item3 in groupsStack)
			{
				if (item3.Criticity == criticity)
				{
					item3.PostLoad();
				}
			}
			progressChangedCallback?.Invoke(1f);
			progressChangedCallback = null;
		}

		private void UnloadAndDestroyGroups()
		{
			Diagnostics.Log("Start unloading windows.");
			Stopwatch stopwatch = default(Stopwatch);
			stopwatch.Start();
			foreach (WindowsGroupType item in groupsStack)
			{
				if (item != null)
				{
					item.PreUnload();
					item.HideAndPreUnloadWindows();
					UITransform uITransform = item.DestroyWindows();
					if (uITransform != null)
					{
						uITransform.gameObject.SetActive(value: false);
						UnityEngine.Object.DestroyImmediate(uITransform.gameObject);
					}
				}
			}
			windowsRoot.transform.DetachChildren();
			Diagnostics.Log("Finished unloading windows in {0:0.0} seconds.", stopwatch.ElapsedMilliseconds / 1000.0);
		}

		private IEnumerator DoInternalReloadGroupsAndWindows(int criticity, Action<float> progressChanged)
		{
			foreach (WindowsGroupType item in groupsStack)
			{
				if (item.Criticity == criticity)
				{
					item.PreUnload();
				}
			}
			foreach (WindowsGroupType item2 in groupsStack)
			{
				if (item2.Criticity == criticity)
				{
					item2.HideAndPreUnloadWindows();
				}
			}
			foreach (WindowsGroupType item3 in groupsStack)
			{
				if (item3.Criticity == criticity)
				{
					yield return item3.DoPostLoadWindows();
				}
			}
			foreach (WindowsGroupType item4 in groupsStack)
			{
				if (item4.Criticity == criticity)
				{
					item4.PostLoad();
				}
			}
		}

		private int GetWindowsCount(int criticity)
		{
			int num = 0;
			foreach (WindowsGroupType item in groupsStack)
			{
				if (item.Criticity == criticity)
				{
					num += item.Definition.Length;
				}
			}
			return num;
		}

		private void CoroutineException(object sender, CoroutineExceptionEventArgs args)
		{
			Diagnostics.LogException(args.Exception);
		}
	}
}
