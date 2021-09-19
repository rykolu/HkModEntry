using System;
using System.Diagnostics;
using Amplitude.Framework;
using Amplitude.Framework.Asset;
using Amplitude.UI.Renderers;
using UnityEngine;

namespace Amplitude.UI
{
	[Serializable]
	public struct UITexture : IEquatable<UITexture>, ISerializationCallbackReceiver
	{
		public static readonly Rect FullRect = new Rect(0f, 0f, 1f, 1f);

		public static UITexture White = new UITexture(UIRenderingManager.WhiteTextureGuid, UITextureFlags.AlphaStraight, UITextureColorFormat.Srgb, null);

		public static UITexture None = new UITexture(Amplitude.Framework.Guid.Null, UITextureFlags.AlphaStraight, UITextureColorFormat.Srgb, null);

		[NonSerialized]
		public Texture Texture;

		[NonSerialized]
		private readonly UITextureFlags flags;

		[NonSerialized]
		private readonly UITextureColorFormat colorFormat;

		[SerializeField]
		private Amplitude.Framework.Guid guid;

		[NonSerialized]
		private int sourceIndex;

		[NonSerialized]
		private bool loaded;

		[NonSerialized]
		private Vector2Int assetWidthHeight;

		[NonSerialized]
		private Rect assetCoordinates;

		[NonSerialized]
		private Rect subCoordinates;

		[NonSerialized]
		private bool useCustomSubCoordinate;

		public bool Loaded => loaded;

		public Rect Coordinates
		{
			get
			{
				if (!useCustomSubCoordinate)
				{
					return assetCoordinates;
				}
				return new Rect(assetCoordinates.xMin + subCoordinates.xMin * assetCoordinates.width, assetCoordinates.yMin + subCoordinates.yMin * assetCoordinates.height, assetCoordinates.width * subCoordinates.width, assetCoordinates.height * subCoordinates.height);
			}
		}

		public Rect SubCoordinates
		{
			get
			{
				if (!useCustomSubCoordinate)
				{
					return FullRect;
				}
				return subCoordinates;
			}
			set
			{
				subCoordinates = value;
				useCustomSubCoordinate = subCoordinates != FullRect;
			}
		}

		public Vector2 WidthHeight => assetWidthHeight;

		public float Aspect
		{
			get
			{
				Vector2 widthHeight = WidthHeight;
				return widthHeight.x / widthHeight.y;
			}
		}

		public UITextureFlags Flags => flags;

		public UITextureColorFormat ColorFormat => colorFormat;

		public bool IsDefined => guid != Amplitude.Framework.Guid.Null;

		public string AssetPath => AssetDatabase.GetAssetPathFromGuid(guid);

		public UnityEngine.Object OwnerObjectForDebug => null;

		public UITexture(Amplitude.Framework.Guid guid, UITextureFlags flags, UITextureColorFormat colorFormat, Texture texture)
		{
			Texture = texture;
			sourceIndex = 0;
			loaded = texture != null;
			assetWidthHeight = ((texture != null) ? new Vector2Int(texture.width, texture.height) : Vector2Int.zero);
			assetCoordinates = FullRect;
			subCoordinates = FullRect;
			useCustomSubCoordinate = false;
			this.guid = guid;
			this.flags = flags;
			this.colorFormat = colorFormat;
		}

		public static bool operator ==(UITexture left, UITexture right)
		{
			return left.guid == right.guid;
		}

		public static bool operator !=(UITexture left, UITexture right)
		{
			return left.guid != right.guid;
		}

		public bool Equals(UITexture other)
		{
			return guid == other.guid;
		}

		public override int GetHashCode()
		{
			return guid.GetHashCode();
		}

		public override bool Equals(object other)
		{
			if (other.GetType() == typeof(UITexture))
			{
				UITexture uITexture = (UITexture)other;
				return guid == uITexture.guid;
			}
			return false;
		}

		public void Unload()
		{
			sourceIndex = -1;
			Texture = null;
			loaded = false;
		}

		public Texture GetAsset()
		{
			if ((bool)Texture)
			{
				return Texture;
			}
			UIRenderingManager instance = UIRenderingManager.Instance;
			Texture result = null;
			instance.RetrieveTextureSource(sourceIndex, out result, out var canKeepReferenceOnTexture);
			if (canKeepReferenceOnTexture)
			{
				Texture = result;
			}
			return result;
		}

		[Conditional("ASSERT")]
		public void SetOwnerObjectForDebug(UnityEngine.Object ownerObjectForDebug)
		{
		}

		public void RequestSize()
		{
			RequestAsset();
		}

		public void RequestAsset()
		{
			if (!loaded)
			{
				UIRenderingManager instance = UIRenderingManager.Instance;
				UnityEngine.Object ownerForDebug = null;
				instance.GetTextureInfo(guid, ownerForDebug, out sourceIndex, out assetWidthHeight, out assetCoordinates);
				subCoordinates = (useCustomSubCoordinate ? subCoordinates : FullRect);
				loaded = true;
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			loaded = false;
		}
	}
}
