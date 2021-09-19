using Amplitude.UI.Boards.Filters;
using UnityEngine;

namespace Amplitude.UI.Boards
{
	public interface IUIBoardProxy
	{
		UITransform HeaderTable { get; }

		GameObject HeaderPrefab { get; }

		UITransform EntryTable { get; }

		GameObject EntryPrefab { get; }

		UIBoard.UIBoardInteractivityType Interactivity { get; }

		bool HandleDoubleClicks { get; }

		string BackgroundStyle { get; }

		UIBoardDefinition Definition { get; }

		Orientation EntriesOrientation { get; }

		UIBoardEntriesComparer Comparer { get; }

		UIBoardFilterController Filters { get; }

		void Sort();

		void OnSelectionChanged();

		void OnSelectionPicked();
	}
}
