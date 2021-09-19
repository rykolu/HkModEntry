using System;
using System.Collections.Generic;
using Amplitude.Framework.Extensions;
using Amplitude.Graphics;
using Amplitude.UI.Renderers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Amplitude.UI
{
	[ExecuteInEditMode]
	public class UIView : UIBehaviour
	{
		private enum Mode
		{
			Fullscreen,
			Viewport,
			Camera
		}

		private class DepthComparer : IComparer<UIView>
		{
			public static readonly DepthComparer Default = new DepthComparer();

			public int Compare(UIView left, UIView right)
			{
				return left.depth.CompareTo(right.depth);
			}
		}

		[HideInInspector]
		public int DrawCallInitialStartSize = 4096;

		[HideInInspector]
		public int RectMasksBufferStartSize = 16;

		[HideInInspector]
		public int RectMaskIdsBufferStartSize = 128;

		[HideInInspector]
		public int MaterialPropertiesBufferStartSize = 64;

		[HideInInspector]
		public int IndexedPrimitiveBufferStartSize = 256;

		[Tooltip("Tell the UI to flip upside down background. Useful when using custom render pipeline.")]
		public bool FlipBackground;

		[NonSerialized]
		public UISortedRenderCommandSet RenderCommands = new UISortedRenderCommandSet(createLists: true);

		private const float EditionWidth = 1920f;

		private const float EditionHeight = 1080f;

		private static List<UIView> allActiveViews = new List<UIView>();

		[SerializeField]
		[UIGroupMask]
		private int groupCullingMask = -1;

		[SerializeField]
		[FormerlySerializedAs("renderCamera")]
		private Camera camera;

		[SerializeField]
		private Rect viewport = new Rect(0f, 0f, 1920f, 1080f);

		[SerializeField]
		private Mode mode;

		[SerializeField]
		private float nearPlane = -1000f;

		[SerializeField]
		private float farPlane = 1000f;

		[SerializeField]
		private int depth;

		[SerializeField]
		private ColorSpace colorSpace;

		[SerializeField]
		private bool receiveInputEvents = true;

		[SerializeField]
		[UILayerList]
		private UILayerList layers = new UILayerList();

		[NonSerialized]
		private int[] orderedLayerIdentifiers;

		[NonSerialized]
		private int lastLayerIdentifier;

		[NonSerialized]
		private int lastLayerIdentifierIndex;

		[NonSerialized]
		private UIPrimitiveDrawer primitiveDrawer;

		[NonSerialized]
		private Matrix4x4 projectionMatrix;

		[NonSerialized]
		private Matrix4x4 projectionMatrixInverse;

		[NonSerialized]
		private Matrix4x4 screenToClipMatrix;

		[NonSerialized]
		private Matrix4x4 globalToScreenMatrix;

		[NonSerialized]
		private Rect standardizedRect = new Rect(0f, 0f, 1f, 1f);

		[NonSerialized]
		private Rect renderedRect = new Rect(0f, 0f, 1f, 1f);

		[NonSerialized]
		private Camera renderCamera;

		public static List<UIView> AllActiveViews => allActiveViews;

		public Camera RenderCamera => renderCamera;

		public Rect Viewport
		{
			get
			{
				return viewport;
			}
			set
			{
				viewport = value;
			}
		}

		public Matrix4x4 ProjectionMatrix => projectionMatrix;

		public Matrix4x4 ScreenToClipMatrix => screenToClipMatrix;

		public Matrix4x4 GlobalToScreenMatrix => globalToScreenMatrix;

		public int OutputWidth => renderCamera.pixelWidth;

		public int OutputHeight => renderCamera.pixelHeight;

		public Vector2 EditionWidthHeight => new Vector2(1920f, 1080f);

		public bool IsFullscreen => mode == Mode.Fullscreen;

		public bool IsDistortedProjection => mode == Mode.Camera;

		public UIPrimitiveDrawer PrimitiveDrawer => primitiveDrawer;

		public int Depth
		{
			get
			{
				return depth;
			}
			set
			{
				int previousDepth = depth;
				if (depth != value)
				{
					depth = value;
					OnDepthChanged(previousDepth, depth);
				}
			}
		}

		public ColorSpace ColorSpace
		{
			get
			{
				if (colorSpace == ColorSpace.Uninitialized)
				{
					return ColorSpace.Linear;
				}
				return colorSpace;
			}
			set
			{
				colorSpace = value;
			}
		}

		public bool ReceiveInputEvents
		{
			get
			{
				return receiveInputEvents;
			}
			set
			{
				receiveInputEvents = value;
			}
		}

		public int GroupCullingMask
		{
			get
			{
				return groupCullingMask;
			}
			set
			{
				groupCullingMask = value;
			}
		}

		public Rect StandardizedRect => standardizedRect;

		public Rect RenderedRect => renderedRect;

		public CommandBuffer CommandBuffer
		{
			get
			{
				if (primitiveDrawer == null)
				{
					return null;
				}
				return primitiveDrawer.CommandBuffer;
			}
		}

		public static event Action<UIView> ViewAdded;

		public static event Action<UIView> ViewRemoved;

		public event Action<UIView> LayersChanged;

		public Vector2 StandardizedPositionToScreenPosition(Vector2 standardizedPosition)
		{
			Vector4 vector = projectionMatrix * new Vector4(standardizedPosition.x, standardizedPosition.y, 0f, 1f);
			float num = vector.x * 0.5f + 0.5f;
			float num2 = (0f - vector.y) * 0.5f + 0.5f;
			num *= (float)OutputWidth;
			num2 *= (float)OutputHeight;
			return new Vector2(num, num2);
		}

		public Vector2 StandardizedPositionToScreenPosition(float standardizedPositionX, float standardizedPositionY)
		{
			return StandardizedPositionToScreenPosition(new Vector2(standardizedPositionX, standardizedPositionY));
		}

		public Vector2 ScreenPositionToStandardizedPosition(Vector2 screenPosition)
		{
			Vector2 normalizedScreenPosition = screenPosition;
			normalizedScreenPosition.x /= OutputWidth;
			normalizedScreenPosition.y /= OutputHeight;
			return NormalizedScreenPositionToStandardizedPosition(normalizedScreenPosition);
		}

		public Vector2 NormalizedScreenPositionToStandardizedPosition(Vector2 normalizedScreenPosition)
		{
			Vector4 vector = new Vector4(0f, 0f, 0f, 1f);
			vector.x = normalizedScreenPosition.x * 2f - 1f;
			vector.y = 0f - (normalizedScreenPosition.y * 2f - 1f);
			return (projectionMatrixInverse * vector).xy();
		}

		public Rect StandardizedRectToScreenRect(Rect standardizedRect)
		{
			Vector2 min = StandardizedPositionToScreenPosition(standardizedRect.min);
			Vector2 max = StandardizedPositionToScreenPosition(standardizedRect.max);
			Rect result = default(Rect);
			result.min = min;
			result.max = max;
			return result;
		}

		public Rect ScreenRectToStandardizedRect(Rect screenRect)
		{
			Vector2 min = ScreenPositionToStandardizedPosition(screenRect.min);
			Vector2 max = ScreenPositionToStandardizedPosition(screenRect.max);
			Rect result = default(Rect);
			result.min = min;
			result.max = max;
			return result;
		}

		public void Render()
		{
			RenderCommands.Render(PrimitiveDrawer);
		}

		public bool ShouldBeViewed(int groupIndex)
		{
			return ((1 << groupIndex) & groupCullingMask) != 0;
		}

		public bool ShouldBeViewed(int groupIndex, int layerIdentifier)
		{
			bool num = ((1 << groupIndex) & groupCullingMask) != 0;
			bool flag = HasLayer(layerIdentifier);
			return num && flag;
		}

		public void AddLayer(int layerIdentifier)
		{
			layers.Elements.Add(new UILayer(layerIdentifier));
			SynchroniseOrderedLayerIdentifiers();
			this.LayersChanged?.Invoke(this);
		}

		public void RemoveLayer(int layerIdentifier)
		{
			for (int i = 0; i < layers.Count; i++)
			{
				if (layers[i].Identifier == layerIdentifier)
				{
					layers.Elements.RemoveAt(i);
					SynchroniseOrderedLayerIdentifiers();
					this.LayersChanged?.Invoke(this);
					break;
				}
			}
		}

		public bool HasLayer(int layerIdentifier)
		{
			if (lastLayerIdentifier == layerIdentifier)
			{
				return lastLayerIdentifierIndex >= 0;
			}
			int num = orderedLayerIdentifiers.Length;
			for (int i = 0; i < num; i++)
			{
				if (orderedLayerIdentifiers[i] == layerIdentifier)
				{
					lastLayerIdentifier = layerIdentifier;
					lastLayerIdentifierIndex = i;
					return true;
				}
			}
			return false;
		}

		public void LayerOrderedIndex(int layerIdentifier, ref int result)
		{
			if (lastLayerIdentifier == layerIdentifier)
			{
				result = lastLayerIdentifierIndex;
				return;
			}
			int num = orderedLayerIdentifiers.Length;
			if (result >= 0 && result < num && orderedLayerIdentifiers[result] == layerIdentifier)
			{
				return;
			}
			result = -1;
			for (int i = 0; i < num; i++)
			{
				if (orderedLayerIdentifiers[i] == layerIdentifier)
				{
					lastLayerIdentifier = layerIdentifier;
					lastLayerIdentifierIndex = i;
					result = i;
				}
			}
		}

		internal void UpdateStandardizedAndRenderedRects()
		{
			float num = (float)OutputWidth / (float)OutputHeight;
			if (num <= 1.77777779f)
			{
				standardizedRect.width = Mathf.CeilToInt(1920f / UIHierarchyManager.GlobalScale);
				standardizedRect.height = Mathf.CeilToInt(standardizedRect.width / num);
			}
			else
			{
				standardizedRect.height = Mathf.CeilToInt(1080f / UIHierarchyManager.GlobalScale);
				standardizedRect.width = Mathf.CeilToInt(standardizedRect.height * num);
			}
			Rect rect = standardizedRect;
			if (mode == Mode.Fullscreen && VirtualScreen.MagnifyingFactor != 1f)
			{
				float width = Mathf.CeilToInt(standardizedRect.width * VirtualScreen.MagnifyingFactor);
				float height = Mathf.CeilToInt(standardizedRect.height * VirtualScreen.MagnifyingFactor);
				float xMin = Mathf.FloorToInt(standardizedRect.width * VirtualScreen.ViewportTranslation.x);
				float yMin = Mathf.FloorToInt(standardizedRect.height * VirtualScreen.ViewportTranslation.y);
				rect.xMin = xMin;
				rect.yMin = yMin;
				rect.width = width;
				rect.height = height;
			}
			if (renderedRect != rect)
			{
				renderedRect = rect;
				if (mode == Mode.Fullscreen)
				{
					viewport = renderedRect;
				}
			}
			if (mode == Mode.Fullscreen || mode == Mode.Viewport)
			{
				projectionMatrix = Matrix4x4.Ortho(viewport.xMin, viewport.xMax, viewport.yMax, viewport.yMin, nearPlane, farPlane);
				projectionMatrixInverse = projectionMatrix.inverse;
				renderCamera.projectionMatrix = projectionMatrix;
			}
			screenToClipMatrix.SetColumn(0, new Vector4(2f / (float)OutputWidth, 0f, 0f, 0f));
			screenToClipMatrix.SetColumn(1, new Vector4(0f, -2f / (float)OutputHeight, 0f, 0f));
			screenToClipMatrix.SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
			screenToClipMatrix.SetColumn(3, new Vector4(-1f, 1f, 0f, 1f));
			globalToScreenMatrix = (GL.GetGPUProjectionMatrix(projectionMatrix, renderIntoTexture: false).inverse * screenToClipMatrix).inverse;
		}

		protected override void Load()
		{
			base.Load();
			renderCamera = camera;
			if (mode == Mode.Fullscreen || mode == Mode.Viewport)
			{
				if (renderCamera == null)
				{
					renderCamera = base.gameObject.GetComponent<Camera>();
				}
				if (renderCamera == null)
				{
					renderCamera = base.gameObject.AddComponent<Camera>();
				}
				renderCamera.clearFlags = CameraClearFlags.Nothing;
				renderCamera.cullingMask = 0;
				renderCamera.allowHDR = false;
				renderCamera.allowMSAA = false;
				renderCamera.forceIntoRenderTexture = true;
				renderCamera.hideFlags = HideFlags.DontSave;
			}
			UIRenderingManager instance = UIRenderingManager.Instance;
			instance.LoadIfNecessary();
			primitiveDrawer = new UIPrimitiveDrawer(this, instance.FontAtlas);
			UpdateStandardizedAndRenderedRects();
			SynchroniseOrderedLayerIdentifiers();
			RegisterView(this);
		}

		protected override void Unload()
		{
			primitiveDrawer?.Release();
			primitiveDrawer = null;
			UnregisterView(this);
			RenderCommands.Clear();
			renderedRect = new Rect(0f, 0f, 1f, 1f);
			renderCamera = null;
			base.Unload();
		}

		private static void RegisterView(UIView view)
		{
			allActiveViews.Add(view);
			allActiveViews.Sort(DepthComparer.Default);
			UIView.ViewAdded?.Invoke(view);
		}

		private static void UnregisterView(UIView view)
		{
			allActiveViews.Remove(view);
			UIView.ViewRemoved?.Invoke(view);
		}

		private void SynchroniseOrderedLayerIdentifiers()
		{
			Array.Resize(ref orderedLayerIdentifiers, layers.Count);
			for (int i = 0; i < layers.Count; i++)
			{
				orderedLayerIdentifiers[i] = layers[i].Identifier;
			}
			lastLayerIdentifier = -1;
			lastLayerIdentifierIndex = -1;
		}

		private void OnLayersChanged()
		{
			this.LayersChanged?.Invoke(this);
		}

		private void OnDepthChanged(int previousDepth, int depth)
		{
			UnregisterView(this);
			RegisterView(this);
		}
	}
}
