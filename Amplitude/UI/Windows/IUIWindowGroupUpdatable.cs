namespace Amplitude.UI.Windows
{
	public interface IUIWindowGroupUpdatable<SharedDataType>
	{
		bool IsReady { get; }

		void SpecificUpdate();

		void Refresh(SharedDataType data);
	}
}
