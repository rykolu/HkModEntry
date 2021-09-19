using System.Collections.Generic;
using Amplitude.UI.Boards.Filters;
using Amplitude.UI.Interactables;
using UnityEngine;

namespace Amplitude.UI.Boards
{
	internal class UIBoardImpl<DataType> : IUIBoardImpl where DataType : class
	{
		private class EntryBinding
		{
			private DataType data;

			private IUIBoardEntry entry;

			public DataType Data => data;

			public IUIBoardEntry Entry => entry;

			public bool IsBound => entry != null;

			public EntryBinding(DataType data)
			{
				this.data = data;
				entry = null;
			}

			public void Bind(IUIBoardEntry entry, IUIBoardEntryEnabler<DataType> validator)
			{
				this.entry = entry;
				this.entry.Transform.VisibleSelf = false;
				this.entry.Bind(data);
				this.entry.SetSelected(value: false);
				Refresh(validator);
				this.entry.Transform.VisibleSelf = true;
			}

			public IUIBoardEntry Unbind()
			{
				IUIBoardEntry iUIBoardEntry = entry;
				if (iUIBoardEntry != null)
				{
					entry.Transform.VisibleSelf = false;
					entry.SetSelected(value: false);
					entry.Unbind();
					entry = null;
				}
				return iUIBoardEntry;
			}

			public void Refresh(IUIBoardEntryEnabler<DataType> validator)
			{
				if (entry != null)
				{
					entry.Refresh();
					validator.RefreshEnabled(data, entry);
				}
			}

			public void Filter(UIBoardFilterController filters)
			{
				if (entry != null)
				{
					bool flag = entry.Filter(filters);
					entry.Transform.VisibleSelf = flag;
					if (!flag)
					{
						entry.SetSelected(value: false, silent: false);
					}
				}
			}
		}

		private class DummyValidator : IUIBoardEntryEnabler<DataType>
		{
			public void RefreshEnabled(DataType data, IUIBoardEntry entry)
			{
				entry.SetEnabled(valid: true, UITooltipData.Empty);
			}
		}

		private IUIBoardProxy proxy;

		private IUIBoardEntryEnabler<DataType> validator;

		private List<EntryBinding> bindings = new List<EntryBinding>();

		private Queue<IUIBoardEntry> freeEntries = new Queue<IUIBoardEntry>();

		private List<DataType> selection;

		private bool dirtyBindings;

		public int EntriesCount
		{
			get
			{
				if (bindings == null)
				{
					return 0;
				}
				return bindings.Count;
			}
		}

		public UIBoardImpl(IUIBoardProxy proxy, IUIBoardEntryEnabler<DataType> validator)
		{
			this.proxy = proxy;
			this.proxy.Filters.FilterChanged += OnFilterChanged;
			IUIBoardEntryEnabler<DataType> iUIBoardEntryEnabler2;
			if (validator == null)
			{
				IUIBoardEntryEnabler<DataType> iUIBoardEntryEnabler = new DummyValidator();
				iUIBoardEntryEnabler2 = iUIBoardEntryEnabler;
			}
			else
			{
				iUIBoardEntryEnabler2 = validator;
			}
			this.validator = iUIBoardEntryEnabler2;
			LoadHeaders();
			if (this.proxy.Interactivity != 0)
			{
				selection = new List<DataType>();
			}
		}

		public void Dispose()
		{
			UnloadHeaders();
			Clear();
			while (freeEntries.Count > 0)
			{
				IUIBoardEntry iUIBoardEntry = freeEntries.Dequeue();
				iUIBoardEntry.Select -= OnEntrySelected;
				iUIBoardEntry.Switch -= OnEntrySwitched;
				iUIBoardEntry.Unload();
				Object.Destroy(iUIBoardEntry.Transform.gameObject);
			}
			validator = null;
			proxy.Filters.FilterChanged -= OnFilterChanged;
			proxy = null;
		}

		public void ReserveLines(int finalCount)
		{
			if (finalCount <= 0)
			{
				return;
			}
			int count = proxy.EntryTable.Children.Count;
			int num = finalCount - count;
			if (num <= 0)
			{
				return;
			}
			IUIBoardEntry[] array = new IUIBoardEntry[num];
			proxy.EntryTable.ReserveChildren(finalCount, proxy.EntryPrefab.transform, "Line");
			for (int i = 0; i < num; i++)
			{
				array[i] = proxy.EntryTable.Children.Data[count + i].GetComponent<IUIBoardEntry>();
			}
			IUIBoardEntry[] array2 = array;
			foreach (IUIBoardEntry iUIBoardEntry in array2)
			{
				iUIBoardEntry.Load(proxy);
				UIBoard.UIBoardInteractivityType interactivity = proxy.Interactivity;
				if ((uint)(interactivity - 1) <= 1u)
				{
					iUIBoardEntry.Switch += OnEntrySwitched;
					iUIBoardEntry.Select += OnEntrySelected;
				}
				freeEntries.Enqueue(iUIBoardEntry);
			}
		}

		public void Add(DataType newData, bool refreshNow)
		{
			bindings.Add(new EntryBinding(newData));
			dirtyBindings = true;
			if (refreshNow)
			{
				Refresh();
			}
		}

		public void AddRange(IEnumerable<DataType> newData, bool refreshNow)
		{
			foreach (DataType newDatum in newData)
			{
				Add(newDatum, refreshNow: false);
			}
			if (refreshNow)
			{
				Refresh();
			}
		}

		public void Remove(DataType data)
		{
			int num = bindings.FindIndex((EntryBinding b) => b.Data == data);
			if (num >= 0)
			{
				freeEntries.Enqueue(bindings[num].Unbind());
				if (selection.Remove(data))
				{
					proxy.OnSelectionChanged();
				}
				bindings.RemoveAt(num);
			}
		}

		public void Clear()
		{
			int entriesCount = EntriesCount;
			for (int i = 0; i < entriesCount; i++)
			{
				EntryBinding entryBinding = bindings[i];
				if (entryBinding.IsBound)
				{
					freeEntries.Enqueue(entryBinding.Unbind());
					if (selection.Remove(entryBinding.Data))
					{
						proxy.OnSelectionChanged();
					}
				}
			}
			bindings.Clear();
		}

		public void Refresh()
		{
			if (dirtyBindings)
			{
				Rebind();
				OnFilterChanged();
				dirtyBindings = false;
			}
			int entriesCount = EntriesCount;
			for (int i = 0; i < entriesCount; i++)
			{
				bindings[i].Refresh(validator);
			}
			proxy.HeaderTable.RefreshChildren(proxy.Definition.ColumnDefinitions, delegate(UIBoardHeader header, UIBoardColumnDefinition definition, int index)
			{
				header.Refresh();
			});
		}

		public void Sort()
		{
			if (proxy.Comparer.OrdersCount <= 0)
			{
				return;
			}
			bindings.Sort((EntryBinding lhs, EntryBinding rhs) => proxy.Comparer.Compare(lhs.Entry, rhs.Entry));
			using (new UITransform.ScopedCostlyOperationsOnSubHierarchy(proxy.EntryTable))
			{
				for (int i = 0; i < bindings.Count; i++)
				{
					bindings[i].Entry.Transform.transform.SetSiblingIndex(i);
				}
			}
		}

		public void Select(DataType data, bool select)
		{
			int count = bindings.Count;
			for (int i = 0; i < count; i++)
			{
				EntryBinding entryBinding = bindings[i];
				if (entryBinding.Data == data)
				{
					entryBinding.Entry.SetSelected(select, silent: false);
				}
			}
		}

		public DataType[] GetCurrentSelection()
		{
			if (selection != null && selection.Count > 0)
			{
				return selection.ToArray();
			}
			return null;
		}

		private void LoadHeaders()
		{
			if (proxy.HeaderTable != null && proxy.HeaderPrefab != null)
			{
				int columnsCount = proxy.Definition.ColumnsCount;
				proxy.HeaderTable.ReserveChildren(columnsCount, proxy.HeaderPrefab.transform, "Header");
				proxy.HeaderTable.RefreshChildren<UIBoardHeader, UIBoardColumnDefinition>(proxy.Definition.ColumnDefinitions, LoadHeader);
			}
		}

		private void UnloadHeaders()
		{
			for (int num = proxy.HeaderTable.Children.Count - 1; num >= 0; num--)
			{
				proxy.HeaderTable.Children.Data[num].GetComponent<UIBoardHeader>().Unload();
				Object.Destroy(proxy.HeaderTable.Children.Data[num].gameObject);
			}
		}

		private void LoadHeader(UIBoardHeader header, UIBoardColumnDefinition columnDefinition, int index)
		{
			header.Load(proxy, columnDefinition);
		}

		private void Rebind()
		{
			int entriesCount = EntriesCount;
			ReserveLines(entriesCount);
			for (int i = 0; i < entriesCount; i++)
			{
				EntryBinding entryBinding = bindings[i];
				if (!entryBinding.IsBound)
				{
					IUIBoardEntry entry = freeEntries.Dequeue();
					entryBinding.Bind(entry, validator);
				}
			}
			if (proxy.Comparer.OrdersCount > 0)
			{
				proxy.Sort();
			}
			dirtyBindings = false;
		}

		private EntryBinding FindBinding(IUIBoardEntry entry)
		{
			int entriesCount = EntriesCount;
			for (int i = 0; i < entriesCount; i++)
			{
				if (bindings[i].Entry == entry)
				{
					return bindings[i];
				}
			}
			return null;
		}

		private EntryBinding FindBinding(DataType data)
		{
			int entriesCount = EntriesCount;
			for (int i = 0; i < entriesCount; i++)
			{
				if (bindings[i].Data == data)
				{
					return bindings[i];
				}
			}
			return null;
		}

		private void OnFilterChanged(IUIBoardFilter filter = null)
		{
			int entriesCount = EntriesCount;
			for (int i = 0; i < entriesCount; i++)
			{
				bindings[i].Filter(proxy.Filters);
			}
		}

		private void OnEntrySwitched(IUIBoardEntry activeEntry, bool selected)
		{
			switch (proxy.Interactivity)
			{
			case UIBoard.UIBoardInteractivityType.RadioSelection:
				OnEntrySelectedRadioMode(activeEntry, selected);
				break;
			case UIBoard.UIBoardInteractivityType.MultiSelection:
				OnEntrySelectedMultiSelectionMode(activeEntry, selected);
				break;
			}
		}

		private void OnEntrySelected(IUIBoardEntry entry)
		{
			if (selection.Count > 0)
			{
				int entriesCount = EntriesCount;
				for (int i = 0; i < entriesCount; i++)
				{
					EntryBinding entryBinding = bindings[i];
					if (entryBinding.Entry != entry)
					{
						entryBinding.Entry.SetSelected(value: false);
					}
				}
				selection.Clear();
			}
			EntryBinding entryBinding2 = FindBinding(entry);
			selection.Add(entryBinding2.Data);
			proxy.OnSelectionChanged();
			proxy.OnSelectionPicked();
		}

		private void OnEntrySelectedRadioMode(IUIBoardEntry activeEntry, bool selected)
		{
			if (selected)
			{
				DataType item = null;
				int count = bindings.Count;
				for (int i = 0; i < count; i++)
				{
					IUIBoardEntry entry = bindings[i].Entry;
					if (entry != activeEntry)
					{
						entry.SetSelected(value: false);
					}
					else
					{
						item = bindings[i].Data;
					}
				}
				selection.Clear();
				selection.Add(item);
			}
			else
			{
				selection.Remove(FindBinding(activeEntry).Data);
			}
			proxy.OnSelectionChanged();
		}

		private void OnEntrySelectedMultiSelectionMode(IUIBoardEntry activeEntry, bool selected)
		{
			EntryBinding entryBinding = FindBinding(activeEntry);
			if (selected)
			{
				selection.Add(entryBinding.Data);
			}
			else
			{
				selection.Remove(entryBinding.Data);
			}
			proxy.OnSelectionChanged();
		}
	}
}
