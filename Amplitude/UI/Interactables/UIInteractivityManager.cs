using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Amplitude.Framework;
using Amplitude.Framework.Input;
using Amplitude.Framework.Interactions;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UIHierarchyManager))]
	[ExecuteInEditMode]
	public class UIInteractivityManager : UIBehaviour
	{
		private struct ViewedSortedResponders
		{
			public UIView View;

			public SortedSet<SortedResponder> SortedResponders;
		}

		private class SortedResponderComparer
		{
			public static int Compare(ref SortedResponder left, ref SortedResponder right)
			{
				if (left.LayerIndex == right.LayerIndex)
				{
					if (left.SortingIndex == right.SortingIndex)
					{
						return 0;
					}
					if (right.SortingIndex >= left.SortingIndex)
					{
						return 1;
					}
					return -1;
				}
				if (right.LayerIndex >= left.LayerIndex)
				{
					return 1;
				}
				return -1;
			}
		}

		public const float DoubleClickDelay = 0.3f;

		public static bool IsMouseCovered;

		public static uint SortedRespondersRevisionIndex;

		private const int UIInteractionGroup = 50;

		private static UIInteractivityManager instance;

		private IInteractionService interactionService;

		private IInputService inputService;

		private InteractionID interactionID = InteractionID.Zero;

		private Vector2 mousePosition;

		private UIControlResponder focusedResponder;

		private PerformanceList<IUIResponder> registeredResponders;

		private PerformanceList<IUIResponder> respondersToDispatch;

		private List<ViewedSortedResponders> perViewSortedResponders = new List<ViewedSortedResponders>();

		private Stack<SortedSet<SortedResponder>> sortedSetsPool = new Stack<SortedSet<SortedResponder>>();

		private InputEvent inputEvent;

		private bool[] mouseButtonDowns = new bool[3];

		private ushort modifiers;

		private uint lastSortedRespondersRevisionIndex;

		internal List<InputEvent> LoggedEvents = new List<InputEvent>();

		internal List<List<IUIResponder>> RespondersForLoggedEvents = new List<List<IUIResponder>>();

		private Stack<List<InputEvent>> poolOfInputEventsLists = new Stack<List<InputEvent>>();

		private Stack<List<IUIResponder>> poolOfUIResponderList = new Stack<List<IUIResponder>>();

		public static UIInteractivityManager Instance => instance;

		public UIControlResponder FocusedResponder => focusedResponder;

		public bool HasCaughtLastMessage { get; private set; }

		protected UIInteractivityManager()
		{
			instance = this;
		}

		public void RegisterResponder(IUIResponder responder)
		{
			int count = registeredResponders.Count;
			registeredResponders.Add(responder);
			responder.ResponderIndex = count;
			SortedRespondersRevisionIndex++;
		}

		public void UnregisterResponder(IUIResponder responder)
		{
			int responderIndex = responder.ResponderIndex;
			if (responderIndex >= 0)
			{
				if (responderIndex + 1 < registeredResponders.Count)
				{
					IUIResponder iUIResponder = registeredResponders.Data[registeredResponders.Count - 1];
					registeredResponders.Data[responderIndex] = iUIResponder;
					iUIResponder.ResponderIndex = responderIndex;
				}
				registeredResponders.Count--;
				responder.ResponderIndex = -1;
				if (focusedResponder == responder)
				{
					focusedResponder = null;
				}
				SortedRespondersRevisionIndex++;
			}
		}

		public bool TryCatchEventByOneResponder(IUIResponder responder, ref InputEvent inputEvent)
		{
			mousePosition.x = inputService.MousePosition.x;
			mousePosition.y = (float)Screen.height - inputService.MousePosition.y;
			return TryCatchEventByOneResponder(responder, ref inputEvent, mousePosition);
		}

		public bool TryCatchEventByOneResponder(IUIResponder responder, ref InputEvent inputEvent, Vector2 mousePosition)
		{
			SortRespondersIfNecessary();
			if (inputEvent.IsKeyboardEvent)
			{
				if (responder != focusedResponder)
				{
					Diagnostics.LogError("Trying to send event '{0}' on specific responder '{1}' but it is not the focused responder (which is '{2}')", inputEvent, responder, focusedResponder);
					return false;
				}
			}
			else
			{
				inputEvent.MousePosition = GetMousePositionForResponder(responder, mousePosition);
			}
			return responder.TryCatchEvent(ref inputEvent);
		}

		public void UncatchInputEvent(ulong inputEventId, IUIResponder responder)
		{
			if (inputEvent.UniqueId == inputEventId)
			{
				inputEvent.Catched = false;
			}
		}

		public void SetFocus(UIControlResponder newFocusedResponder = null)
		{
			if (newFocusedResponder != focusedResponder)
			{
				UIControlResponder uIControlResponder = focusedResponder;
				focusedResponder = newFocusedResponder;
				uIControlResponder?.OnFocusLoss();
				newFocusedResponder?.OnFocusGain();
			}
		}

		public Vector2 GetMousePosition(UIView view)
		{
			return view.ScreenPositionToStandardizedPosition(mousePosition);
		}

		public Vector2 GetMousePosition(IUIResponder responder = null)
		{
			if (responder == null)
			{
				return UIHierarchyManager.Instance.MainFullscreenView.ScreenPositionToStandardizedPosition(mousePosition);
			}
			return GetMousePositionForResponder(responder, mousePosition);
		}

		public void AcquireKeyboard(IUITextField targetTextField)
		{
			Services.GetService<IKeyboardService>()?.UsePlatformKeyboard(targetTextField.GetUITransform().GlobalRect, () => targetTextField.Text, targetTextField.MaximumChars);
		}

		public void ReleaseKeyboard()
		{
			Services.GetService<IKeyboardService>()?.TryDisposePlatformKeyboard();
		}

		internal void SpecificUpdate()
		{
			if (inputService == null)
			{
				return;
			}
			int num = mouseButtonDowns.Length;
			for (int i = 0; i < num; i++)
			{
				MouseButton mouseButton = (MouseButton)i;
				if (mouseButtonDowns[i] && inputService.GetMouseButtonUp(mouseButton))
				{
					TryCatchInputEvent(new InputEvent(InputEvent.EventType.MouseUp, mouseButton, modifiers));
					mouseButtonDowns[i] = false;
				}
			}
		}

		protected override void Load()
		{
			base.Load();
			foreach (UIView allActiveView in UIView.AllActiveViews)
			{
				OnViewAdded(allActiveView);
			}
			UIView.ViewAdded += OnViewAdded;
			UIView.ViewRemoved += OnViewRemoved;
			UIServiceAccessManager.Instance?.LoadIfNecessary();
			UnityCoroutine.StartCoroutine(this, RegisterToServices, CoroutineExceptionHandler);
		}

		protected override void Unload()
		{
			StopAllCoroutines();
			if (interactionService != null && interactionID != InteractionID.Zero)
			{
				interactionService.Unsubscribe(interactionID);
			}
			interactionService = null;
			inputService = null;
			UIView.ViewAdded -= OnViewAdded;
			UIView.ViewRemoved -= OnViewRemoved;
			registeredResponders.Clear();
			respondersToDispatch.Clear();
			perViewSortedResponders.Clear();
			base.Unload();
		}

		protected override void Destruct()
		{
			instance = null;
			base.Destruct();
		}

		private IEnumerator RegisterToServices()
		{
			yield return Services.DoWaitForBindService(delegate(IInteractionService service)
			{
				interactionService = service;
			});
			yield return Services.DoWaitForBindService(delegate(IInputService service)
			{
				inputService = service;
			});
			interactionService.Subscribe(GetType().Name, HandleInteractionMessage, 50, 10);
		}

		private void SortRespondersIfNecessary()
		{
			if (lastSortedRespondersRevisionIndex == SortedRespondersRevisionIndex)
			{
				return;
			}
			for (int i = 0; i < perViewSortedResponders.Count; i++)
			{
				perViewSortedResponders[i].SortedResponders.Clear();
				sortedSetsPool.Push(perViewSortedResponders[i].SortedResponders);
			}
			perViewSortedResponders.Clear();
			respondersToDispatch.Clear();
			if (registeredResponders.Count >= respondersToDispatch.Capacity)
			{
				respondersToDispatch.Reserve(registeredResponders.Count * 2);
			}
			Array.Copy(registeredResponders.Data, respondersToDispatch.Data, registeredResponders.Count);
			respondersToDispatch.Count = registeredResponders.Count;
			ViewedSortedResponders item = default(ViewedSortedResponders);
			foreach (UIView allActiveView in UIView.AllActiveViews)
			{
				if (!allActiveView.ReceiveInputEvents)
				{
					continue;
				}
				item.View = allActiveView;
				item.SortedResponders = ((sortedSetsPool.Count > 0) ? sortedSetsPool.Pop() : new SortedSet<SortedResponder>(SortedResponderComparer.Compare));
				int groupCullingMask = allActiveView.GroupCullingMask;
				for (int num = respondersToDispatch.Count - 1; num >= 0; num--)
				{
					IUIResponder iUIResponder = respondersToDispatch.Data[num];
					if (iUIResponder != null)
					{
						SortedResponder item2 = iUIResponder.GetSortedResponder(allActiveView, groupCullingMask);
						if (item2.Responder != null && item2.LayerIndex >= 0)
						{
							item.SortedResponders.Add(ref item2);
							respondersToDispatch.Data[num] = null;
						}
					}
				}
				item.SortedResponders.Sort();
				perViewSortedResponders.Add(item);
			}
			lastSortedRespondersRevisionIndex = SortedRespondersRevisionIndex;
		}

		private InteractionResponse HandleInteractionMessage(ref Message message)
		{
			if (!base.Loaded)
			{
				return InteractionResponse.Continue;
			}
			bool flag = false;
			SortRespondersIfNecessary();
			try
			{
				MessageID iD = (MessageID)message.ID;
				modifiers = message.Parameter.UInt16_2;
				switch (iD)
				{
				case MessageID.MouseUpdate:
					mousePosition.x = (int)message.Parameter.UInt16_0;
					mousePosition.y = Screen.height - message.Parameter.UInt16_1;
					IsMouseCovered = TryCatchInputEvent(new InputEvent(InputEvent.EventType.Tick | InputEvent.EventType.MouseHover));
					if (IsMouseCovered)
					{
						message.Parameter.UInt16_3 |= 16;
					}
					if (focusedResponder is UITextFieldResponder)
					{
						message.Parameter.UInt16_2 |= 32;
					}
					TryCatchInputEvent(new InputEvent(InputEvent.EventType.MouseHoverTooltip));
					break;
				case MessageID.MouseButtonDown:
				{
					MouseButton uInt16_3 = (MouseButton)message.Parameter.UInt16_0;
					if (uInt16_3 == MouseButton.Left)
					{
						TryCatchInputEvent(new InputEvent(InputEvent.EventType.CastFocus));
					}
					flag = TryCatchInputEvent(new InputEvent(InputEvent.EventType.MouseDown, uInt16_3, modifiers));
					mouseButtonDowns[(int)uInt16_3] = true;
					break;
				}
				case MessageID.MouseButtonUp:
				{
					MouseButton uInt16_4 = (MouseButton)message.Parameter.UInt16_0;
					if (TryCatchInputEvent(new InputEvent(InputEvent.EventType.MouseUp, uInt16_4, modifiers)))
					{
						message.Parameter.UInt16_3 |= 16;
					}
					mouseButtonDowns[(int)uInt16_4] = false;
					break;
				}
				case MessageID.MouseWheel:
				{
					float num = (float)(short)message.Parameter.UInt16_0 / 1000f;
					float num2 = (float)(short)message.Parameter.UInt16_1 / 1000f;
					flag = ((message.Parameter.UInt16_3 == 2 && !(Mathf.Abs(num) > float.Epsilon)) ? TryCatchInputEvent(new InputEvent(InputEvent.EventType.MouseScroll, num2, modifiers)) : TryCatchInputEvent(new InputEvent(InputEvent.EventType.AxisUpdate2D, num, num2, modifiers)));
					break;
				}
				case MessageID.KeyDown:
				case MessageID.KeyRepeat:
				{
					KeyCode uInt16_2 = (KeyCode)message.Parameter.UInt16_0;
					flag = TryCatchInputEvent(new InputEvent(InputEvent.EventType.KeyDown, uInt16_2, modifiers));
					break;
				}
				case MessageID.KeyUp:
				{
					KeyCode uInt16_ = (KeyCode)message.Parameter.UInt16_0;
					flag = TryCatchInputEvent(new InputEvent(InputEvent.EventType.KeyUp, uInt16_, modifiers));
					break;
				}
				case MessageID.Char:
					flag = TryCatchInputEvent(new InputEvent(InputEvent.EventType.NewChar, (char)message.Parameter.UInt16_0, modifiers));
					break;
				}
			}
			catch (Exception exception)
			{
				Diagnostics.LogException(exception);
			}
			if (flag)
			{
				HasCaughtLastMessage = true;
				message.Parameter.UInt16_3 |= 16;
				return InteractionResponse.Break;
			}
			HasCaughtLastMessage = false;
			return InteractionResponse.Continue;
		}

		private bool TryCatchInputEvent(InputEvent newInputEvent)
		{
			inputEvent = newInputEvent;
			if (inputEvent.IsKeyboardEvent)
			{
				if (focusedResponder != null)
				{
					return focusedResponder.TryCatchEvent(ref inputEvent);
				}
				return false;
			}
			for (int num = perViewSortedResponders.Count - 1; num >= 0; num--)
			{
				ViewedSortedResponders viewedSortedResponders = perViewSortedResponders[num];
				inputEvent.MousePosition = viewedSortedResponders.View.ScreenPositionToStandardizedPosition(mousePosition);
				int count = viewedSortedResponders.SortedResponders.Count;
				SortedResponder[] data = viewedSortedResponders.SortedResponders.Data;
				for (int i = 0; i < count; i++)
				{
					if ((data[i].EventSensitivity & newInputEvent.Type) != 0 && data[i].Responder.TryCatchEvent(ref inputEvent))
					{
						inputEvent.Catched = true;
					}
				}
			}
			return inputEvent.Catched;
		}

		private Vector2 GetMousePositionForResponder(IUIResponder responder, Vector2 mousePositionOnScreen)
		{
			foreach (UIView allActiveView in UIView.AllActiveViews)
			{
				if (allActiveView.ReceiveInputEvents)
				{
					return allActiveView.ScreenPositionToStandardizedPosition(mousePositionOnScreen);
				}
			}
			Diagnostics.LogError("No view available.", responder);
			return default(Vector2);
		}

		private void OnViewAdded(UIView view)
		{
			view.LayersChanged += OnViewLayersChanged;
			SortedRespondersRevisionIndex++;
		}

		private void OnViewRemoved(UIView view)
		{
			view.LayersChanged -= OnViewLayersChanged;
			SortedRespondersRevisionIndex++;
		}

		private void OnViewLayersChanged(UIView view)
		{
			SortedRespondersRevisionIndex++;
		}

		private void CoroutineExceptionHandler(object sender, CoroutineExceptionEventArgs args)
		{
			Diagnostics.LogException(args.Exception);
		}

		[Conditional("UNITY_EDITOR")]
		private void ClearLoggedEvents()
		{
			LoggedEvents.Clear();
			int count = RespondersForLoggedEvents.Count;
			for (int i = 0; i < count; i++)
			{
				List<IUIResponder> list = RespondersForLoggedEvents[i];
				if (list != null)
				{
					list.Clear();
					poolOfUIResponderList.Push(list);
				}
			}
			RespondersForLoggedEvents.Clear();
		}

		[Conditional("UNITY_EDITOR")]
		private void LogEventRaised(ref InputEvent inputEvent)
		{
			LoggedEvents.Add(inputEvent);
			RespondersForLoggedEvents.Add(null);
		}

		[Conditional("UNITY_EDITOR")]
		private void LogEventCatchedBy(ref InputEvent inputEvent, IUIResponder responder)
		{
			int indexOfLoggedEvent = GetIndexOfLoggedEvent(ref inputEvent);
			if (RespondersForLoggedEvents[indexOfLoggedEvent] == null)
			{
				RespondersForLoggedEvents[indexOfLoggedEvent] = ((poolOfUIResponderList.Count > 0) ? poolOfUIResponderList.Pop() : new List<IUIResponder>());
			}
			RespondersForLoggedEvents[indexOfLoggedEvent].Add(responder);
		}

		[Conditional("UNITY_EDITOR")]
		private void LogEventUncatchedBy(ref InputEvent inputEvent, IUIResponder responder)
		{
			int indexOfLoggedEvent = GetIndexOfLoggedEvent(ref inputEvent);
			RespondersForLoggedEvents[indexOfLoggedEvent].Add(responder);
		}

		private int GetIndexOfLoggedEvent(ref InputEvent inputEvent)
		{
			for (int i = 0; i < LoggedEvents.Count; i++)
			{
				if (LoggedEvents[i].UniqueId == inputEvent.UniqueId)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
