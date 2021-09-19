namespace Amplitude.UI.Interactables
{
	public struct SortedResponder
	{
		public IUIResponder Responder;

		public long SortingIndex;

		public int LayerIndex;

		public InputEvent.EventType EventSensitivity;
	}
}
