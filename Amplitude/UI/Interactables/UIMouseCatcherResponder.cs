namespace Amplitude.UI.Interactables
{
	public class UIMouseCatcherResponder : UIResponder
	{
		public UIMouseCatcherResponder(UIMouseCatcher mouseCatcher)
			: base(mouseCatcher)
		{
			eventSensitivity = InputEvent.EventType.AllButKeyboard;
		}

		public override bool TryCatchEvent(ref InputEvent inputEvent)
		{
			if (inputEvent.Catched)
			{
				return false;
			}
			if ((inputEvent.Type & eventSensitivity) != 0 && Contains(inputEvent.MousePosition))
			{
				return true;
			}
			return false;
		}
	}
}
