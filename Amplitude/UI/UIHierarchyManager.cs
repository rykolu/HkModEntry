using Amplitude.UI.Interactables;
using Amplitude.UI.Renderers;
using Amplitude.UI.Styles.Scene;
using UnityEngine;

namespace Amplitude.UI
{
	[RequireComponent(typeof(UIRenderingManager))]
	[RequireComponent(typeof(UIInteractivityManager))]
	[RequireComponent(typeof(UIStyleManager))]
	[ExecuteInEditMode]
	public class UIHierarchyManager : UIBehaviour
	{
		private static UIHierarchyManager instance = null;

		private static float globalScale = 1f;

		[SerializeField]
		private UIView mainFullScreenView;

		[SerializeField]
		private string[] layerNames = new string[1] { "Default" };

		[SerializeField]
		private string[] groupNames = new string[1] { "Default" };

		private int previousOutputWidth = 1;

		private int previousOutputHeight = 1;

		public static UIHierarchyManager Instance => instance;

		public static float GlobalScale
		{
			get
			{
				return globalScale;
			}
			set
			{
				globalScale = value;
			}
		}

		public Vector2 StandardizedWidthHeight => new Vector2(mainFullScreenView.StandardizedRect.width, mainFullScreenView.StandardizedRect.height);

		public Vector2 RenderedWidthHeight => new Vector2(mainFullScreenView.RenderedRect.width, mainFullScreenView.RenderedRect.height);

		public string[] LayerNames => layerNames;

		public string[] GroupNames => groupNames;

		public UIView MainFullscreenView
		{
			get
			{
				return mainFullScreenView;
			}
			internal set
			{
				mainFullScreenView = value;
			}
		}

		protected UIHierarchyManager()
		{
			instance = this;
			UITransform.Roots.Clear();
		}

		internal void SpecificLateUpdate()
		{
			if (!base.Loaded)
			{
				return;
			}
			Rect standardizedRect = mainFullScreenView.StandardizedRect;
			Rect renderedRect = mainFullScreenView.RenderedRect;
			foreach (UIView allActiveView in UIView.AllActiveViews)
			{
				allActiveView.UpdateStandardizedAndRenderedRects();
			}
			if (mainFullScreenView.StandardizedRect != standardizedRect)
			{
				for (int i = 0; i < UITransform.Roots.Count; i++)
				{
					UITransform.Roots.Data[i].ForceUpdatePositionGloballyRecursively();
				}
			}
			bool flag = mainFullScreenView.RenderedRect != renderedRect;
			if (mainFullScreenView.OutputWidth != previousOutputWidth || mainFullScreenView.OutputHeight != previousOutputHeight)
			{
				if (previousOutputWidth == 1 && previousOutputHeight == 1)
				{
					Diagnostics.Log("Resolution initialized to {0}x{1}", mainFullScreenView.OutputWidth, mainFullScreenView.OutputHeight);
				}
				else
				{
					Diagnostics.Log("Resolution changed from {0}x{1} to {2}x{3}", previousOutputWidth, previousOutputHeight, mainFullScreenView.OutputWidth, mainFullScreenView.OutputHeight);
				}
				flag = true;
				previousOutputWidth = mainFullScreenView.OutputWidth;
				previousOutputHeight = mainFullScreenView.OutputHeight;
			}
			if (!flag)
			{
				return;
			}
			for (int j = 0; j < UITransform.Roots.Count; j++)
			{
				IUIResolutionDependent[] componentsInChildren = UITransform.Roots.Data[j].GetComponentsInChildren<IUIResolutionDependent>();
				for (int k = 0; k < componentsInChildren.Length; k++)
				{
					componentsInChildren[k].OnResolutionChanged(previousOutputWidth, previousOutputHeight, mainFullScreenView.OutputWidth, mainFullScreenView.OutputHeight);
				}
			}
		}

		internal void RegisterRoot(UITransform root)
		{
			UITransform.Roots.Add(root);
			if (base.Loaded)
			{
				ReinitializeRoots();
			}
		}

		internal void UnregisterRoot(UITransform root)
		{
			int index = UITransform.Roots.IndexOf(root);
			UITransform.Roots.RemoveAt(index);
			root.ResetRecursively();
			if (base.Loaded)
			{
				ReinitializeRoots();
			}
		}

		protected override void Load()
		{
			base.Load();
			instance = this;
			if ((bool)mainFullScreenView)
			{
				mainFullScreenView.LoadIfNecessary();
			}
			ReinitializeRoots();
		}

		protected override void Unload()
		{
			ResetRoots();
			base.Unload();
		}

		protected override void Destruct()
		{
			UITransform.Roots.Clear();
			instance = null;
			base.Destruct();
		}

		private void ReinitializeRoots()
		{
			int count = UITransform.Roots.Count;
			if (count != 0)
			{
				long num = long.MaxValue / count;
				for (int i = 0; i < count; i++)
				{
					UITransform obj = UITransform.Roots.Data[i];
					IndexRange sortingRange = new IndexRange(num * i, num * (i + 1));
					obj.InitializeRecursively(sortingRange);
				}
			}
		}

		private void ResetRoots()
		{
			for (int i = 0; i < UITransform.Roots.Count; i++)
			{
				UITransform uITransform = UITransform.Roots.Data[i];
				if (uITransform.SortingRange.IsValid)
				{
					uITransform.ResetRecursively();
				}
			}
		}
	}
}
