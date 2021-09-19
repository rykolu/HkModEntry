namespace Amplitude.UI.Renderers
{
	public interface IUIRenderingManager
	{
		void AddRenderRequest(IUIRenderRequest renderRequest);

		void RemoveRenderRequest(IUIRenderRequest renderRequest);

		void RefreshRenderRequest(IUIRenderRequest renderRequest);
	}
}
