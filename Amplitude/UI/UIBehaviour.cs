using System;
using System.Diagnostics;
using UnityEngine;

/*using System.Reflection;
using System.IO;
using System.Collections.Generic;*/

namespace Amplitude.UI
{
	public abstract class UIBehaviour : MonoBehaviour
	{
		public enum BehaviourState
		{
			Unloaded,
			Loading,
			Loaded,
			Unloading,
			Destructing,
			Destruct,
			Undefined
		}

		[NonSerialized]
		private BehaviourState currentState;

		[NonSerialized]
		private BehaviourState nextState = BehaviourState.Undefined;

		public bool Loaded => currentState == BehaviourState.Loaded;

		public bool Loading => currentState == BehaviourState.Loading;

		protected virtual bool CanBeDisabled => true;

		private bool CanBeLoaded
		{
			get
			{
				if (currentState == BehaviourState.Unloaded && base.enabled)
				{
					return base.gameObject.activeInHierarchy;
				}
				return false;
			}
		}

		public static bool IsInPlayModeOrEditMode()
		{
			return true;
		}

		public void LoadIfNecessary()
		{
			if (!IsInPlayModeOrEditMode())
			{
				return;
			}
			if (UIBehaviourAsynchronousLoader.Active)
			{
				UIBehaviourAsynchronousLoader.Push(this);
				return;
			}
			if (!base.enabled && !CanBeDisabled)
			{
				base.enabled = true;
			}
			DoCheckSerializedFields();
			if (CanBeLoaded)
			{
				DoLoad();
			}
			else if (currentState == BehaviourState.Loading)
			{
				Diagnostics.LogError("Circular dependency detected while loading '{0}'", this);
			}
			else if (currentState != BehaviourState.Loaded)
			{
				Diagnostics.Log("'{0}' cannot be loaded.", this);
			}
		}

		public void TryReload()
		{
			if (Loaded)
			{
				DoUnload();
			}
			LoadIfNecessary();
		}

		protected virtual void Load()
		{
		}

		protected virtual void Unload()
		{
		}

		protected virtual void Destruct()
		{
		}

		protected virtual void OnValidate()
		{
		}

		protected virtual void OnPropertyChanged()
		{
		}

		protected void Awake()
		{
		}

		/*protected void Start()
		{
		}*/

		protected void OnEnable()
		{
			if (IsInPlayModeOrEditMode())
			{
				if (currentState == BehaviourState.Unloaded)
				{
					LoadIfNecessary();
				}
				else if (currentState == BehaviourState.Unloading)
				{
					Diagnostics.LogWarning("Enable '{0}' (ID: {2}) while unloading it.\nCurrentState = {1}", this, currentState, GetInstanceID());
					nextState = BehaviourState.Loaded;
				}
			}
		}

		protected void OnDisable()
		{
			if (!CanBeDisabled && !base.enabled && base.gameObject.activeInHierarchy)
			{
				base.enabled = true;
			}
			else if (currentState == BehaviourState.Loaded)
			{
				DoUnload();
			}
			else if (currentState == BehaviourState.Loading)
			{
				Diagnostics.LogWarning("Disable '{0}' while loading it.\nCurrentState = {1}", this, currentState);
				nextState = BehaviourState.Unloaded;
			}
		}

		protected void OnDestroy()
		{
			DoDestruct();
		}

		private void DoLoad()
		{
			currentState = BehaviourState.Loading;
			nextState = BehaviourState.Loaded;
			try
			{
				Load();
				currentState = nextState;
				nextState = BehaviourState.Undefined;
			}
			catch (Exception exception)
			{
				Diagnostics.LogException(exception);
				currentState = BehaviourState.Unloaded;
				nextState = BehaviourState.Undefined;
			}
		}

		private void DoUnload()
		{
			currentState = BehaviourState.Unloading;
			nextState = BehaviourState.Unloaded;
			try
			{
				Unload();
				currentState = nextState;
				nextState = BehaviourState.Undefined;
			}
			catch (Exception exception)
			{
				Diagnostics.LogException(exception);
				currentState = BehaviourState.Loaded;
				nextState = BehaviourState.Undefined;
			}
		}

		private void DoDestruct()
		{
			currentState = BehaviourState.Destructing;
			nextState = BehaviourState.Destruct;
			Destruct();
			currentState = nextState;
			nextState = BehaviourState.Undefined;
		}

		private bool DoCheckSerializedFields()
		{
			return true;
		}

		[Conditional("UNITY_EDITOR")]
		internal static void LogLoadingTimesIfInUnityEditor()
		{
		}

		[Conditional("UNITY_EDITOR")]
		private static void ProfileComponentLoading(bool loading)
		{
		}

		[Conditional("UNITY_EDITOR")]
		private void ProfileComponentUnloading(bool unloading)
		{
		}

		protected void Start()
		{
		}

		protected void Update()
		{
		}

		//HOW TO FIND ALL CLASSES CURRENTLY INHERITING "UIBehaviour"
		/*private static void Maintry()
        {
			try
			{
				var allTypes = Assembly.GetAssembly(typeof(UIBehaviour)).GetTypes();
				foreach (var myType in allTypes)
				{
					// Check if this type is subclass of your base class
					bool isSubType = myType.IsSubclassOf(typeof(UIBehaviour));

					// If it is sub-type, then print its name in Debug window.
					if (isSubType)
					{
						StreamWriter HKmodlogger = new StreamWriter(
							System.IO.Path.Combine(ModPath, "ModLogger.txt"),
							true);
						HKmodlogger.WriteLine("GET TYPES: " + myType.Name);
						HKmodlogger.Close();
					}
				}
			}
			catch (Exception e)
			{
				return;
			}
		}*/




		//all classes that inherit UIBehaviour:
		/*
		GET TYPES: UIAbstractShowable
		GET TYPES: UIComponent
		GET TYPES: UIHierarchyManager
		GET TYPES: UIIndexedComponent
		GET TYPES: UIPerformanceAlertFeedback
		GET TYPES: UIServiceAccessManager
		GET TYPES: UIStampsManager
		GET TYPES: UITransform
		GET TYPES: UIUpdatingManager		<--
		GET TYPES: UIView
		GET TYPES: UIContainer
		GET TYPES: UIContainerManager
		GET TYPES: UIPanel
		GET TYPES: UIShowable
		GET TYPES: UIWindow
		GET TYPES: UIWindowsManager`2
		GET TYPES: UIWindowsManager_Base`2
		GET TYPES: UIWindowsManager_Standalone`2
		GET TYPES: UITooltipBrick
		GET TYPES: UITooltipWindow
		GET TYPES: UIStyleManager
		GET TYPES: UIAbstractImage
		GET TYPES: UIBlur
		GET TYPES: UIImage
		GET TYPES: UIImageEffect
		GET TYPES: UILabel
		GET TYPES: UIMaterialModifier
		GET TYPES: UIPath
		GET TYPES: UIRenderer
		GET TYPES: UIRenderingManager
		GET TYPES: UIRenderState
		GET TYPES: UIRoundImage
		GET TYPES: UIScopedRenderer
		GET TYPES: UISector
		GET TYPES: UISquircleImage
		GET TYPES: UIVideo
		GET TYPES: UIMask
		GET TYPES: UIRectMask
		GET TYPES: UILayout
		GET TYPES: UILayoutCircular
		GET TYPES: UITable1D
		GET TYPES: UITable2D
		GET TYPES: UIButton
		GET TYPES: UIButtonDisk
		GET TYPES: UIControl
		GET TYPES: UIDragArea
		GET TYPES: UIDropList
		GET TYPES: UIInteractable
		GET TYPES: UIInteractivityManager
		GET TYPES: UIMouseCatcher
		GET TYPES: UIMouseCatcherDisk
		GET TYPES: UIScrollBar
		GET TYPES: UIScrollView
		GET TYPES: UISlider
		GET TYPES: UITextField
		GET TYPES: UIThreeStatesToggle
		GET TYPES: UIToggle
		GET TYPES: UITooltip
		GET TYPES: UITooltipDisk
		GET TYPES: UITooltipManager
		GET TYPES: UIBoard
		GET TYPES: UIBoardCell`1
		GET TYPES: UIBoardEntry
		GET TYPES: UIBoardHeader
		GET TYPES: UIBoardReflectionCell`1
		GET TYPES: UIBoardStringCell
		GET TYPES: UIAnimationComponent
		GET TYPES: UIAnimationManager
		GET TYPES: UIAnimatorComponent
		GET TYPES: UIAnimatorManager
		*/
	}
}
