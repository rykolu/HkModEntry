using System;
using Amplitude.Framework.Input;
using Amplitude.UI.Audio;

namespace Amplitude.UI.Interactables
{
	public class UIDropListResponder : UIControlResponder
	{
		private bool closePopupOnNextTick;

		public IUIDropList DropList => base.Interactable as IUIDropList;

		public event Action<IUIDropList, int> SelectionChange;

		public UIDropListResponder(IUIDropList dropList)
			: base(dropList)
		{
		}

		public void Start()
		{
			if (DropList.DropDownToggle != null && DropList.DropDownToggle.Loaded)
			{
				DropList.DropDownToggle.Switch += DropDownToggle_Switch;
			}
			DropList.UpdatePopupVisibility(value: false, instant: true);
		}

		public void Stop()
		{
			if (DropList.DropDownToggle != null && DropList.DropDownToggle.Loaded)
			{
				DropList.DropDownToggle.Switch -= DropDownToggle_Switch;
			}
		}

		public void OnItemsCollectionChanged(ref PerformanceList<UITransform> items)
		{
			int count = items.Count;
			for (int i = 0; i < count; i++)
			{
				UITransform uITransform = items.Data[i];
				IUIToggle component = items.Data[i].GetComponent<IUIToggle>();
				if (component == null)
				{
					Diagnostics.LogError("The UIDropList item '{0}' is not a IUIToggle.", items.Data[i]);
				}
				component.Switch -= ItemToggle_Switch;
				component.FocusGain -= ItemToggle_FocusGain;
				component.FocusLoss -= ItemToggle_FocusLoss;
				if (uITransform.VisibleSelf)
				{
					component.Switch += ItemToggle_Switch;
					component.FocusGain += ItemToggle_FocusGain;
					component.FocusLoss += ItemToggle_FocusLoss;
				}
			}
		}

		public void OnSelectedItemChanged(int selectedIndex)
		{
			this.SelectionChange?.Invoke(DropList, selectedIndex);
		}

		public override bool TryCatchEvent(ref InputEvent inputEvent)
		{
			if (DropList.Popup.VisibleSelf)
			{
				if (inputEvent.Type == InputEvent.EventType.MouseDown && inputEvent.Button == MouseButton.Left)
				{
					if (!UIHierarchyManager.Instance.MainFullscreenView.StandardizedRect.Contains(inputEvent.MousePosition) || (!Contains(inputEvent.MousePosition) && !DropList.Popup.Contains(inputEvent.MousePosition)))
					{
						UpdatePopupVisibility(value: false);
					}
				}
				else if (inputEvent.Type == InputEvent.EventType.MouseUp && inputEvent.Button == MouseButton.Right && DropList.CloseOnRightClickUp)
				{
					UpdatePopupVisibility(value: false);
					return true;
				}
			}
			return base.TryCatchEvent(ref inputEvent);
		}

		protected override void OnTick(ref InputEvent tickEvent)
		{
			base.OnTick(ref tickEvent);
			if (closePopupOnNextTick)
			{
				closePopupOnNextTick = false;
				DropList.UpdatePopupVisibility(value: false);
				DropList.TryTriggerAudioEvent(AudioEvent.Interactivity.DroplistClose);
			}
		}

		private void UpdatePopupVisibility(bool value)
		{
			if (value)
			{
				closePopupOnNextTick = false;
				DropList.UpdatePopupVisibility(value: true);
			}
			else
			{
				closePopupOnNextTick = true;
			}
		}

		private void DropDownToggle_Switch(IUIToggle dropDownToggle, bool state)
		{
			if (state)
			{
				UpdatePopupVisibility(value: true);
				DropList.TryTriggerAudioEvent(AudioEvent.Interactivity.DroplistOpen);
			}
			else
			{
				UpdatePopupVisibility(value: false);
				DropList.TryTriggerAudioEvent(AudioEvent.Interactivity.DroplistClose);
			}
		}

		private void ItemToggle_FocusGain(IUIControl itemToggle)
		{
			UpdatePopupVisibility(value: true);
		}

		private void ItemToggle_FocusLoss(IUIControl itemToggle)
		{
			UpdatePopupVisibility(value: false);
		}

		private void ItemToggle_Switch(IUIToggle itemToggle, bool state)
		{
			UpdatePopupVisibility(value: false);
			DropList.TryTriggerAudioEvent(AudioEvent.Interactivity.DroplistItemPick);
			DropList.SelectItem(itemToggle);
		}
	}
}
