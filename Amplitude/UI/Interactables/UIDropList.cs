using System;
using System.Collections;
using System.Collections.Generic;
using Amplitude.UI.Audio;
using Amplitude.UI.Windows;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UITransform))]
	public class UIDropList : UIControl, IUIDropList, IUIControl, IUIInteractable, IAudioControllerOwner
	{
		public delegate void BindItemDelegate(UITransform item, object entry);

		public delegate void UnbindItemDelegate(UITransform item);

		[SerializeField]
		private UIToggle dropDownToggle;

		[SerializeField]
		private UITransform currentItem;

		[SerializeField]
		private UITransform popup;

		[SerializeField]
		private UITransform itemsTable;

		[SerializeField]
		private Transform itemTemplate;

		[SerializeField]
		private float screenOverflowMargin = 4f;

		[SerializeField]
		[HideInInspector]
		private AudioController audioController;

		[SerializeField]
		private bool closeOnRightClickUp = true;

		private UIShowable popupShowable;

		private UIPanel popupPanel;

		private UIScrollView popupScrollView;

		private List<object> entries = new List<object>();

		private BindItemDelegate bindItemDelegate;

		private UnbindItemDelegate unbindItemDelegate;

		private int selectedIndex = -1;

		public AudioController AudioController => audioController;

		public IUIToggle DropDownToggle => dropDownToggle;

		public UITransform CurrentItem => currentItem;

		public UITransform Popup => popup;

		public UITransform ItemsTable => itemsTable;

		public int SelectedIndex => selectedIndex;

		public object SelectedEntry
		{
			get
			{
				if (selectedIndex < 0 || selectedIndex > entries.Count)
				{
					return null;
				}
				return entries[selectedIndex];
			}
		}

		public int EntriesCount => entries.Count;

		public bool CloseOnRightClickUp
		{
			get
			{
				return closeOnRightClickUp;
			}
			set
			{
				closeOnRightClickUp = value;
			}
		}

		private bool IsBound => entries.Count > 0;

		private UIDropListResponder DropListResponder => (UIDropListResponder)base.Responder;

		public event Action<IUIDropList, int> SelectionChange
		{
			add
			{
				DropListResponder.SelectionChange += value;
			}
			remove
			{
				DropListResponder.SelectionChange -= value;
			}
		}

		public void Configure(BindItemDelegate bindItemDelegate, UnbindItemDelegate unbindItemDelegate)
		{
			if (IsBound)
			{
				Diagnostics.LogError($"Trying to reconfigure UIDropList '{this}' while it is already bound.");
				return;
			}
			this.bindItemDelegate = bindItemDelegate;
			this.unbindItemDelegate = unbindItemDelegate;
		}

		public IEnumerator DoReserveItems(int wantedNumber)
		{
			yield return itemsTable.DoReserveChildren(wantedNumber, itemTemplate, itemTemplate.name);
		}

		public void Bind(IEnumerable sourceEntries)
		{
			if (IsBound)
			{
				Unbind();
			}
			foreach (object sourceEntry in sourceEntries)
			{
				entries.Add(sourceEntry);
			}
			RefreshItems();
			RefreshSelection();
		}

		public void Unbind()
		{
			entries.Clear();
			selectedIndex = -1;
			RefreshItems();
		}

		public void RefreshItems()
		{
			if (!(itemsTable == null))
			{
				if (popupScrollView != null)
				{
					AdjustPopupScrollViewMaxHeight();
				}
				int count = entries.Count;
				itemsTable.ReserveChildren(count, itemTemplate, itemTemplate.name);
				int count2 = itemsTable.Children.Count;
				for (int i = 0; i < count; i++)
				{
					UITransform uITransform = itemsTable.Children.Data[i];
					unbindItemDelegate?.Invoke(uITransform);
					bindItemDelegate?.Invoke(uITransform, entries[i]);
					uITransform.VisibleSelf = true;
				}
				for (int j = count; j < count2; j++)
				{
					UITransform uITransform2 = itemsTable.Children.Data[j];
					uITransform2.VisibleSelf = false;
					unbindItemDelegate?.Invoke(uITransform2);
				}
				DropListResponder.OnItemsCollectionChanged(ref itemsTable.Children);
				popupScrollView?.Reset();
			}
		}

		public void SelectIndex(int newIndex, bool silent = false, bool refresh = true)
		{
			int num = Mathf.Max(newIndex, -1);
			if (num >= entries.Count)
			{
				Diagnostics.LogError("Trying to select index {0} in UIDropList '{1}' which only has {2} entries.", newIndex, this, entries.Count);
			}
			else if (selectedIndex != num)
			{
				_ = selectedIndex;
				selectedIndex = num;
				if (refresh)
				{
					RefreshSelection();
				}
				if (!silent)
				{
					DropListResponder.OnSelectedItemChanged(selectedIndex);
				}
			}
		}

		public void SelectEntry(object entry, bool silent = false, bool refresh = true)
		{
			int num = entries.IndexOf(entry);
			if (num == -1)
			{
				if (entry == null)
				{
					SelectIndex(-1);
					return;
				}
				Diagnostics.LogError("Could not find entry '{0}' in UIDropList '{1}'", entry, this);
			}
			else
			{
				SelectIndex(num, silent, refresh);
			}
		}

		public void SelectItem(IUIToggle item, bool silent = false, bool refresh = true)
		{
			int count = itemsTable.Children.Count;
			for (int i = 0; i < count; i++)
			{
				if (itemsTable.Children.Data[i].GetComponent<IUIToggle>() == item)
				{
					SelectIndex(i, silent, refresh);
					return;
				}
			}
			Diagnostics.LogError("Could not find item '{0}' in UIDropList '{1}'", item, this);
		}

		public void EnableIndex(int index, bool enable)
		{
			if (index < 0)
			{
				Diagnostics.LogError("Trying to {0} the negative index {1} in UIDropList '{2}'.", enable ? "enable" : "disable", index, this);
			}
			else if (index > entries.Count)
			{
				Diagnostics.LogError("Trying to {0} index {1} in UIDropList '{2}' which only has {3} entries.", enable ? "enable" : "disable", index, this, entries.Count);
			}
			else
			{
				itemsTable.Children.Data[index].InteractiveSelf = enable;
			}
		}

		public void EnableEntry(object entry, bool enable)
		{
			int num = entries.IndexOf(entry);
			if (num == -1)
			{
				Diagnostics.LogError("Could not find entry '{0}' in UIDropList '{1}'", entry, this);
			}
			else
			{
				EnableIndex(num, enable);
			}
		}

		public void UpdatePopupVisibility(bool visible, bool instant = false)
		{
			if (dropDownToggle != null && dropDownToggle.Loaded)
			{
				dropDownToggle.State = visible;
			}
			bool flag = false;
			if (popupShowable != null && popupShowable.Loaded)
			{
				flag = popupShowable.Shown != visible;
				popupShowable.UpdateVisibility(visible, instant);
			}
			else if (popupPanel != null && popupPanel.Loaded)
			{
				flag = popupPanel.Shown != visible;
				popupPanel.UpdateVisibility(visible, instant);
			}
			else
			{
				flag = popup.VisibleSelf != visible;
				popup.VisibleSelf = visible;
			}
			if (popupScrollView != null && flag)
			{
				if (visible)
				{
					popup.GlobalPositionOrSizeChange += Popup_GlobalPositionOrSizeChange;
					Popup_GlobalPositionOrSizeChange(positionChanged: true, sizeChanged: false);
				}
				else
				{
					popup.GlobalPositionOrSizeChange -= Popup_GlobalPositionOrSizeChange;
				}
			}
		}

		public void AddEntry(object entry)
		{
			entries.Add(entry);
			RefreshItems();
		}

		public void InsertEntry(int index, object entry)
		{
			if (index < 0)
			{
				Diagnostics.LogError("Trying to insert entry '{0}' at negative index {1} in UIDropList '{2}'", entry, index, this);
			}
			else if (index > entries.Count)
			{
				Diagnostics.LogError("Trying to insert entry '{0}' at index {1} in UIDropList '{2}' which only has {3} entries.", entry, index, this, entries.Count);
			}
			else
			{
				entries.Insert(index, entry);
				RefreshItems();
			}
		}

		public void RemoveEntry(object entry)
		{
			if (!entries.Remove(entry))
			{
				Diagnostics.LogError("Failed to remove entry '{0}' in UIDropList '{1}'", entry, this);
			}
			else
			{
				RefreshItems();
			}
		}

		public void ClearEntries()
		{
			entries.Clear();
			RefreshItems();
		}

		public void SetAudioProfile(AudioProfile audioProfile)
		{
			audioController.Profile = audioProfile;
		}

		void IAudioControllerOwner.InitializeAudioProfile(AudioProfile audioProfile)
		{
			audioProfile.Initialize(AudioEvent.Interactivity.MouseEnter, AudioEvent.Interactivity.MouseLeave, AudioEvent.Interactivity.DroplistOpen, AudioEvent.Interactivity.DroplistItemPick, AudioEvent.Interactivity.DroplistClose);
		}

		protected override IUIResponder InstantiateResponder()
		{
			return new UIDropListResponder(this);
		}

		protected override void Load()
		{
			dropDownToggle?.LoadIfNecessary();
			currentItem?.LoadIfNecessary();
			popup?.LoadIfNecessary();
			itemsTable?.LoadIfNecessary();
			base.Load();
			if (popup != null)
			{
				popupShowable = popup.GetComponent<UIShowable>();
				popupPanel = popup.GetComponent<UIPanel>();
				popupScrollView = popup.GetComponent<UIScrollView>();
				if (popupScrollView != null)
				{
					popupScrollView.LoadIfNecessary();
					popupScrollView.AutoAdjustHeight = true;
				}
			}
			if (itemsTable != null)
			{
				foreach (Transform item in itemsTable.transform)
				{
					item.GetComponent<UITransform>()?.LoadIfNecessary();
				}
			}
			if (itemTemplate != null)
			{
				itemTemplate.GetComponent<IUIToggle>().Focusable = false;
			}
			if (dropDownToggle != null && dropDownToggle.Loaded && currentItem != null && currentItem.Loaded && popup != null && popup.Loaded && itemsTable != null && itemsTable.Loaded && itemTemplate != null)
			{
				DropListResponder.Start();
			}
		}

		protected override void Unload()
		{
			popup.GlobalPositionOrSizeChange -= Popup_GlobalPositionOrSizeChange;
			DropListResponder.Stop();
			if (IsBound)
			{
				Unbind();
			}
			popupScrollView = null;
			popupShowable = null;
			popupPanel = null;
			base.Unload();
		}

		protected override void OnTransformVisibleGloballyChanged(bool previouslyVisible, bool currentlyVisible)
		{
			base.OnTransformVisibleGloballyChanged(previouslyVisible, currentlyVisible);
			if (!currentlyVisible)
			{
				UpdatePopupVisibility(visible: false, instant: true);
			}
		}

		private void AdjustPopupScrollViewMaxHeight()
		{
			float height = UIHierarchyManager.Instance.MainFullscreenView.StandardizedRect.height;
			float yMin = popup.UITransform.GlobalRect.yMin;
			popupScrollView.MaxHeight = height - yMin - screenOverflowMargin;
		}

		private void RefreshSelection()
		{
			if (itemsTable != null)
			{
				int count = itemsTable.Children.Count;
				for (int i = 0; i < count; i++)
				{
					itemsTable.Children.Data[i].GetComponent<IUIToggle>().State = selectedIndex == i;
				}
			}
			if (currentItem != null)
			{
				unbindItemDelegate?.Invoke(currentItem);
				if (selectedIndex >= 0)
				{
					bindItemDelegate?.Invoke(currentItem, SelectedEntry);
				}
			}
		}

		private void Popup_GlobalPositionOrSizeChange(bool positionChanged, bool sizeChanged)
		{
			if (positionChanged && popupScrollView != null)
			{
				AdjustPopupScrollViewMaxHeight();
			}
		}
	}
}
