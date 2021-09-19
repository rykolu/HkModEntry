using System.Collections;
using Amplitude.Framework;
using Amplitude.Framework.Game;
using Amplitude.Framework.Runtime;

namespace Amplitude.UI.Windows
{
	public abstract class UIWindowsManager<WindowsGroupType, SharedDataType> : UIWindowsManager_Base<WindowsGroupType, SharedDataType>, IUIWindowsGroupClient, IUIWindowsService, IService where WindowsGroupType : UIWindowsGroup, IUIWindowGroupUpdatable<SharedDataType>
	{
		private IRuntimeService runtimeService;

		private IGameService gameService;

		private UnityCoroutine onGameCreatedCoroutine;

		protected IRuntimeService RuntimeService => runtimeService;

		protected IGameService GameService => gameService;

		protected override void PostLoad(int criticity)
		{
			if (runtimeService == null)
			{
				runtimeService = Services.GetService<IRuntimeService>();
				if (runtimeService != null)
				{
					runtimeService.RuntimeChange += RuntimeService_RuntimeChange;
					if (runtimeService.Runtime != null && runtimeService.Runtime.HasBeenLoaded)
					{
						OnRuntimeLoaded();
					}
				}
			}
			if (gameService == null)
			{
				gameService = Services.GetService<IGameService>();
				if (gameService != null)
				{
					gameService.GameChange += GameService_GameChange;
				}
			}
		}

		protected override void PreUnload()
		{
			if (gameService != null)
			{
				gameService.GameChange -= GameService_GameChange;
				gameService = null;
			}
			if (runtimeService != null)
			{
				runtimeService.RuntimeChange -= RuntimeService_RuntimeChange;
				runtimeService = null;
			}
		}

		protected virtual void OnRuntimeStateChanged(FiniteState newState)
		{
			foreach (WindowsGroupType item in groupsStack)
			{
				item.OnRuntimeStateChanged(newState);
			}
		}

		protected virtual IEnumerator OnGameStarted()
		{
			foreach (WindowsGroupType item in groupsStack)
			{
				yield return item.OnGameCreated(gameService.Game);
			}
		}

		protected virtual void OnGameShuttingDown()
		{
			if (onGameCreatedCoroutine != null && !onGameCreatedCoroutine.IsFinished)
			{
				StopCoroutine(onGameCreatedCoroutine.Coroutine);
				onGameCreatedCoroutine = null;
			}
			foreach (WindowsGroupType item in groupsStack)
			{
				item.OnGameShuttingDown();
			}
		}

		private void OnRuntimeLoaded()
		{
			runtimeService.Runtime.FiniteStateMachine.StateChange += RuntimeService_RuntimeStateChange;
			if (runtimeService.Runtime.FiniteStateMachine.CurrentState != null)
			{
				OnRuntimeStateChanged(runtimeService.Runtime.FiniteStateMachine.CurrentState);
			}
		}

		private void OnRuntimeUnloading()
		{
			runtimeService.Runtime.FiniteStateMachine.StateChange -= RuntimeService_RuntimeStateChange;
		}

		private void RuntimeService_RuntimeChange(object sender, RuntimeChangeEventArgs e)
		{
			if (e.Action == RuntimeChangeAction.Loaded)
			{
				OnRuntimeLoaded();
			}
			else if (e.Action == RuntimeChangeAction.Unloading)
			{
				OnRuntimeUnloading();
			}
		}

		private void RuntimeService_RuntimeStateChange(object sender, FiniteStateChangeEventArgs args)
		{
			if (args.Action == FiniteStateChangeAction.Begun)
			{
				OnRuntimeStateChanged(args.State);
			}
		}

		private void GameService_GameChange(object sender, GameChangeEventArgs e)
		{
			switch (e.Action)
			{
			case GameChangeAction.Started:
				onGameCreatedCoroutine = UnityCoroutine.StartCoroutine(this, OnGameStarted, GameCreatedCoroutineExceptionHandler);
				break;
			case GameChangeAction.ShuttingDown:
				OnGameShuttingDown();
				break;
			}
		}

		private void GameCreatedCoroutineExceptionHandler(object sender, CoroutineExceptionEventArgs args)
		{
			Diagnostics.LogException(args.Exception);
			onGameCreatedCoroutine = null;
		}
	}
}
