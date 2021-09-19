namespace Amplitude.UI
{
	public interface IUIMapperWithFacets<TKeyType>
	{
		bool TryGetFacet(TKeyType key, out IUIMapperFacet facet);
	}
}
