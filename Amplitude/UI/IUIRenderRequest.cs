namespace Amplitude.UI
{
	public interface IUIRenderRequest
	{
		void BindRenderCommands(UIView view);

		void UnbindRenderCommands();
	}
}
