using System;
using Amplitude.Framework;
using Amplitude.Graphics;
using Amplitude.Graphics.Fx;
using UnityEngine;

namespace Amplitude.UI.Atlas
{
	public class UITextureAtlas : InspectableScriptableObject
	{
		[Serializable]
		public struct AtlasEntry
		{
			[FakeAssetReference(typeof(Texture2D))]
			public Amplitude.Framework.Guid Guid;

			public int Index;

			public Rect Coordinates;

			public Vector2Int WidthHeight;

			public AtlasEntry(Amplitude.Framework.Guid guid, Rect coordinates, Vector2Int widthHeight, int index)
			{
				Guid = guid;
				Index = index;
				Coordinates = coordinates;
				WidthHeight = widthHeight;
			}
		}

		[Serializable]
		public struct OutputEntry
		{
			[Flags]
			public enum OptionEnum
			{
				None = 0x0,
				Unloadable = 0x1,
				UseProxy = 0x2
			}

			[SerializeField]
			public string Name;

			[SerializeField]
			public uint Options;

			[SerializeField]
			[FakeAssetReference(typeof(Texture2D))]
			private Amplitude.Framework.Guid texture;

			[SerializeField]
			[FakeAssetReference(typeof(Texture2D))]
			private Amplitude.Framework.Guid proxy;

			public Amplitude.Framework.Guid Texture
			{
				get
				{
					return texture;
				}
				set
				{
					texture = value;
				}
			}

			public Amplitude.Framework.Guid Proxy
			{
				get
				{
					return proxy;
				}
				set
				{
					proxy = value;
				}
			}
		}

		[SerializeField]
		public string Owner;

		[SerializeField]
		protected AtlasEntry[] atlasEntries;

		[SerializeField]
		protected OutputEntry[] outputEntries;

		public OutputEntry[] OutputEntries => outputEntries;

		public AtlasEntry[] AtlasEntries => atlasEntries;
	}
}
