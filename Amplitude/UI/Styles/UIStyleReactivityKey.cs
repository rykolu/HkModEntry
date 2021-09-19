using System;

namespace Amplitude.UI.Styles
{
	public struct UIStyleReactivityKey
	{
		public static string Separator = ";";

		public static char[] SeparatorChar = new char[1] { ';' };

		private readonly StaticString[] tags;

		public int Length
		{
			get
			{
				if (tags == null)
				{
					return 0;
				}
				return tags.Length;
			}
		}

		public StaticString this[int index] => tags[index];

		public UIStyleReactivityKey(string tagsAsString)
		{
			string[] array = tagsAsString.Split(SeparatorChar, StringSplitOptions.RemoveEmptyEntries);
			int num = array.Length;
			tags = new StaticString[num];
			for (int i = 0; i < num; i++)
			{
				tags[i] = new StaticString(array[i]);
			}
		}

		public bool Contains(StaticString tag)
		{
			if (tags != null)
			{
				int num = tags.Length;
				for (int i = 0; i < num; i++)
				{
					if (tag == tags[i])
					{
						return true;
					}
				}
			}
			return false;
		}

		public override string ToString()
		{
			if (tags == null)
			{
				return string.Empty;
			}
			return StaticString.Join(Separator, tags);
		}
	}
}
