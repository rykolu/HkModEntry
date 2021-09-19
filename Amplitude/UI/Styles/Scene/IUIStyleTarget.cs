namespace Amplitude.UI.Styles.Scene
{
	public interface IUIStyleTarget
	{
		ref UIStyleController StyleController { get; }

		bool IsStyleValueApplied(StaticString identifier);
	}
}
