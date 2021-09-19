using UnityEngine;

namespace Amplitude.UI.Styles.Data
{
	[CreateAssetMenu(fileName = "NewStylesheets.asset", menuName = "Amplitude/UI/Stylesheet")]
	public class UIStylesheet : ScriptableObject
	{
		[SerializeField]
		internal UIStyle[] Styles;

		public bool Contains(StaticString name)
		{
			int num = ((Styles != null) ? Styles.Length : 0);
			for (int i = 0; i < num; i++)
			{
				UIStyle uIStyle = Styles[i];
				if (uIStyle != null && uIStyle.Name == name)
				{
					return true;
				}
			}
			return false;
		}

		internal void Load()
		{
			int num = ((Styles != null) ? Styles.Length : 0);
			for (int i = 0; i < num; i++)
			{
				UIStyle uIStyle = Styles[i];
				if (uIStyle != null)
				{
					uIStyle.Load();
				}
			}
		}
	}
}
