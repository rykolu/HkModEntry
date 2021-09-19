using System;
using System.Collections.Generic;
using Amplitude.UI.Boards.Filters;
using Amplitude.UI.Layouts;
using UnityEngine;

namespace Amplitude.UI.Boards
{
	public class UIBoard : UIComponent, IUIBoardProxy
	{
		[Serializable]
		public enum UIBoardInteractivityType
		{
			None,
			RadioSelection,
			MultiSelection
		}

		[SerializeField]
		private UITransform headerTable;

		[SerializeField]
		private GameObject headerPrefab;

		[SerializeField]
		private UITransform entryTable;

		[NonSerialized]
		private UITable1D entryLayout;

		[SerializeField]
		private GameObject entryPrefab;

		[SerializeField]
		private UIBoardInteractivityType interactivity;

		[SerializeField]
		private bool handleDoubleClicks = true;

		[SerializeField]
		private string backgroundStyle = string.Empty;

		[SerializeField]
		private UIBoardDefinition definition;

		[NonSerialized]
		private UIBoardEntriesComparer comparer;

		[NonSerialized]
		private UIBoardFilterController filters;

		[NonSerialized]
		private IUIBoardImpl boardImplementation;

		private Orientation entriesOrientation = Orientation.Vertical;

		UITransform IUIBoardProxy.HeaderTable => headerTable;

		GameObject IUIBoardProxy.HeaderPrefab => headerPrefab;

		UITransform IUIBoardProxy.EntryTable => entryTable;

		GameObject IUIBoardProxy.EntryPrefab => entryPrefab;

		UIBoardInteractivityType IUIBoardProxy.Interactivity => interactivity;

		bool IUIBoardProxy.HandleDoubleClicks => handleDoubleClicks;

		string IUIBoardProxy.BackgroundStyle => backgroundStyle;

		public UIBoardDefinition Definition => definition;

		Orientation IUIBoardProxy.EntriesOrientation => entriesOrientation;

		public UIBoardEntriesComparer Comparer => comparer;

		public UIBoardFilterController Filters => filters;

		public event Action SelectionChanged;

		public event Action SelectionPicked;

		public UIBoard()
		{
			comparer = new UIBoardEntriesComparer(this);
		}

		public void Load<DataType>(IUIBoardEntryEnabler<DataType> validator = null) where DataType : class
		{
			entryLayout = entryTable.GetComponent<UITable1D>();
			CheckOrientations();
			if (definition != null && definition != null && entryPrefab != null)
			{
				filters = new UIBoardFilterController(definition);
				boardImplementation = new UIBoardImpl<DataType>(this, validator);
			}
			else
			{
				Diagnostics.LogWarning("Something went wrong in initializing UIBoard(" + definition.ToString() + ")");
				Unload();
			}
		}

		public new void Unload()
		{
			comparer.Clear();
			if (entryLayout != null)
			{
				entryLayout.SetComparer(null);
				entryLayout = null;
			}
			if (boardImplementation != null)
			{
				boardImplementation.Dispose();
				boardImplementation = null;
			}
		}

		public void Refresh()
		{
			boardImplementation.Refresh();
			boardImplementation.Sort();
		}

		public void Add<DataType>(DataType newData, bool rebindNow = true) where DataType : class
		{
			(boardImplementation as UIBoardImpl<DataType>)?.Add(newData, rebindNow);
		}

		public void AddRange<DataType>(IEnumerable<DataType> newData, bool rebindNow = true) where DataType : class
		{
			(boardImplementation as UIBoardImpl<DataType>)?.AddRange(newData, rebindNow);
		}

		public void Remove<DataType>(DataType data) where DataType : class
		{
			(boardImplementation as UIBoardImpl<DataType>)?.Remove(data);
		}

		public void Clear()
		{
			boardImplementation.Clear();
		}

		public DataType[] GetCurrentSelection<DataType>() where DataType : class
		{
			return (boardImplementation as UIBoardImpl<DataType>)?.GetCurrentSelection();
		}

		public void Select<DataType>(DataType data, bool select) where DataType : class
		{
			(boardImplementation as UIBoardImpl<DataType>).Select(data, select);
		}

		void IUIBoardProxy.Sort()
		{
			if (comparer.OrdersCount > 0)
			{
				boardImplementation.Sort();
				entryLayout.ArrangeChildren();
			}
		}

		void IUIBoardProxy.OnSelectionChanged()
		{
			this.SelectionChanged?.Invoke();
		}

		void IUIBoardProxy.OnSelectionPicked()
		{
			this.SelectionPicked?.Invoke();
		}

		private void OnFilterChanged(IUIBoardFilter filter)
		{
		}

		private void CheckOrientations()
		{
			entriesOrientation = entryLayout.Orientation;
		}
	}
}
