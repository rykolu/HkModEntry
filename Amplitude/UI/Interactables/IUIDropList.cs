using System;
using System.Collections;

namespace Amplitude.UI.Interactables
{
	public interface IUIDropList : IUIControl, IUIInteractable
	{
		IUIToggle DropDownToggle { get; }

		UITransform CurrentItem { get; }

		UITransform Popup { get; }

		UITransform ItemsTable { get; }

		int SelectedIndex { get; }

		object SelectedEntry { get; }

		int EntriesCount { get; }

		bool CloseOnRightClickUp { get; }

		event Action<IUIDropList, int> SelectionChange;

		IEnumerator DoReserveItems(int wantedNumber);

		void Bind(IEnumerable sourceEntries);

		void Unbind();

		void SelectIndex(int newIndex, bool silent = false, bool refresh = true);

		void SelectEntry(object entry, bool silent = false, bool refresh = true);

		void SelectItem(IUIToggle item, bool silent = false, bool refresh = true);

		void EnableIndex(int index, bool enable);

		void EnableEntry(object entry, bool enable);

		void AddEntry(object entry);

		void InsertEntry(int index, object entry);

		void RemoveEntry(object entry);

		void ClearEntries();

		void UpdatePopupVisibility(bool value, bool instant = false);
	}
}
