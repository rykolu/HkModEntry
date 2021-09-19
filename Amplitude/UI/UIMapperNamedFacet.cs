using UnityEngine.Serialization;

namespace Amplitude.UI
{
	public abstract class UIMapperNamedFacet : IUIMapperFacet<string>, IUIMapperFacet
	{
		[FormerlySerializedAs("name")]
		public string Name;

		public bool KeyEquals(string key)
		{
			return Name == key;
		}

		public abstract void OnEnable();

		public abstract void OnValidate();
	}
}
