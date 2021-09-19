using System;
using System.Collections;
using System.Collections.Generic;
using Amplitude.Framework;
using Amplitude.Framework.Localization;
using Amplitude.Wwise.Audio;
using UnityEngine;

namespace Amplitude.UI
{
	[ExecuteInEditMode]
	public class UIServiceAccessManager : UIBehaviour
	{
		private static UIServiceAccessManager instance;

		[SerializeField]
		private GameObject managerContainerPrefab;

		[NonSerialized]
		private GameObject managerContainer;

		[NonSerialized]
		private List<Manager> managers = new List<Manager>();

		[NonSerialized]
		private ILocalizationService localizationService;

		[NonSerialized]
		private IAudioService audioService;

		public static UIServiceAccessManager Instance => instance;

		public static ILocalizationService LocalizationService
		{
			get
			{
				if (instance != null)
				{
					return instance.localizationService;
				}
				return null;
			}
		}

		public static IAudioService AudioService
		{
			get
			{
				if (instance != null)
				{
					return instance.audioService;
				}
				return null;
			}
		}

		protected UIServiceAccessManager()
		{
			instance = this;
		}

		protected override void Load()
		{
			base.Load();
			if (UnityEngine.Object.FindObjectOfType<Amplitude.Framework.Application>() == null || !UnityEngine.Application.isPlaying)
			{
				Services.Clear();
				managerContainer = UnityEngine.Object.Instantiate(managerContainerPrefab, base.transform);
				managerContainer.name = string.Format("(Autoinstanciated in {1}) {0} ", managerContainerPrefab.name, UnityEngine.Application.isPlaying ? "Play" : "Editor");
				managerContainer.hideFlags = HideFlags.DontSave;
				managerContainer.GetComponents(managers);
				Coroutine coroutine = Coroutine.StartCoroutine(DoStartManagers, DoStartManagersExceptionHandler);
				int num = 16;
				int num2 = 0;
				while (!coroutine.IsFinished)
				{
					coroutine.Run();
					num2++;
					if (coroutine.LastException != null)
					{
						throw coroutine.LastException;
					}
					if (num2 >= num)
					{
						Diagnostics.LogWarning("Unable to start managers (timeout)");
						break;
					}
				}
			}
			UnityCoroutine.StartCoroutine(this, DoWaitForBindLocalizationService);
			UnityCoroutine.StartCoroutine(this, DoWaitForBindAudioService);
		}

		protected override void Unload()
		{
			localizationService = null;
			if (managerContainer != null)
			{
				Coroutine.StartCoroutine(DoShutdownManagers, DoShutdownManagerExceptionHandler).RunUntilIsFinished();
				CleanUp();
			}
			base.Unload();
		}

		protected override void Destruct()
		{
			instance = null;
			base.Destruct();
		}

		private void CleanUp()
		{
			managers.Clear();
			Services.Clear();
			UnityEngine.Object.DestroyImmediate(managerContainer);
			managerContainer = null;
		}

		private IEnumerator DoStartManagers()
		{
			foreach (Manager manager in managers)
			{
				yield return manager.DoStart();
			}
		}

		private void DoStartManagersExceptionHandler(object sender, CoroutineExceptionEventArgs args)
		{
			CleanUp();
			Diagnostics.LogException(args.Exception);
		}

		private IEnumerator DoShutdownManagers()
		{
			foreach (Manager manager in managers)
			{
				yield return manager.DoShutdown();
			}
		}

		private void DoShutdownManagerExceptionHandler(object sender, CoroutineExceptionEventArgs args)
		{
			CleanUp();
			Diagnostics.LogException(args.Exception);
		}

		private IEnumerator DoWaitForBindLocalizationService()
		{
			yield return Services.DoWaitForBindService<ILocalizationService>();
			localizationService = Services.GetService<ILocalizationService>();
		}

		private IEnumerator DoWaitForBindAudioService()
		{
			yield return Services.DoWaitForBindService<IAudioService>();
			audioService = Services.GetService<IAudioService>();
		}

		private void OnManagerContainerChanged()
		{
			TryReload();
		}
	}
}
