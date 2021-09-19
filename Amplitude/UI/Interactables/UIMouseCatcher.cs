namespace Amplitude.UI.Interactables
{
	public class UIMouseCatcher : UIInteractable
	{
		private UIMouseCatcherResponder MouseCatcherResponder => (UIMouseCatcherResponder)base.Responder;

		protected override IUIResponder InstantiateResponder()
		{
			return new UIMouseCatcherResponder(this);
		}
	}
}
