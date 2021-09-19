using UnityEngine;

namespace Amplitude.UI
{
	public abstract class UIMapperFacet<TKeyType> : IUIMapperFacet<TKeyType>, IUIMapperFacet, IUIMapperFacetWithTitle
	{
		[SerializeField]
		public string Title = string.Empty;

		[SerializeField]
		public string Description = string.Empty;

		[SerializeField]
		public UIMapper.Image[] Images;

		[SerializeField]
		public Color Color;

		[SerializeField]
		public string Symbol = string.Empty;

		string IUIMapperFacetWithTitle.Title => Title;

		public abstract bool KeyEquals(TKeyType key);

		public UITexture GetImage(StaticString key)
		{
			return UIMapper.Image.GetImage(Images, key);
		}

		public void OnEnable()
		{
			UIMapper.Image.OnEnable(Images);
		}

		public void OnValidate()
		{
			UIMapper.Image.OnValidate(Images);
		}
	}
}
