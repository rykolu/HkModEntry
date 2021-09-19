using System;

namespace Amplitude.UI
{
	public static class UISettings
	{
		public struct Entry<T> where T : IEquatable<T>
		{
			internal delegate T GetValue(string key, T defaultValue);

			internal delegate void SetValue(string key, T value);

			private readonly string name;

			private readonly string key;

			private T cachedValue;

			private GetValue getValue;

			private SetValue setValue;

			public string Name => name;

			public T Value
			{
				get
				{
					return cachedValue;
				}
				set
				{
					if (!cachedValue.Equals(value))
					{
						cachedValue = value;
						setValue(key, value);
					}
				}
			}

			internal Entry(string name, GetValue getValue, SetValue setValue, T initialValue)
			{
				this.name = name;
				key = "Amplitude.UI.UISettings.v00." + name;
				this.getValue = getValue;
				this.setValue = setValue;
				cachedValue = this.getValue(key, initialValue);
			}
		}

		public static Entry<bool> EnableRender = new Entry<bool>("EnableRender", GetBool, SetBool, initialValue: true);

		public static Entry<bool> EnablePerformanceAlertFeedback = new Entry<bool>("PerformanceAlertFeedback", GetBool, SetBool, initialValue: false);

		public static Entry<float> BudgetPerFrameWarning = new Entry<float>("BudgetPerFrameWarning", GetFloat, SetFloat, 0f);

		public static Entry<float> BudgetPerFrameCritical = new Entry<float>("BudgetPerFrameCritical", GetFloat, SetFloat, 0f);

		public static Entry<float> BudgetPerInstantiationWarning = new Entry<float>("BudgetPerInstantiationWarning", GetFloat, SetFloat, 0f);

		public static Entry<bool> EnableVerboseHierarchy = new Entry<bool>("VerboseHierarchy", GetBool, SetBool, initialValue: false);

		public static Entry<bool> EnableVerboseRendering = new Entry<bool>("VerboseRendering", GetBool, SetBool, initialValue: false);

		public static Entry<bool> EnableVerboseInteractivity = new Entry<bool>("VerboseInteractivity", GetBool, SetBool, initialValue: false);

		public static Entry<bool> EnableVerboseAnimations = new Entry<bool>("VerboseAnimations", GetBool, SetBool, initialValue: false);

		public static Entry<bool> EnableVerboseStyles = new Entry<bool>("VerboseStyles", GetBool, SetBool, initialValue: false);

		public static Entry<bool> EnableVerboseWindows = new Entry<bool>("VerboseWindows", GetBool, SetBool, initialValue: false);

		public static Entry<bool> EnableVerboseTooltips = new Entry<bool>("VerboseTooltips", GetBool, SetBool, initialValue: false);

		public static Entry<bool> EnableVerboseStamps = new Entry<bool>("VerboseStamps", GetBool, SetBool, initialValue: false);

		private const string Version = "v00";

		private const string KeyBase = "Amplitude.UI.UISettings.v00.";

		private static int GetInt(string key, int defaultValue)
		{
			return defaultValue;
		}

		private static void SetInt(string key, int value)
		{
		}

		private static float GetFloat(string key, float defaultValue)
		{
			return defaultValue;
		}

		private static void SetFloat(string key, float value)
		{
		}

		private static bool GetBool(string key, bool defaultValue)
		{
			return defaultValue;
		}

		private static void SetBool(string key, bool value)
		{
		}
	}
}
