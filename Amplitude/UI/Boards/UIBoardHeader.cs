using System;
using Amplitude.Framework;

namespace Amplitude.UI.Boards
{
	public class UIBoardHeader : UIComponent
	{
		[NonSerialized]
		private UIMapper uiMapper;

		protected UIMapper Content => uiMapper;

		protected UIBoardColumnDefinition Definition { get; private set; }

		protected IUIBoardProxy Proxy { get; private set; }

		public virtual void Load(IUIBoardProxy proxy, UIBoardColumnDefinition definition)
		{
			Proxy = proxy;
			Definition = definition;
			Refresh();
		}

		public new virtual void Unload()
		{
			Proxy = null;
			Definition = null;
		}

		public virtual void Refresh()
		{
			Definition.ApplySize(Proxy, GetComponent<UITransform>());
			IDatabase<UIMapper> database = Databases.GetDatabase<UIMapper>();
			if (database != null)
			{
				uiMapper = database.GetValue(Definition.Name);
				if (uiMapper == null)
				{
					Diagnostics.LogWarning($" Could not find UIMapper {Definition.Name} for Board header");
				}
			}
		}
	}
}
