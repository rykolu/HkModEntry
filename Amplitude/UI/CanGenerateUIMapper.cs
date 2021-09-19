using System;

namespace Amplitude.UI
{
	[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
	public class CanGenerateUIMapper : Attribute
	{
		public string UIMapperType { get; set; }

		public string Prefix { get; set; }

		internal static bool IsValid(Type uiMapperType)
		{
			if (uiMapperType != null && !uiMapperType.IsAbstract)
			{
				return typeof(UIMapper).IsAssignableFrom(uiMapperType);
			}
			return false;
		}
	}
}
