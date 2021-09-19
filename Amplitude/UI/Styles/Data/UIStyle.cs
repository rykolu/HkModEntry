using UnityEngine;

namespace Amplitude.UI.Styles.Data
{
	public class UIStyle : ScriptableObject
	{
		private StaticString cachedName = StaticString.Empty;

		[SerializeField]
		[HideInInspector]
		private UIStyleItem[] items;

		public StaticString Name
		{
			get
			{
				return cachedName;
			}
			internal set
			{
				cachedName = value;
				base.name = value.ToString();
			}
		}

		internal int ItemsLength
		{
			get
			{
				if (items == null)
				{
					return 0;
				}
				return items.Length;
			}
		}

		internal void Load()
		{
			if (StaticString.IsNullOrEmpty(cachedName))
			{
				cachedName = new StaticString(base.name);
			}
			int num = ((items != null) ? items.Length : 0);
			for (int i = 0; i < num; i++)
			{
				UIStyleItem uIStyleItem = items[i];
				if (uIStyleItem != null)
				{
					uIStyleItem.Load();
				}
			}
		}

		internal UIStyleItem GetItem(int index)
		{
			if (items != null && index >= 0 && index < items.Length)
			{
				return items[index];
			}
			return null;
		}

		internal bool HasItemIdentifier(StaticString identifier)
		{
			if (items != null)
			{
				for (int i = 0; i < items.Length; i++)
				{
					if (items[i].GetIdentifier() == identifier)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void OnEnable()
		{
			Load();
		}
	}
}
