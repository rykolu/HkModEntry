using System;
using Amplitude.UI.Animations;
using Amplitude.UI.Animations.Scene;
using Amplitude.UI.Interactables;
using Amplitude.UI.Renderers;
using Amplitude.UI.Styles.Scene;
using Amplitude.UI.Windows;
using UnityEngine;

//extra references needed
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace Amplitude.UI
{
	[ExecuteInEditMode]
	[DefaultExecutionOrder(100)]
	public class UIUpdatingManager : UIBehaviour
	{
		private static UIUpdatingManager instance;

		[NonSerialized]
		private UIHierarchyManager hierarchyManager;

		[NonSerialized]
		private UIRenderingManager renderingManager;

		[NonSerialized]
		private UIInteractivityManager interactivityManager;

		[NonSerialized]
		private UITooltipManager tooltipManager;

		[NonSerialized]
		private UIAnimationManager animationManager;

		[NonSerialized]
		private UIAnimatorManager animatorManager;

		[NonSerialized]
		private UIStyleManager styleManager;

		[NonSerialized]
		private IUIWindowsService windowsManager;

		[NonSerialized]
		private UIContainerManager containerManager;

		public static UIUpdatingManager Instance => instance;

		public static event Action PreRender;

		protected UIUpdatingManager()
		{
			instance = this;
		}

		internal void Render()
		{
			if (base.Loaded)
			{
				try
				{
					UIUpdatingManager.PreRender?.Invoke();
					renderingManager.Render();
				}
				catch (Exception exception)
				{
					Diagnostics.LogException(exception);
				}
			}
		}

		protected override void Load()
		{
			base.Load();
			hierarchyManager = UIHierarchyManager.Instance;
			hierarchyManager.LoadIfNecessary();
			renderingManager = UIRenderingManager.Instance;
			renderingManager.LoadIfNecessary();
			interactivityManager = UIInteractivityManager.Instance;
			interactivityManager.LoadIfNecessary();
			animationManager = UIAnimationManager.Instance;
			animationManager?.LoadIfNecessary();
			styleManager = UIStyleManager.Instance;
			styleManager.LoadIfNecessary();
			containerManager = UIContainerManager.Instance;
			if (containerManager != null)
			{
				containerManager.LoadIfNecessary();
			}
			tooltipManager = UITooltipManager.Instance;
			if (tooltipManager != null)
			{
				tooltipManager.LoadIfNecessary();
			}
			animatorManager = UIAnimatorManager.Instance;
			animatorManager?.LoadIfNecessary();
			windowsManager = hierarchyManager.GetComponent<IUIWindowsService>();
		}

		protected override void Unload()
		{
			hierarchyManager = null;
			renderingManager = null;
			interactivityManager = null;
			animationManager = null;
			animatorManager = null;
			styleManager = null;
			containerManager = null;
			tooltipManager = null;
			windowsManager = null;
			base.Unload();
		}

		protected override void Destruct()
		{
			instance = null;
			base.Destruct();
		}

		/*protected virtual void Update()
		{
			_ = base.Loaded;
		}*/

		protected virtual void LateUpdate()
		{
			if (base.Loaded)
			{
				try
				{
					interactivityManager?.SpecificUpdate();
					animationManager?.SpecificUpdate();
					hierarchyManager.SpecificLateUpdate();
					styleManager?.SpecificUpdate();
					windowsManager?.SpecificUpdate();
					containerManager?.SpecificUpdate();
					animatorManager?.SpecificUpdate();
					tooltipManager?.SpecificUpdate();
				}
				catch (Exception exception)
				{
					Diagnostics.LogException(exception);
				}
				if (UIView.AllActiveViews.Count > 0)
				{
					Render();
				}
			}
		}


		//BELOW ARE THE CHANGES I'VE MADE, ABOVE IS THE GAME'S ORIGINAL CODE



		private static bool HkModderInitialized = false;    //currently, this is redundant, but maybe useful later
		private static string modPath = "";                 //C:/Program Files (x86)/Steam/steamapps/common/Humankind/Mods
															//this is the string that will be passed to Mymod.Main.Load

		//all variables(in lists) needed to iterate through each mod in mod folder
		private static List<string> ListOfMods = new List<string>();					//"HkCameraMod", "HkMod1", etc
		private static List<Type> modType = new List<Type>();
		private static List<object> cInst = new List<object>();
		private static List<MethodInfo> ModLoad = new List<MethodInfo>();				//Mymod.Main.Load
		//private static List<MethodInfo> ModCheckIfLoaded = new List<MethodInfo>();	//Mymod.Main.CheckIfLoaded
		private static List<MethodInfo> ModOnUpdate = new List<MethodInfo>();           //Mymod.Main.OnToggle
		private static List<MethodInfo> ModOnToggle = new List<MethodInfo>();           //Mymod.Main.OnUpdate
		private static List<bool> OnUpdates = new List<bool>();							//[true, false, false, ...]

		protected void Start()
		{
            if (HkModderInitialized == false)
            {
                ListOfMods = HkModSetup();
                HkModEntry();
                HkModderInitialized = true;

				//runs only the "OnToggle" dlls
				for (int i = 0; i < ListOfMods.Count; i++)
                {
					if (OnUpdates[i] == false)
                    {
						ModOnToggle[i].Invoke(cInst[i], new object[] { });
					}
                }
			}
        }

		protected virtual void Update()
		{
			//from original dll
			_ = base.Loaded;

			//runs only the "OnUpdate" dlls
			for (int i = 0; i < ListOfMods.Count; i++)
            {
				if (OnUpdates[i] == true)
                {
					ModOnUpdate[i].Invoke(cInst[i], new object[] {});
				}
            }
		}



		private static List<string> HkModSetup()
		{
			List<string> modList = new List<string>();

			//creates this path (example)-->  C:/Program Files (x86)/Steam/steamapps/common/Humankind/Mods
			modPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			modPath = modPath.Replace("\\Humankind_Data\\Managed", "");
			modPath = System.IO.Path.Combine(modPath, "Mods");
			modPath = modPath.Replace("\\", "/");

			//clears ModLogger.txt 
			if (HkModderInitialized == false) { File.WriteAllText(System.IO.Path.Combine(modPath, "ModLogger.txt"), ""); }
			
			//sets up all the above lists
			DirectoryInfo md = new DirectoryInfo(modPath);
			DirectoryInfo[] modDir = md.GetDirectories();
			for (int i = 0; i < modDir.Length; i++)
			{
				try
				{
					//EXAMPLE:
					//Assembly modDLL = Assembly.LoadFile(
					//	"C:/Program Files (x86)/Steam/steamapps/common/Humankind/Mods/HkCameraMod/HkCameraMod.dll"
					//	);																	/\			/\
					//
					//this is why the file folder and .dll you want to run MUST have the same name!
					Assembly modDLL = Assembly.LoadFile(
						modPath + "/" + modDir[i].Name + "/" + modDir[i].Name + ".dll"
						); //				   /\					/\

					//how I find the dll methods at runtime
					modType.Add(modDLL.GetType(modDir[i].Name + ".Main"));
					cInst.Add(Activator.CreateInstance(modType[i]));
					ModLoad.Add(modType[i].GetMethod("Load"));
					//ModCheckIfLoaded.Add(modType[i].GetMethod("CheckIfLoaded"));
					ModOnUpdate.Add(modType[i].GetMethod("OnUpdate"));
					ModOnToggle.Add(modType[i].GetMethod("OnToggle"));
					modList.Add(modDir[i].Name);
				}
				catch (Exception e)
				{
					modList.Clear();
					modList.Add("ERROR: " + e);     //make better exception handler
					return modList;
				}
			}
			return modList;
		}



		private static void HkModEntry()
		{

			try
			{
				//how to write to ModLogger.txt w/ "System.IO.StreamWriter"
				StreamWriter HkModLogger = new StreamWriter(
					System.IO.Path.Combine(modPath, "ModLogger.txt"),
					true);
				HkModLogger.WriteLine("HkModder Initialized");

				//iterates through each mod returned by HkModSetup
				for (int i = 0; i < ListOfMods.Count; i++)
				{
					try
					{
						HkModLogger.WriteLine("FOUND: " + ListOfMods[i] + ".dll");

						//how to invoke unreferenced methods at runtime
						var returnVal = ModLoad[i].Invoke(cInst[i], new object[] { modPath });  //aka-->  Mymod.Main.Load(modPath)
						HkModLogger.WriteLine("SUCCESS: " + ListOfMods[i] + " loaded: " + returnVal);

						//see Update()
						if		(returnVal is "OnToggle") { OnUpdates.Add(false); }
						else if	(returnVal is "OnUpdate") { OnUpdates.Add(true); }
						else { throw new ArgumentException("argument passed must be \"OnUpdate\" or \"OnToggle\"", ListOfMods[i]); }
					}
					catch (Exception e)
					{
						HkModLogger.WriteLine("ERROR: " + ListOfMods[i] + " => " + e);
					}
				}

				HkModLogger.WriteLine("  >>>");
				HkModLogger.Close();
			}
			catch (Exception)
			{
				return;// ¯\_(ツ)_/¯
			}
		}


	}
}
