using Amplitude.UI.Styles;

namespace Amplitude.UI.Interactables
{
	public struct UIReactivityState
	{
		public static class Key
		{
			public static readonly StaticString Hover = new StaticString("hover");

			public static readonly StaticString Pressed = new StaticString("pressed");

			public static readonly StaticString On = new StaticString("on");

			public static readonly StaticString Unspecified = new StaticString("unspecified");

			public static readonly StaticString Disabled = new StaticString("disabled");

			public static readonly StaticString Focused = new StaticString("focused");
		}

		public static readonly UIReactivityState Normal = default(UIReactivityState);

		private PerformanceList<StaticString> tags;

		public bool IsNormal => tags.Count == 0;

		public int TagsCount => tags.Count;

		public bool IsSameAs(ref UIReactivityState other)
		{
			if (other.IsNormal)
			{
				return IsNormal;
			}
			int count = tags.Count;
			int count2 = other.tags.Count;
			if (count != count2)
			{
				return false;
			}
			for (int i = 0; i < count; i++)
			{
				if (!other.ContainsTag(tags.Data[i]))
				{
					return false;
				}
			}
			for (int j = 0; j < count2; j++)
			{
				if (!ContainsTag(other.tags.Data[j]))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsSameAs(StaticString[] tags)
		{
			int count = this.tags.Count;
			int num = ((tags != null) ? tags.Length : 0);
			if (count != num)
			{
				return false;
			}
			for (int i = 0; i < num; i++)
			{
				if (!ContainsTag(tags[i]))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsSameAs(StaticString tag)
		{
			if (tags.Count == 1)
			{
				return tags.Data[0] != tag;
			}
			return false;
		}

		public bool TryKey(ref UIStyleReactivityKey key, out int score)
		{
			score = 0;
			int length = key.Length;
			for (int i = 0; i < length; i++)
			{
				if (!ContainsTag(key[i]))
				{
					return false;
				}
			}
			int count = tags.Count;
			for (int j = 0; j < count; j++)
			{
				for (int k = 0; k < length; k++)
				{
					if (key[k] == tags.Data[j])
					{
						score++;
					}
				}
			}
			return true;
		}

		public void Add(StaticString tag)
		{
			if (!StaticString.IsNullOrEmpty(tag) && !ContainsTag(tag))
			{
				tags.Add(tag);
			}
		}

		public void Add(StaticString[] tags)
		{
			int num = ((tags != null) ? tags.Length : 0);
			for (int i = 0; i < num; i++)
			{
				StaticString staticString = tags[i];
				if (!StaticString.IsNullOrEmpty(staticString) && !ContainsTag(staticString))
				{
					this.tags.Add(staticString);
				}
			}
		}

		public void AddTagsFrom(ref UIReactivityState other)
		{
			int count = other.tags.Count;
			for (int i = 0; i < count; i++)
			{
				StaticString staticString = other.tags.Data[i];
				if (!StaticString.IsNullOrEmpty(staticString) && !ContainsTag(staticString))
				{
					tags.Add(staticString);
				}
			}
		}

		public bool ContainsTag(StaticString key)
		{
			int count = tags.Count;
			for (int i = 0; i < count; i++)
			{
				if (tags.Data[i] == key)
				{
					return true;
				}
			}
			return false;
		}

		public void Clear()
		{
			tags.Clear();
		}

		public override string ToString()
		{
			string text = string.Empty;
			int count = tags.Count;
			for (int i = 0; i < count; i++)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += ";";
				}
				text += tags.Data[i].ToString();
			}
			if (string.IsNullOrEmpty(text))
			{
				return "normal";
			}
			return text;
		}
	}
}
