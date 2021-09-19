namespace Amplitude.UI
{
	public interface IUIMapperFacet
	{
		void OnEnable();

		void OnValidate();
	}
	public interface IUIMapperFacet<TKeyType> : IUIMapperFacet
	{
		bool KeyEquals(TKeyType key);
	}
}
