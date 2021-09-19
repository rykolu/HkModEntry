namespace Amplitude.UI.Boards
{
	public interface IUIBoardEntryEnabler<DataType>
	{
		void RefreshEnabled(DataType data, IUIBoardEntry entry);
	}
}
