namespace Amplitude.UI.Interactables
{
	public interface IUIThreeStatesToggle : IUIToggle, IUIControl, IUIInteractable
	{
		bool Unspecified { get; set; }
	}
}
