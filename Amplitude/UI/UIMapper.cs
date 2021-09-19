using System;
using Amplitude.Framework;
using UnityEngine;

namespace Amplitude.UI
{
	public class UIMapper : DatatableElement
	{
		[Serializable]
		public struct Image
		{
			[SerializeField]
			public UITexture Value;

			[SerializeField]
			private string serializableKey;

			public StaticString Key { get; private set; }

			public static void OnEnable(Image[] images)
			{
				if (images != null)
				{
					for (int i = 0; i < images.Length; i++)
					{
						images[i].OnEnable();
					}
				}
			}

			public static void OnValidate(Image[] images)
			{
				if (images != null)
				{
					for (int i = 0; i < images.Length; i++)
					{
						images[i].OnValidate();
					}
				}
			}

			public static UITexture GetImage(Image[] images, StaticString key)
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

			public void OnEnable()
			{
				Key = new StaticString(serializableKey);
			}

			public void OnValidate()
			{
				Key = new StaticString(serializableKey);
			}
		}

		[SerializeField]
		public string Title = string.Empty;

		[SerializeField]
		public string Description = string.Empty;

		[SerializeField]
		public Image[] Images;

		[SerializeField]
		public Color Color;

		[SerializeField]
		public string Symbol = string.Empty;

		public bool TryFindFacetTitle<T>(T facetKey, out string title)
		{
			title = Title;
			IUIMapperWithFacets<T> iUIMapperWithFacets = this as IUIMapperWithFacets<T>;
			if (iUIMapperWithFacets == null)
			{
				return false;
			}
			IUIMapperFacet facet = null;
			if (!iUIMapperWithFacets.TryGetFacet(facetKey, out facet))
			{
				return false;
			}
			IUIMapperFacetWithTitle iUIMapperFacetWithTitle = facet as IUIMapperFacetWithTitle;
			if (iUIMapperFacetWithTitle == null)
			{
				return false;
			}
			title = iUIMapperFacetWithTitle.Title;
			return true;
		}

		public UITexture GetImage(StaticString key)
		{
			UITexture result = UITexture.None;
			int num = ((Images != null) ? Images.Length : 0);
			for (int i = 0; i < num; i++)
			{
				Image image = Images[i];
				if (key == image.Key)
				{
					result = image.Value;
					break;
				}
			}
			return result;
		}

		public virtual void OnAutoGeneration(StaticString name)
		{
			Title = $"%{name.ToString()}Title";
			Description = $"%{name.ToString()}Description";
		}

		public override void Initialize()
		{
			base.Initialize();
			if (Images != null)
			{
				for (int i = 0; i < Images.Length; i++)
				{
					Images[i].OnEnable();
				}
			}
		}

		protected virtual void OnValidate()
		{
			if (Images != null)
			{
				for (int i = 0; i < Images.Length; i++)
				{
					Images[i].OnValidate();
				}
			}
		}
	}
}
