using System;

namespace Amplitude.UI
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public class ExcludeFromUIMapperGeneration : Attribute
	{
	}
}
