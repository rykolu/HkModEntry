using System;
using Amplitude.UI.Boards.Filters;
using Amplitude.UI.Interactables;

namespace Amplitude.UI.Boards
{
	public interface IUIBoardEntry
	{
		UITransform Transform { get; }

		UITransform CellsTable { get; }

		bool Selected { get; }

		bool Enabled { get; }

		bool IsBound { get; }

		event Action<IUIBoardEntry, bool> Switch;

		event Action<IUIBoardEntry> Select;

		void Load(IUIBoardProxy proxy);

		void Unload();

		void Bind<TData>(TData data) where TData : class;

		void Unbind();

		void Refresh();

		bool Filter(UIBoardFilterController filters);

		void SetSelected(bool value, bool silent = true);

		void SetEnabled(bool valid, UITooltipData data);

		int CompareTo(IUIBoardEntry entry, IUIBoardEntriesComparerReadOnly comparer, IUIBoardProxy proxy);

		IUIBoardCell FindCell(StaticString column);
	}
}
