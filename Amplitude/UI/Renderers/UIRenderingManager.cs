using System;
using System.Collections.Generic;
using System.Diagnostics;
using Amplitude.Framework;
using Amplitude.Framework.Asset;
using Amplitude.Graphics;
using Amplitude.Graphics.Fx;
using Amplitude.Graphics.Text;
using Amplitude.UI.Atlas;
using Amplitude.UI.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Amplitude.UI.Renderers
{
	[RequireComponent(typeof(UIHierarchyManager))]
	[ExecuteInEditMode]
	[RequireComponent(typeof(UIHierarchyManager))]
	public class UIRenderingManager : UIBehaviour, IUIRenderingManager
	{
		private static class ShaderStrings
		{
			public static ShaderString RectBuffer = "_RectBuffer";

			public static ShaderString SquircleBuffer = "_SquircleBuffer";

			public static ShaderString CircleBuffer = "_CircleBuffer";

			public static ShaderString GlyphBuffer = "_GlyphBuffer";

			public static ShaderString SectorBuffer = "_SectorBuffer";

			public static ShaderString CurveBuffer = "_CurveBuffer";

			public static ShaderString CurveSegmentBuffer = "_CurveSegmentBuffer";

			public static ShaderString TransformBuffer = "_TransformBuffer";

			public static ShaderString GenericBuffer = "_GenericBuffer";

			public static void Load()
			{
				RectBuffer.Load();
				SquircleBuffer.Load();
				CircleBuffer.Load();
				GlyphBuffer.Load();
				SectorBuffer.Load();
				CurveBuffer.Load();
				CurveSegmentBuffer.Load();
				TransformBuffer.Load();
				GenericBuffer.Load();
			}
		}

		private struct TextureSource
		{
			public readonly Amplitude.Framework.Guid Guid;

			public readonly UITextureAtlas Atlas;

			public readonly int AtlasOutputIndex;

			public readonly bool Persistant;

			public Texture Texture;

			public int LastUsedDate;

			private bool loadingProxy;

			private AssetBundleRequest assetBundleRequest;

			private AsyncTextureContentGenerator textureGenerator;

			private Texture2D proxyTexture;

			private bool sharedTexture;

			public TextureSource(Amplitude.Framework.Guid guid, Texture texture, bool persistant)
			{
				Texture = texture;
				proxyTexture = null;
				Guid = guid;
				Atlas = null;
				AtlasOutputIndex = -1;
				LastUsedDate = -1;
				Persistant = persistant;
				assetBundleRequest = null;
				textureGenerator = null;
				loadingProxy = false;
				sharedTexture = false;
			}

			public TextureSource(Amplitude.Framework.Guid guid, AsyncTextureContentGenerator textureGenerator)
			{
				Texture = null;
				proxyTexture = null;
				Guid = guid;
				Atlas = null;
				AtlasOutputIndex = -1;
				LastUsedDate = -1;
				Persistant = false;
				assetBundleRequest = null;
				this.textureGenerator = textureGenerator;
				loadingProxy = false;
				sharedTexture = false;
			}

			public TextureSource(UITextureAtlas atlas, int atlasOutputIndex)
			{
				Texture = null;
				proxyTexture = null;
				Guid = atlas.OutputEntries[atlasOutputIndex].Texture;
				Atlas = atlas;
				AtlasOutputIndex = atlasOutputIndex;
				LastUsedDate = -1;
				Persistant = false;
				assetBundleRequest = null;
				textureGenerator = null;
				loadingProxy = false;
				sharedTexture = false;
				if (!atlas.OutputEntries[atlasOutputIndex].Proxy.IsNull)
				{
					assetBundleRequest = AssetDatabase.LoadAssetAsync<Texture2D>(Atlas.OutputEntries[AtlasOutputIndex].Proxy, 64u, out proxyTexture);
					if (assetBundleRequest != null)
					{
						loadingProxy = true;
					}
				}
			}

			public Texture Load(Texture2D fallbackTexture)
			{
				if ((bool)Texture)
				{
					return Texture;
				}
				uint num = 64u;
				if (Atlas != null)
				{
					if (loadingProxy)
					{
						proxyTexture = assetBundleRequest.asset as Texture2D;
						loadingProxy = false;
						assetBundleRequest = null;
					}
					if (assetBundleRequest == null)
					{
						Texture2D assetIfAlreadyLoaded = null;
						assetBundleRequest = AssetDatabase.LoadAssetAsync<Texture2D>(Atlas.OutputEntries[AtlasOutputIndex].Texture, num, out assetIfAlreadyLoaded);
						Texture = assetIfAlreadyLoaded;
						if (assetBundleRequest == null && Texture == null)
						{
							Texture = fallbackTexture;
							sharedTexture = true;
						}
					}
					else if (assetBundleRequest.isDone)
					{
						Texture = assetBundleRequest.asset as Texture;
						assetBundleRequest = null;
						if (Texture == null)
						{
							Texture = fallbackTexture;
							sharedTexture = true;
						}
					}
					if ((bool)Texture)
					{
						return Texture;
					}
					if (proxyTexture != null)
					{
						return proxyTexture;
					}
					return fallbackTexture;
				}
				if (textureGenerator != null)
				{
					bool done = false;
					Texture texture = textureGenerator.GetTexture(fallbackTexture, out done);
					if (done)
					{
						Texture = texture;
					}
					return texture;
				}
				Texture = AssetDatabase.TryLoadAsset(Guid, num, fallbackTexture);
				sharedTexture = Texture == fallbackTexture;
				return Texture;
			}

			public void UnloadIfTooOld(int currentDate, int frameCountDelayBeforeUnloadingTexture = 360000)
			{
				if (!Persistant && currentDate - LastUsedDate >= frameCountDelayBeforeUnloadingTexture && Texture != null && Texture != Texture2D.whiteTexture)
				{
					if (!sharedTexture)
					{
						Resources.UnloadAsset(Texture);
						sharedTexture = false;
					}
					Texture = null;
					if (textureGenerator != null)
					{
						textureGenerator.TextureUnloaded();
					}
				}
			}
		}

		private struct TextureInfo
		{
			public readonly Amplitude.Framework.Guid AssetGuid;

			public readonly Rect Coordinates;

			public readonly Vector2Int TextureWidthHeight;

			public readonly int TextureSourceIndex;

			public TextureInfo(int textureSourceIndex, Amplitude.Framework.Guid assetGuid, int textureWidth, int textureHeight, Rect coordinates)
			{
				AssetGuid = assetGuid;
				Coordinates = coordinates;
				TextureWidthHeight = new Vector2Int(textureWidth, textureHeight);
				TextureSourceIndex = textureSourceIndex;
			}
		}

		private struct TextureRegistration
		{
			public readonly Texture Texture;

			public readonly Amplitude.Framework.Guid Guid;

			public TextureRegistration(Texture texture, Amplitude.Framework.Guid guid)
			{
				Texture = texture;
				Guid = guid;
			}
		}

		private struct TextureSourceRepository
		{
			public struct Index
			{
				public int ArrayIndex;

				public int HandleIndex;
			}

			[NonSerialized]
			public TextureSource[] Data;

			[NonSerialized]
			public int Count;

			[NonSerialized]
			public Index[] Indexes;

			public int Instanciate()
			{
				CheckConsistency();
				int num = ((Data != null) ? Data.Length : 0);
				if (Count + 1 > num)
				{
					int num2 = System.Math.Max(num * 2, Count + 1);
					Array.Resize(ref Data, num2);
					Array.Resize(ref Indexes, num2);
					for (int i = num; i < num2; i++)
					{
						Indexes[i].ArrayIndex = -1;
						Indexes[i].HandleIndex = i;
					}
				}
				CheckConsistency();
				int count = Count;
				int handleIndex = Indexes[count].HandleIndex;
				Indexes[handleIndex].ArrayIndex = count;
				CheckConsistency();
				Data[Count] = default(TextureSource);
				Count++;
				return handleIndex;
			}

			public void Destroy(int handleIndex)
			{
				CheckConsistency();
				int arrayIndex = Indexes[handleIndex].ArrayIndex;
				if (arrayIndex < Count - 1)
				{
					int handleIndex2 = Indexes[Count - 1].HandleIndex;
					Data[arrayIndex] = Data[Count - 1];
					Indexes[handleIndex2].ArrayIndex = arrayIndex;
					Indexes[arrayIndex].HandleIndex = handleIndex2;
					Indexes[Count - 1].HandleIndex = handleIndex;
				}
				Indexes[handleIndex].ArrayIndex = -1;
				Data[Count - 1] = default(TextureSource);
				Count--;
				CheckConsistency();
			}

			public void RetrieveTextureSource(int sourceIndex, out Texture result, out bool canKeepReferenceOnTexture, Texture2D fallbackTexture)
			{
				int arrayIndex = Indexes[sourceIndex].ArrayIndex;
				Data[arrayIndex].LastUsedDate = Time.frameCount;
				canKeepReferenceOnTexture = Data[arrayIndex].Persistant && Data[arrayIndex].Atlas != null;
				result = Data[arrayIndex].Texture;
				if (result == null)
				{
					result = Data[arrayIndex].Load(fallbackTexture);
				}
			}

			public void SetData(int handleIndex, ref TextureSource source)
			{
				int arrayIndex = Indexes[handleIndex].ArrayIndex;
				Data[arrayIndex] = source;
			}

			public Texture GetTexture(int handleIndex, Texture2D fallbackTexture)
			{
				int arrayIndex = Indexes[handleIndex].ArrayIndex;
				ref TextureSource reference = ref Data[arrayIndex];
				Texture texture = reference.Texture;
				if (texture == null)
				{
					return reference.Load(fallbackTexture);
				}
				return texture;
			}

			public int GetTextureSourceIndex(ref Amplitude.Framework.Guid guid)
			{
				int count = Count;
				for (int i = 0; i < count; i++)
				{
					if (Data[i].Guid == guid)
					{
						return i;
					}
				}
				return -1;
			}

			public void Clear()
			{
				Data = null;
				Count = 0;
				Indexes = null;
			}

			public void UnloadAllUnusedTextures(int frameCountDelayBeforeUnloadingTexture = 360000)
			{
				_ = Time.frameCount;
				int count = Count;
				for (int i = 0; i < count; i++)
				{
					Data[i].UnloadIfTooOld(frameCountDelayBeforeUnloadingTexture);
				}
			}

			private void CheckConsistency()
			{
				for (int i = 0; i < Count; i++)
				{
					int num = i;
					_ = ref Indexes[num];
				}
			}
		}

		private struct CacheEntry
		{
			public Amplitude.Framework.Guid Guid;

			public int HandleIndex;
		}

		public static bool SoftDebugMissingTexture = false;

		[NonSerialized]
		public readonly SymbolMapperController SymbolMapperController = new SymbolMapperController();

		private static UIRenderingManager instance = null;

		[SerializeField]
		[FakeAssetReference(typeof(UIMaterialCollection))]
		private Amplitude.Framework.Guid materialCollectionRef = new Amplitude.Framework.Guid("abcb656180fb4a4419171bb74a11fcc0");

		[SerializeField]
		private FontAtlasRenderer fontAtlas;

		[SerializeField]
		private FontFamily defaultFontFamily;

		[SerializeField]
		private FontFamily fallbackFontFamily;

		[SerializeField]
		private FontRenderingMode fontRenderingMode;

		[SerializeField]
		private string unknownChars;

		[SerializeField]
		private SymbolMappersCollection[] symbolMappersCollections;

		[SerializeField]
		private int primitiveRectDataPageSize = 2048;

		[SerializeField]
		private int primitiveSquircleDataPageSize = 2048;

		[SerializeField]
		private int primitiveCircleDataPageSize = 1024;

		[SerializeField]
		private int primitiveGlyphDataPageSize = 1024;

		[SerializeField]
		private int primitiveSectorDataPageSize = 1024;

		[SerializeField]
		private int primitiveCurveDataPageSize = 1024;

		[SerializeField]
		private int primitiveCurveSegmentDataPageSize = 1024;

		[SerializeField]
		private int matrix4x4PageSize = 8192;

		[SerializeField]
		private int uintPageSize = 16384;

		[NonSerialized]
		private List<IUIRenderRequest> registeredRenderRequests = new List<IUIRenderRequest>();

		[NonSerialized]
		private HashSet<IUIRenderRequest> renderRequestsToRefresh = new HashSet<IUIRenderRequest>();

		[NonSerialized]
		private UIMaterialCollection loadedMaterialCollection;

		public static readonly Amplitude.Framework.Guid WhiteTextureGuid = new Amplitude.Framework.Guid("c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0c0");

		private const int FrameCountDelayBeforeUnloadingTexture = 360000;

		private static readonly Rect FullRect = new Rect(0f, 0f, 1f, 1f);

		[SerializeField]
		private List<Amplitude.Framework.Guid> textureAtlases = new List<Amplitude.Framework.Guid>();

		[SerializeField]
		[FakeAssetReference(typeof(Texture2D))]
		private Amplitude.Framework.Guid fallback = Amplitude.Framework.Guid.Null;

		[SerializeField]
		[FakeAssetReference(typeof(Texture2D))]
		private Amplitude.Framework.Guid softFallback = Amplitude.Framework.Guid.Null;

		private int currentTextureRevision;

		[NonSerialized]
		private List<TextureRegistration> registeredTextures = new List<TextureRegistration>();

		[NonSerialized]
		private Dictionary<Amplitude.Framework.Guid, TextureInfo> guidToTextureInfoTable = new Dictionary<Amplitude.Framework.Guid, TextureInfo>(Amplitude.Framework.Guid.DefaultComparer);

		[NonSerialized]
		private TextureSourceRepository textureSources;

		[NonSerialized]
		private CacheEntry[] guidToTextureSourceIndexCache = new CacheEntry[64];

		[NonSerialized]
		private Amplitude.Framework.Guid usedTextureGuidIfNull = WhiteTextureGuid;

		[NonSerialized]
		private Texture2D fallbackTexture;

		public static UIRenderingManager Instance => instance;

		public FontRenderingMode FontRenderingMode => fontRenderingMode;

		public UIMaterialCollection MaterialCollection => loadedMaterialCollection;

		internal FontAtlasRenderer FontAtlas
		{
			get
			{
				return fontAtlas;
			}
			set
			{
				fontAtlas = value;
			}
		}

		internal FontFamily DefaultFontFamily
		{
			get
			{
				return defaultFontFamily;
			}
			set
			{
				defaultFontFamily = value;
			}
		}

		internal FontFamily FallbackFontFamily
		{
			get
			{
				return fallbackFontFamily;
			}
			set
			{
				fallbackFontFamily = value;
			}
		}

		internal string UnknownChars
		{
			get
			{
				return unknownChars;
			}
			set
			{
				unknownChars = value;
			}
		}

		internal int PrimitiveGlyphDataPageSize => primitiveGlyphDataPageSize;

		public int CurrentTextureRevision => currentTextureRevision;

		protected UIRenderingManager()
		{
			instance = this;
		}

		public void AddRenderRequest(IUIRenderRequest renderRequest)
		{
			registeredRenderRequests.Add(renderRequest);
			renderRequestsToRefresh.Add(renderRequest);
		}

		public void RemoveRenderRequest(IUIRenderRequest renderRequest)
		{
			registeredRenderRequests.Remove(renderRequest);
			renderRequestsToRefresh.Remove(renderRequest);
			renderRequest.UnbindRenderCommands();
		}

		public void RefreshRenderRequest(IUIRenderRequest renderRequest)
		{
			renderRequestsToRefresh.Add(renderRequest);
		}

		public FontRenderingMode ResolveFontRenderingMode(FontRenderingMode wantedRenderingMode, FontFamily fontFamily)
		{
			if (fontRenderingMode == FontRenderingMode.Default)
			{
				return fontFamily.ResolveRenderingMode(wantedRenderingMode);
			}
			return fontRenderingMode;
		}

		internal void Render()
		{
			if (!base.Loaded)
			{
				return;
			}
			UIAtomRepository.FrameBarrier();
			UnloadAllUnusedTextures();
			List<UIView> allActiveViews = UIView.AllActiveViews;
			if (allActiveViews.Count == 0)
			{
				return;
			}
			if (renderRequestsToRefresh.Count > 0)
			{
				for (int i = 0; i < allActiveViews.Count; i++)
				{
					UIView view = allActiveViews[i];
					foreach (IUIRenderRequest item in renderRequestsToRefresh)
					{
						if (i == 0)
						{
							item.UnbindRenderCommands();
						}
						item.BindRenderCommands(view);
					}
				}
				renderRequestsToRefresh.Clear();
			}
			foreach (UIView item2 in allActiveViews)
			{
				item2.PrimitiveDrawer.Open();
			}
			foreach (UIView item3 in allActiveViews)
			{
				item3.Render();
			}
			FontAtlas.PrepareGlyphs();
			foreach (UIView item4 in allActiveViews)
			{
				item4.PrimitiveDrawer.Close();
			}
			FontAtlas.RepackGlyphs(force: false);
		}

		protected override void Load()
		{
			base.Load();
			instance = this;
			if ((bool)defaultFontFamily && !defaultFontFamily.LoadAssetsIFN())
			{
				Diagnostics.LogError("Issue while loading default font family");
			}
			if ((bool)fallbackFontFamily && !fallbackFontFamily.LoadAssetsIFN())
			{
				Diagnostics.LogError("Issue while loading fallback font family");
			}
			if (loadedMaterialCollection == null)
			{
				loadedMaterialCollection = AssetDatabase.LoadAsset<UIMaterialCollection>(materialCollectionRef, 32u);
			}
			if (loadedMaterialCollection == null)
			{
				Diagnostics.LogError($"Unable to load materialCollection '{materialCollectionRef}'.");
			}
			else
			{
				loadedMaterialCollection.LoadIfNecessary();
			}
			foreach (UIView allActiveView in UIView.AllActiveViews)
			{
				OnViewAdded(allActiveView);
			}
			UIView.ViewAdded -= OnViewAdded;
			UIView.ViewAdded += OnViewAdded;
			UIView.ViewRemoved -= OnViewRemoved;
			UIView.ViewRemoved += OnViewRemoved;
			StartupTextures(reportAtlasNotFound: false, allowAsyncLoading: false);
			ShaderStrings.Load();
			SynchroniseUIAtomContainerOptionEnum synchroniseOption = ((SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan) ? SynchroniseUIAtomContainerOptionEnum.NoPartialUpdate : SynchroniseUIAtomContainerOptionEnum.Default);
			int startPageCount = 3;
			UIAtomRepository.Declare<UIPrimitiveRectData>(primitiveRectDataPageSize, startPageCount, synchroniseOption);
			UIAtomRepository.Declare<UIPrimitiveSquircleData>(primitiveSquircleDataPageSize, startPageCount, synchroniseOption);
			UIAtomRepository.Declare<UIPrimitiveCircleData>(primitiveCircleDataPageSize, startPageCount, synchroniseOption);
			UIAtomRepository.Declare<UIPrimitiveGlyphData>(primitiveGlyphDataPageSize, startPageCount, synchroniseOption);
			UIAtomRepository.Declare<UIPrimitiveSectorData>(primitiveSectorDataPageSize, startPageCount, synchroniseOption);
			UIAtomRepository.Declare<UIPrimitiveCurveData>(primitiveCurveDataPageSize, startPageCount, synchroniseOption);
			UIAtomRepository.Declare<UIPrimitiveCurveSegmentData>(primitiveCurveSegmentDataPageSize, startPageCount, synchroniseOption);
			UIAtomRepository.Declare<Matrix4x4>(matrix4x4PageSize, startPageCount, synchroniseOption);
			UIAtomRepository.Declare<uint>(uintPageSize, startPageCount, synchroniseOption);
			UIAtomContainer<UIPrimitiveRectData>.Global.AddToShaderGlobal(ShaderStrings.RectBuffer);
			UIAtomContainer<UIPrimitiveSquircleData>.Global.AddToShaderGlobal(ShaderStrings.SquircleBuffer);
			UIAtomContainer<UIPrimitiveCircleData>.Global.AddToShaderGlobal(ShaderStrings.CircleBuffer);
			UIAtomContainer<UIPrimitiveGlyphData>.Global.AddToShaderGlobal(ShaderStrings.GlyphBuffer);
			UIAtomContainer<UIPrimitiveSectorData>.Global.AddToShaderGlobal(ShaderStrings.SectorBuffer);
			UIAtomContainer<UIPrimitiveCurveData>.Global.AddToShaderGlobal(ShaderStrings.CurveBuffer);
			UIAtomContainer<UIPrimitiveCurveSegmentData>.Global.AddToShaderGlobal(ShaderStrings.CurveSegmentBuffer);
			UIAtomContainer<Matrix4x4>.Global.AddToShaderGlobal(ShaderStrings.TransformBuffer);
			UIAtomContainer<uint>.Global.AddToShaderGlobal(ShaderStrings.GenericBuffer);
			SymbolMapperController.Load(symbolMappersCollections);
		}

		protected override void Unload()
		{
			UIView.ViewAdded -= OnViewAdded;
			UIView.ViewRemoved -= OnViewRemoved;
			SymbolMapperController.Unload();
			CleanupTextures();
			UIAtomRepository.Release();
			if (loadedMaterialCollection != null)
			{
				loadedMaterialCollection.UnloadIfNecessary();
			}
			loadedMaterialCollection = null;
			defaultFontFamily?.Unload();
			fallbackFontFamily?.Unload();
			base.Unload();
		}

		protected override void Destruct()
		{
			instance = null;
			base.Destruct();
		}

		private void OnViewAdded(UIView view)
		{
			view.LayersChanged -= OnViewLayersChanged;
			view.LayersChanged += OnViewLayersChanged;
			RefreshAllRenderRequests();
		}

		private void OnViewRemoved(UIView view)
		{
			view.LayersChanged -= OnViewLayersChanged;
		}

		private void OnViewLayersChanged(UIView view)
		{
			RefreshAllRenderRequests();
		}

		private void RefreshAllRenderRequests()
		{
			foreach (IUIRenderRequest registeredRenderRequest in registeredRenderRequests)
			{
				renderRequestsToRefresh.Add(registeredRenderRequest);
			}
		}

		private void OnFontRenderingModeChanged(FontRenderingMode previous, FontRenderingMode next)
		{
			fontAtlas.ClearGlyphs();
		}

		[Conditional("UNITY_EDITOR")]
		private void FixupLoadingStatus()
		{
			if (loadedMaterialCollection == null)
			{
				loadedMaterialCollection = AssetDatabase.LoadAsset<UIMaterialCollection>(materialCollectionRef, 32u);
				loadedMaterialCollection.LoadIfNecessary();
			}
		}

		public void GetTextureInfo(Amplitude.Framework.Guid assetGuid, UnityEngine.Object ownerForDebug, out int sourceIndex, out Vector2Int widthHeight, out Rect coordinates)
		{
			if (assetGuid == Amplitude.Framework.Guid.Null || assetGuid == WhiteTextureGuid)
			{
				assetGuid = usedTextureGuidIfNull;
				sourceIndex = 0;
				widthHeight = new Vector2Int(Texture2D.whiteTexture.width, Texture2D.whiteTexture.height);
				coordinates = UITexture.FullRect;
			}
			if (!guidToTextureInfoTable.TryGetValue(assetGuid, out var value))
			{
				FindTextureSource(assetGuid, out var texture, out sourceIndex);
				if (texture == null)
				{
					if (fallbackTexture == null)
					{
						fallbackTexture = GetFallbackTexture();
					}
					texture = fallbackTexture;
					_ = UnityEngine.Application.isPlaying;
				}
				value = new TextureInfo(sourceIndex, assetGuid, texture.width, texture.height, FullRect);
				guidToTextureInfoTable.Add(assetGuid, value);
			}
			sourceIndex = value.TextureSourceIndex;
			widthHeight = value.TextureWidthHeight;
			coordinates = value.Coordinates;
		}

		public void RetrieveTextureSource(int sourceIndex, out Texture result, out bool canKeepReferenceOnTexture)
		{
			if (fallbackTexture == null)
			{
				fallbackTexture = GetFallbackTexture();
			}
			textureSources.RetrieveTextureSource(sourceIndex, out result, out canKeepReferenceOnTexture, fallbackTexture);
		}

		public Amplitude.Framework.Guid RegisterTexture(Texture texture)
		{
			int count = registeredTextures.Count;
			for (int i = 0; i < count; i++)
			{
				if (registeredTextures[i].Texture == texture)
				{
					return registeredTextures[i].Guid;
				}
			}
			Amplitude.Framework.Guid guid = Amplitude.Framework.Guid.NewGuid();
			registeredTextures.Add(new TextureRegistration(texture, guid));
			int textureSourceIndex = InstanciateTexture(guid, texture, persistant: true);
			guidToTextureInfoTable.Add(guid, new TextureInfo(textureSourceIndex, guid, texture.width, texture.height, FullRect));
			return guid;
		}

		public void UnregisterTexture(Texture texture)
		{
			int count = registeredTextures.Count;
			for (int i = 0; i < count; i++)
			{
				if (registeredTextures[i].Texture == texture)
				{
					if (guidToTextureInfoTable.TryGetValue(registeredTextures[i].Guid, out var value))
					{
						textureSources.Destroy(value.TextureSourceIndex);
						guidToTextureInfoTable.Remove(registeredTextures[i].Guid);
					}
					registeredTextures.RemoveAt(i);
					break;
				}
			}
		}

		public Amplitude.Framework.Guid RegisterTextureSourceGenerator(AsyncTextureContentGenerator textureGenerator)
		{
			Amplitude.Framework.Guid guid = textureGenerator.Guid;
			if (!guid.IsNull)
			{
				return guid;
			}
			guid = (textureGenerator.Guid = Amplitude.Framework.Guid.NewGuid());
			int num = textureSources.Instanciate();
			TextureSource source = new TextureSource(guid, textureGenerator);
			textureSources.SetData(num, ref source);
			Vector2Int size = textureGenerator.Size;
			Rect rect = textureGenerator.Rect;
			guidToTextureInfoTable.Add(guid, new TextureInfo(num, guid, size.x, size.y, rect));
			return guid;
		}

		public void UnregisterTextureSourceGenerator(AsyncTextureContentGenerator textureGenerator)
		{
			if (!textureGenerator.Guid.IsNull)
			{
				if (guidToTextureInfoTable.TryGetValue(textureGenerator.Guid, out var value))
				{
					textureSources.Destroy(value.TextureSourceIndex);
					guidToTextureInfoTable.Remove(textureGenerator.Guid);
				}
				textureGenerator.Guid = Amplitude.Framework.Guid.Null;
			}
		}

		private void UnloadAllUnusedTextures(int frameCountDelayBeforeUnloadingTexture = 360000)
		{
			textureSources.UnloadAllUnusedTextures(frameCountDelayBeforeUnloadingTexture);
		}

		private Texture2D GetFallbackTexture()
		{
			if (SoftDebugMissingTexture)
			{
				if (fallbackTexture == null)
				{
					Diagnostics.Log($"Loading 'soft' fallback texture ${softFallback} (nullref: {(object)fallbackTexture == null})");
					fallbackTexture = AssetDatabase.LoadAsset<Texture2D>(softFallback, 32u);
				}
			}
			else if (fallbackTexture == null)
			{
				Diagnostics.Log($"Loading regular fallback texture {fallback} (nullref: {(object)fallbackTexture == null})");
				fallbackTexture = AssetDatabase.LoadAsset<Texture2D>(fallback, 32u);
			}
			if (fallbackTexture == null)
			{
				Diagnostics.LogWarning("Falling back to whiteTexture for the fallback texture.");
				fallbackTexture = Texture2D.whiteTexture;
			}
			return fallbackTexture;
		}

		private void FindTextureSource(Amplitude.Framework.Guid guid, out Texture texture, out int index)
		{
			if (guid == Amplitude.Framework.Guid.Null)
			{
				guid = usedTextureGuidIfNull;
			}
			uint num = (uint)guid.GetHashCode() % (uint)guidToTextureSourceIndexCache.Length;
			if (guid == guidToTextureSourceIndexCache[num].Guid)
			{
				index = guidToTextureSourceIndexCache[num].HandleIndex;
				texture = textureSources.GetTexture(index, fallbackTexture);
				return;
			}
			int textureSourceIndex = textureSources.GetTextureSourceIndex(ref guid);
			if (textureSourceIndex != -1)
			{
				index = textureSources.Indexes[textureSourceIndex].HandleIndex;
				texture = textureSources.GetTexture(index, fallbackTexture);
				guidToTextureSourceIndexCache[num].Guid = guid;
				guidToTextureSourceIndexCache[num].HandleIndex = index;
				return;
			}
			uint assetBundleFlags = 64u;
			texture = AssetDatabase.TryLoadAsset<Texture2D>(guid, assetBundleFlags);
			if (texture == null)
			{
				if (fallbackTexture == null)
				{
					fallbackTexture = GetFallbackTexture();
				}
				texture = fallbackTexture;
			}
			TextureSource source = new TextureSource(guid, texture, persistant: false);
			int num2 = textureSources.Instanciate();
			textureSources.SetData(num2, ref source);
			index = num2;
			guidToTextureSourceIndexCache[num].Guid = guid;
			guidToTextureSourceIndexCache[num].HandleIndex = num2;
		}

		private int InstanciateTexture(Amplitude.Framework.Guid guid, Texture texture, bool persistant)
		{
			int num = textureSources.Instanciate();
			TextureSource source = new TextureSource(guid, texture, persistant);
			textureSources.SetData(num, ref source);
			return num;
		}

		private void StartupTextures(bool reportAtlasNotFound, bool allowAsyncLoading)
		{
			int textureSourceIndex = InstanciateTexture(WhiteTextureGuid, Texture2D.whiteTexture, persistant: true);
			guidToTextureInfoTable.Add(WhiteTextureGuid, new TextureInfo(textureSourceIndex, WhiteTextureGuid, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height, FullRect));
			usedTextureGuidIfNull = WhiteTextureGuid;
			for (int i = 0; i < textureAtlases.Count; i++)
			{
				Amplitude.Framework.Guid guid = textureAtlases[i];
				bool flag = false;
				UITextureAtlas uITextureAtlas = AssetDatabase.TryLoadAsset<UITextureAtlas>(guid, 64u);
				flag = uITextureAtlas == null;
				if (reportAtlasNotFound && flag)
				{
					Diagnostics.LogError("Texture atlas missing!\n Guid:{0}", guid);
				}
				if (uITextureAtlas != null)
				{
					AddAllAtlasEntries(uITextureAtlas);
				}
			}
			currentTextureRevision++;
			UITexture.White.RequestAsset();
			UITexture.None.RequestAsset();
		}

		private void AddAllAtlasEntries(UITextureAtlas atlas)
		{
			int num = atlas.OutputEntries.Length;
			int[] array = new int[num];
			for (int i = 0; i < num; i++)
			{
				int num2 = textureSources.Instanciate();
				TextureSource source = new TextureSource(atlas, i);
				textureSources.SetData(num2, ref source);
				array[i] = num2;
			}
			int num3 = atlas.AtlasEntries.Length;
			for (int j = 0; j < num3; j++)
			{
				UITextureAtlas.AtlasEntry atlasEntry = atlas.AtlasEntries[j];
				TextureInfo value = new TextureInfo(array[atlasEntry.Index], atlasEntry.Guid, atlasEntry.WidthHeight.x, atlasEntry.WidthHeight.y, atlasEntry.Coordinates);
				guidToTextureInfoTable.Add(atlasEntry.Guid, value);
			}
		}

		private void CleanupTextures()
		{
			textureSources.Clear();
			registeredTextures.Clear();
			guidToTextureInfoTable.Clear();
			Array.Clear(guidToTextureSourceIndexCache, 0, guidToTextureSourceIndexCache.Length);
			SymbolMappersCollection[] array = symbolMappersCollections;
			int num = ((array != null) ? array.Length : 0);
			for (int i = 0; i < num; i++)
			{
				symbolMappersCollections[i].Unload();
			}
		}

		private void RestartTextures()
		{
			TextureRegistration[] array = new TextureRegistration[registeredTextures.Count];
			registeredTextures.CopyTo(array);
			CleanupTextures();
			StartupTextures(reportAtlasNotFound: false, allowAsyncLoading: false);
			TextureRegistration[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				TextureRegistration item = array2[i];
				registeredTextures.Add(item);
				int textureSourceIndex = InstanciateTexture(item.Guid, item.Texture, persistant: true);
				guidToTextureInfoTable.Add(item.Guid, new TextureInfo(textureSourceIndex, item.Guid, item.Texture.width, item.Texture.height, FullRect));
			}
		}
	}
}
