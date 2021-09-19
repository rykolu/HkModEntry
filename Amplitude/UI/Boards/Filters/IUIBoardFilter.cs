using System;

namespace Amplitude.UI.Boards.Filters
{
	public interface IUIBoardFilter
	{
		event Action<IUIBoardFilter> FilterChanged;

		void Reset();
	}
}
