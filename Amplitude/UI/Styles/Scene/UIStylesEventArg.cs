namespace Amplitude.UI.Styles.Scene
{
	internal class UIStylesEventArg
	{
		public enum Type
		{
			StyleCreated,
			StyleDeleted,
			ItemCreated,
			ItemDeleted,
			ItemChanged
		}

		public readonly Type EventType;

		public UIStylesEventArg(Type type)
		{
			EventType = type;
		}
	}
}
