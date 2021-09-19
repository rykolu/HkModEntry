namespace Amplitude.UI
{
	public interface IUIResolutionDependent
	{
		void OnResolutionChanged(float oldWidth, float oldHeight, float newWidth, float newHeight);
	}
}
