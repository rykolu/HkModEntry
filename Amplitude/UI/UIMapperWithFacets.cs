using System;
using UnityEngine.Serialization;

namespace Amplitude.UI
{
	public abstract class UIMapperWithFacets : UIMapper
	{
		[Obsolete]
		protected static UITexture GetImage(Image[] images, StaticString key)
		{
			UITexture result = UITexture.None;
			int num = ((images != null) ? images.Length : 0);
			for (int i = 0; i < num; i++)
			{
				Image image = images[i];
				if (key == image.Key)
				{
					result = image.Value;
					break;
				}
			}
			return result;
		}

		[Obsolete]
		protected static void OnEnable_Images(Image[] images)
		{
			if (images != null)
			{
				for (int i = 0; i < images.Length; i++)
				{
					images[i].OnEnable();
				}
			}
		}

		[Obsolete]
		protected static void OnValidate_Images(Image[] images)
		{
			if (images != null)
			{
				for (int i = 0; i < images.Length; i++)
				{
					images[i].OnValidate();
				}
			}
		}
	}
	public abstract class UIMapperWithFacets<TFacetType, TKeyType> : UIMapperWithFacets, IUIMapperWithFacets<TKeyType> where TFacetType : class, IUIMapperFacet<TKeyType>
	{
		[FormerlySerializedAs("Facettes")]
		public TFacetType[] Facets;

		public bool TryGetFacet(TKeyType key, out IUIMapperFacet facet)
		{
			TFacetType facet2 = null;
			if (TryGetFacet(key, out facet2))
			{
				facet = facet2;
				return true;
			}
			facet = null;
			return false;
		}

		public virtual bool TryGetFacet(TKeyType key, out TFacetType facet)
		{
			facet = null;
			if (Facets != null)
			{
				for (int i = 0; i < Facets.Length; i++)
				{
					if (Facets[i].KeyEquals(key))
					{
						facet = Facets[i];
						return true;
					}
				}
			}
			return false;
		}

		public TFacetType GetFacet(TKeyType key)
		{
			TFacetType facet = null;
			if (!TryGetFacet(key, out facet))
			{
				Diagnostics.LogWarning("Facet '{0}' could not be found on UIMapper '{1}'", key, base.Name);
			}
			return facet;
		}

		public override void Initialize()
		{
			base.Initialize();
			if (Facets != null)
			{
				for (int i = 0; i < Facets.Length; i++)
				{
					Facets[i].OnEnable();
				}
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (Facets != null)
			{
				for (int i = 0; i < Facets.Length; i++)
				{
					Facets[i].OnValidate();
				}
			}
		}
	}
}
