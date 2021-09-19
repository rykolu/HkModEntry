namespace Amplitude.UI.Boards.Filters
{
	public interface IUIBoardGlobalFilterProvider
	{
		int GlobalFiltersCount { get; }

		IUIBoardFilter GetGlobalFilter(int index);
	}
}
