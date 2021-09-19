namespace Amplitude.UI.Boards
{
	public interface IUIBoardEntriesComparerReadOnly
	{
		int OrdersCount { get; }

		StaticString GetOrder(int index, out int order);

		int FindOrder(StaticString column);
	}
}
