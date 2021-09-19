using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Amplitude.Framework.Extensions;
using Amplitude.UI.Animations;
using Amplitude.UI.Interactables;
using Amplitude.UI.Renderers;
using Amplitude.UI.Styles;
using Amplitude.UI.Styles.Scene;
using Amplitude.UI.Traits;
using UnityEngine;
using UnityEngine.Serialization;

namespace Amplitude.UI
{
	[ExecuteInEditMode]
	public class UITransform : UIComponent, IUIStyleTarget, IUITraitPosition, IUITrait<float>, IUITraitRotation, IUITraitLeftBorderAnchor, IUITrait<UIBorderAnchor>, IUITrait<bool>, IUITraitTopBorderAnchor, IUITraitRightBorderAnchor, IUITraitBottomBorderAnchor, IUITraitPivotXAnchor, IUITrait<UIPivotAnchor>, IUITraitPivotYAnchor, IUITraitScale, IUITrait<Vector3>, IUIAnimationTarget
	{
		public delegate void RefreshChildDelegate<TItem, TData>(TItem childItem, TData data, int index);

		public delegate void ShowHideChildDelegate<TItem>(TItem childItem, bool show);

		public delegate void UpdateChildDelegate<TItem>(TItem childItem);

		[Flags]
		public enum MatrixFlags
		{
			HasRotation = 0x1,
			HasScale = 0x2
		}

		[Flags]
		public enum InvalidationFlagsEnum
		{
			None = 0x0,
			AnchorCheck = 0x1,
			LocalToGlobalMatrixCheck = 0x2,
			LocalRectCheck = 0x4,
			VisibleCheck = 0x8,
			InteractiveCheck = 0x10,
			LayerIdentifierCheck = 0x20,
			GroupIndexCheck = 0x40,
			Siblings_OnTransformPositionOrSizeChange_Position = 0x80,
			Siblings_OnTransformPositionOrSizeChange_Size = 0x100,
			Siblings_OnTransformLocalRectChange = 0x200,
			Siblings_OnTransformGlobalPositionChange = 0x400,
			Siblings_OnTransformGlobalSizeChange = 0x800,
			Siblings_OnTransformStateChange = 0x1000,
			Childs_LocalToGlobalMatrixCheck = 0x2000,
			Childs_AnchorCheck = 0x4000,
			Childs_VisibleCheck = 0x8000,
			Childs_InteractiveCheck = 0x10000,
			Childs_LayerIdentifierCheck = 0x20000,
			Childs_GroupIndexCheck = 0x40000,
			Childs_ResetSortingIndex = 0x80000,
			Invoke_PositionOrSizeChange_Position = 0x100000,
			Invoke_PositionOrSizeChange_Size = 0x200000,
			Invoke_OnPivotChange = 0x400000,
			Invoke_VisibleGloballyChange = 0x800000,
			Invoke_GlobalPositionOrSizeChange_Position = 0x1000000,
			Invoke_GlobalPositionOrSizeChange_Size = 0x2000000,
			Checks = 0x7F,
			Siblings = 0x1D80,
			Childs = 0xFE000,
			LocalPosChange = 0x100083,
			LocalRectChange = 0x5,
			LocalScaleChange = 0x2C02,
			LocalRotationChange = 0x2402,
			LocalToGlobalMatrixChange = 0x3002C00
		}

		private struct SiblingEntry
		{
			public UIIndexedComponent IndexedComponent;

			public UIIndexedComponent.EventReceiverFlags ReceiverFlags;
		}

		private struct ReserveChildrenTask : UIBehaviourAsynchronousLoader.ITask
		{
			private readonly UITransform transform;

			private readonly int wantedNumber;

			private readonly Transform prefabTransform;

			private readonly string nameTemplate;

			public ReserveChildrenTask(UITransform transform, int wantedNumber, Transform prefabTransform, string nameTemplate)
			{
				this.transform = transform;
				this.wantedNumber = wantedNumber;
				this.prefabTransform = prefabTransform;
				this.nameTemplate = nameTemplate;
			}

			public IEnumerator Run()
			{
				using (new ScopedCostlyOperationsOnSubHierarchy(transform))
				{
					int count = transform.children.Count;
					int i = count;
					while (i < wantedNumber)
					{
						transform.InstantiateChild(prefabTransform, nameTemplate + i.ToString("D3"));
						yield return null;
						int num = i + 1;
						i = num;
					}
				}
			}
		}

		private struct InstantiateChildInternalTask : UIBehaviourAsynchronousLoader.ITask
		{
			private readonly UITransform transform;

			private readonly Transform prefabTransform;

			private readonly WeakReference<UITransform> childReference;

			private readonly string name;

			public InstantiateChildInternalTask(UITransform transform, Transform prefabTransform, WeakReference<UITransform> childReference, string name)
			{
				this.transform = transform;
				this.prefabTransform = prefabTransform;
				this.childReference = childReference;
				this.name = name;
			}

			public IEnumerator Run()
			{
				UITransform target = transform.InstantiateChild(prefabTransform, name);
				if (childReference != null)
				{
					childReference.Target = target;
				}
				yield break;
			}
		}

		public class ScopedCostlyOperationsOnSubHierarchy : IDisposable
		{
			private UITransform uiTransform;

			private IndexRange sortingRange;

			public ScopedCostlyOperationsOnSubHierarchy(UITransform uiTransform)
			{
				this.uiTransform = uiTransform;
				sortingRange = uiTransform.SortingRange;
				this.uiTransform.UpdateRecursively(InvalidationFlagsEnum.None, IndexRange.Invalid);
			}

			public void Dispose()
			{
				uiTransform.UpdateRecursively(InvalidationFlagsEnum.Checks, sortingRange);
			}
		}

		private static class Mutators
		{
			public static readonly MutatorSet<UITransform, float> X = new MutatorSet<UITransform, float>(ItemIdentifiers.X, (UITransform t) => t.X, delegate(UITransform t, float value)
			{
				t.X = value;
			});

			public static readonly MutatorSet<UITransform, float> Y = new MutatorSet<UITransform, float>(ItemIdentifiers.Y, (UITransform t) => t.Y, delegate(UITransform t, float value)
			{
				t.Y = value;
			});

			public static readonly MutatorSet<UITransform, float> Width = new MutatorSet<UITransform, float>(ItemIdentifiers.Width, (UITransform t) => t.Width, delegate(UITransform t, float value)
			{
				t.Width = value;
			});

			public static readonly MutatorSet<UITransform, float> Height = new MutatorSet<UITransform, float>(ItemIdentifiers.Height, (UITransform t) => t.Height, delegate(UITransform t, float value)
			{
				t.Height = value;
			});

			public static readonly MutatorSet<UITransform, bool> LeftAnchor = new MutatorSet<UITransform, bool>(ItemIdentifiers.LeftAnchor, (UITransform t) => t.LeftAnchor.Attach, delegate(UITransform t, bool value)
			{
				t.LeftAnchor = t.LeftAnchor.SetAttach(value);
			});

			public static readonly MutatorSet<UITransform, float> LeftAnchorPercent = new MutatorSet<UITransform, float>(ItemIdentifiers.LeftAnchorPercent, (UITransform t) => t.LeftAnchor.Percent, delegate(UITransform t, float value)
			{
				t.LeftAnchor = t.LeftAnchor.SetPercent(value);
			});

			public static readonly MutatorSet<UITransform, float> LeftAnchorMargin = new MutatorSet<UITransform, float>(ItemIdentifiers.LeftAnchorMargin, (UITransform t) => t.LeftAnchor.Margin, delegate(UITransform t, float value)
			{
				t.LeftAnchor = t.LeftAnchor.SetMargin(value);
			});

			public static readonly MutatorSet<UITransform, float> LeftAnchorOffset = new MutatorSet<UITransform, float>(ItemIdentifiers.LeftAnchorOffset, (UITransform t) => t.LeftAnchor.Offset, delegate(UITransform t, float value)
			{
				t.LeftAnchor = t.LeftAnchor.SetOffset(value);
			});

			public static readonly MutatorSet<UITransform, bool> RightAnchor = new MutatorSet<UITransform, bool>(ItemIdentifiers.RightAnchor, (UITransform t) => t.RightAnchor.Attach, delegate(UITransform t, bool value)
			{
				t.RightAnchor = t.RightAnchor.SetAttach(value);
			});

			public static readonly MutatorSet<UITransform, float> RightAnchorPercent = new MutatorSet<UITransform, float>(ItemIdentifiers.RightAnchorPercent, (UITransform t) => t.RightAnchor.Percent, delegate(UITransform t, float value)
			{
				t.RightAnchor = t.RightAnchor.SetPercent(value);
			});

			public static readonly MutatorSet<UITransform, float> RightAnchorMargin = new MutatorSet<UITransform, float>(ItemIdentifiers.RightAnchorMargin, (UITransform t) => t.RightAnchor.Margin, delegate(UITransform t, float value)
			{
				t.RightAnchor = t.RightAnchor.SetMargin(value);
			});

			public static readonly MutatorSet<UITransform, float> RightAnchorOffset = new MutatorSet<UITransform, float>(ItemIdentifiers.RightAnchorOffset, (UITransform t) => t.RightAnchor.Offset, delegate(UITransform t, float value)
			{
				t.RightAnchor = t.RightAnchor.SetOffset(value);
			});

			public static readonly MutatorSet<UITransform, bool> TopAnchor = new MutatorSet<UITransform, bool>(ItemIdentifiers.TopAnchor, (UITransform t) => t.TopAnchor.Attach, delegate(UITransform t, bool value)
			{
				t.TopAnchor = t.TopAnchor.SetAttach(value);
			});

			public static readonly MutatorSet<UITransform, float> TopAnchorPercent = new MutatorSet<UITransform, float>(ItemIdentifiers.TopAnchorPercent, (UITransform t) => t.TopAnchor.Percent, delegate(UITransform t, float value)
			{
				t.TopAnchor = t.TopAnchor.SetPercent(value);
			});

			public static readonly MutatorSet<UITransform, float> TopAnchorMargin = new MutatorSet<UITransform, float>(ItemIdentifiers.TopAnchorMargin, (UITransform t) => t.TopAnchor.Margin, delegate(UITransform t, float value)
			{
				t.TopAnchor = t.TopAnchor.SetMargin(value);
			});

			public static readonly MutatorSet<UITransform, float> TopAnchorOffset = new MutatorSet<UITransform, float>(ItemIdentifiers.TopAnchorOffset, (UITransform t) => t.TopAnchor.Offset, delegate(UITransform t, float value)
			{
				t.TopAnchor = t.TopAnchor.SetOffset(value);
			});

			public static readonly MutatorSet<UITransform, bool> BottomAnchor = new MutatorSet<UITransform, bool>(ItemIdentifiers.BottomAnchor, (UITransform t) => t.BottomAnchor.Attach, delegate(UITransform t, bool value)
			{
				t.BottomAnchor = t.BottomAnchor.SetAttach(value);
			});

			public static readonly MutatorSet<UITransform, float> BottomAnchorPercent = new MutatorSet<UITransform, float>(ItemIdentifiers.BottomAnchorPercent, (UITransform t) => t.BottomAnchor.Percent, delegate(UITransform t, float value)
			{
				t.BottomAnchor = t.BottomAnchor.SetPercent(value);
			});

			public static readonly MutatorSet<UITransform, float> BottomAnchorMargin = new MutatorSet<UITransform, float>(ItemIdentifiers.BottomAnchorMargin, (UITransform t) => t.BottomAnchor.Margin, delegate(UITransform t, float value)
			{
				t.BottomAnchor = t.BottomAnchor.SetMargin(value);
			});

			public static readonly MutatorSet<UITransform, float> BottomAnchorOffset = new MutatorSet<UITransform, float>(ItemIdentifiers.BottomAnchorOffset, (UITransform t) => t.BottomAnchor.Offset, delegate(UITransform t, float value)
			{
				t.BottomAnchor = t.BottomAnchor.SetOffset(value);
			});

			public static readonly MutatorSet<UITransform, float> PivotX = new MutatorSet<UITransform, float>(ItemIdentifiers.PivotX, (UITransform t) => t.Pivot.x, delegate(UITransform t, float value)
			{
				t.Pivot = new Vector2(value, t.Pivot.y);
			});

			public static readonly MutatorSet<UITransform, bool> PivotXAttached = new MutatorSet<UITransform, bool>(ItemIdentifiers.PivotXAttached, (UITransform t) => t.PivotXAnchor.Attach, delegate(UITransform t, bool value)
			{
				t.PivotXAnchor = t.PivotXAnchor.SetAttach(value);
			});

			public static readonly MutatorSet<UITransform, float> PivotXPercent = new MutatorSet<UITransform, float>(ItemIdentifiers.PivotXPercent, (UITransform t) => t.PivotXAnchor.Percent, delegate(UITransform t, float value)
			{
				t.PivotXAnchor = t.PivotXAnchor.SetPercent(value);
			});

			public static readonly MutatorSet<UITransform, float> PivotXMinMargin = new MutatorSet<UITransform, float>(ItemIdentifiers.PivotXMinMargin, (UITransform t) => t.PivotXAnchor.MinMargin, delegate(UITransform t, float value)
			{
				t.PivotXAnchor = t.PivotXAnchor.SetMinMargin(value);
			});

			public static readonly MutatorSet<UITransform, float> PivotXMaxMargin = new MutatorSet<UITransform, float>(ItemIdentifiers.PivotXMaxMargin, (UITransform t) => t.PivotXAnchor.MaxMargin, delegate(UITransform t, float value)
			{
				t.PivotXAnchor = t.PivotXAnchor.SetMaxMargin(value);
			});

			public static readonly MutatorSet<UITransform, float> PivotXOffset = new MutatorSet<UITransform, float>(ItemIdentifiers.PivotXOffset, (UITransform t) => t.PivotXAnchor.Offset, delegate(UITransform t, float value)
			{
				t.PivotXAnchor = t.PivotXAnchor.SetOffset(value);
			});

			public static readonly MutatorSet<UITransform, float> PivotY = new MutatorSet<UITransform, float>(ItemIdentifiers.PivotY, (UITransform t) => t.Pivot.y, delegate(UITransform t, float value)
			{
				t.Pivot = new Vector2(t.Pivot.x, value);
			});

			public static readonly MutatorSet<UITransform, bool> PivotYAttached = new MutatorSet<UITransform, bool>(ItemIdentifiers.PivotYAttached, (UITransform t) => t.PivotYAnchor.Attach, delegate(UITransform t, bool value)
			{
				t.PivotYAnchor = t.PivotYAnchor.SetAttach(value);
			});

			public static readonly MutatorSet<UITransform, float> PivotYPercent = new MutatorSet<UITransform, float>(ItemIdentifiers.PivotYPercent, (UITransform t) => t.PivotYAnchor.Percent, delegate(UITransform t, float value)
			{
				t.PivotYAnchor = t.PivotYAnchor.SetPercent(value);
			});

			public static readonly MutatorSet<UITransform, float> PivotYMinMargin = new MutatorSet<UITransform, float>(ItemIdentifiers.PivotYMinMargin, (UITransform t) => t.PivotYAnchor.MinMargin, delegate(UITransform t, float value)
			{
				t.PivotYAnchor = t.PivotYAnchor.SetMinMargin(value);
			});

			public static readonly MutatorSet<UITransform, float> PivotYMaxMargin = new MutatorSet<UITransform, float>(ItemIdentifiers.PivotYMaxMargin, (UITransform t) => t.PivotYAnchor.MaxMargin, delegate(UITransform t, float value)
			{
				t.PivotYAnchor = t.PivotYAnchor.SetMaxMargin(value);
			});

			public static readonly MutatorSet<UITransform, float> PivotYOffset = new MutatorSet<UITransform, float>(ItemIdentifiers.PivotYOffset, (UITransform t) => t.PivotYAnchor.Offset, delegate(UITransform t, float value)
			{
				t.PivotYAnchor = t.PivotYAnchor.SetOffset(value);
			});

			public static readonly MutatorSet<UITransform, float> Z = new MutatorSet<UITransform, float>(ItemIdentifiers.Z, (UITransform t) => t.Z, delegate(UITransform t, float value)
			{
				t.Z = value;
			});

			public static readonly MutatorSet<UITransform, float> ScaleX = new MutatorSet<UITransform, float>(ItemIdentifiers.ScaleX, (UITransform t) => t.Scale.x, delegate(UITransform t, float value)
			{
				t.Scale = new Vector3(value, t.Scale.y, t.Scale.z);
			});

			public static readonly MutatorSet<UITransform, float> ScaleY = new MutatorSet<UITransform, float>(ItemIdentifiers.ScaleY, (UITransform t) => t.Scale.y, delegate(UITransform t, float value)
			{
				t.Scale = new Vector3(t.Scale.x, value, t.Scale.z);
			});

			public static readonly MutatorSet<UITransform, float> ScaleZ = new MutatorSet<UITransform, float>(ItemIdentifiers.ScaleZ, (UITransform t) => t.Scale.z, delegate(UITransform t, float value)
			{
				t.Scale = new Vector3(t.Scale.x, t.Scale.y, value);
			});

			public static readonly MutatorSet<UITransform, float> RotationX = new MutatorSet<UITransform, float>(ItemIdentifiers.RotationX, (UITransform t) => t.Rotation.x, delegate(UITransform t, float value)
			{
				t.Rotation = new Vector3(value, t.Rotation.y, t.Rotation.z);
			});

			public static readonly MutatorSet<UITransform, float> RotationY = new MutatorSet<UITransform, float>(ItemIdentifiers.RotationY, (UITransform t) => t.Rotation.y, delegate(UITransform t, float value)
			{
				t.Rotation = new Vector3(t.Rotation.x, value, t.Rotation.z);
			});

			public static readonly MutatorSet<UITransform, float> RotationZ = new MutatorSet<UITransform, float>(ItemIdentifiers.RotationZ, (UITransform t) => t.Rotation.z, delegate(UITransform t, float value)
			{
				t.Rotation = new Vector3(t.Rotation.x, t.Rotation.y, value);
			});
		}

		public static PerformanceList<UITransform> Roots;

		private const float Epsilon = 0.01f;

		[SerializeField]
		private bool visibleSelf = true;

		[SerializeField]
		[FormerlySerializedAs("enableSelf")]
		private bool interactiveSelf = true;

		[SerializeField]
		private Vector3 position = Vector3.zero;

		[SerializeField]
		private Vector3 rotation = Vector3.zero;

		[SerializeField]
		private Vector3 scale = Vector3.one;

		[SerializeField]
		private Vector2 widthHeight = new Vector2(100f, 50f);

		[SerializeField]
		private Vector2 pivot = Vector2.zero;

		[SerializeField]
		private UIBorderAnchor leftAnchor = new UIBorderAnchor(attach: false, 0f, 0f, 0f);

		[SerializeField]
		private UIBorderAnchor rightAnchor = new UIBorderAnchor(attach: false, 1f, 0f, 0f);

		[SerializeField]
		private UIBorderAnchor topAnchor = new UIBorderAnchor(attach: false, 0f, 0f, 0f);

		[SerializeField]
		private UIBorderAnchor bottomAnchor = new UIBorderAnchor(attach: false, 1f, 0f, 0f);

		[SerializeField]
		private UIPivotAnchor pivotXAnchor = new UIPivotAnchor(attach: false, 0f, 0f, 0f, 0f);

		[SerializeField]
		private UIPivotAnchor pivotYAnchor = new UIPivotAnchor(attach: false, 0f, 0f, 0f, 0f);

		[SerializeField]
		private int resizeWeight;

		[SerializeField]
		[UILayerIdentifier]
		private int layerIdentifierSelf = -1;

		[SerializeField]
		[UIGroupIndex]
		private int groupIndexSelf = -1;

		[SerializeField]
		private UIStyleController styleController;

		private bool visibleGlobally;

		private bool interactiveGlobally;

		private int layerIdentifierGlobally = -1;

		private int groupIndexGlobally = -1;

		private Matrix4x4 localToGlobalMatrix = Matrix4x4.identity;

		private MatrixFlags localToGlobalMatrixFlags;

		private UIAtomId matrixAtomId = UIAtomId.Invalid;

		private InvalidationFlagsEnum invalidationFlags;

		private Rect lastLocalRectChangeEvent = Rect.zero;

		private IndexRange sortingRange = IndexRange.Invalid;

		private UITransform parent;

		private PerformanceList<UITransform> children;

		private UIIndexedComponent.EventReceiverFlags siblingsFlagsUnion;

		private PerformanceList<SiblingEntry> siblings;

		public bool VisibleSelf
		{
			get
			{
				return visibleSelf;
			}
			set
			{
				if (visibleSelf != value)
				{
					visibleSelf = value;
					UpdateRecursively(InvalidationFlagsEnum.VisibleCheck, sortingRange);
				}
			}
		}

		public bool VisibleGlobally => visibleGlobally;

		public bool InteractiveSelf
		{
			get
			{
				return interactiveSelf;
			}
			set
			{
				if (interactiveSelf != value)
				{
					interactiveSelf = value;
					UpdateRecursively(InvalidationFlagsEnum.InteractiveCheck, sortingRange);
				}
			}
		}

		public bool InteractiveGlobally => interactiveGlobally;

		public Rect Rect
		{
			get
			{
				Vector2 vector = default(Vector2);
				vector.x = position.x - pivot.x * widthHeight.x;
				vector.y = position.y - pivot.y * widthHeight.y;
				return new Rect(vector.x, vector.y, widthHeight.x, widthHeight.y);
			}
			set
			{
				Vector2 vector = default(Vector2);
				vector.x = pivot.x * widthHeight.x;
				vector.y = pivot.y * widthHeight.y;
				Vector2 vector2 = default(Vector2);
				vector2.x = position.x - vector.x;
				vector2.y = position.y - vector.y;
				float num = value.x - vector2.x;
				float num2 = value.y - vector2.y;
				num = ((num < 0f) ? (0f - num) : num);
				num2 = ((num2 < 0f) ? (0f - num2) : num2);
				bool num3 = num > 0.001f || num2 > 0.001f;
				bool flag = widthHeight.x != value.width || widthHeight.y != value.height;
				InvalidationFlagsEnum invalidationFlagsEnum = InvalidationFlagsEnum.None;
				if (num3)
				{
					position.x = value.x + vector.x;
					position.y = value.y + vector.y;
					invalidationFlagsEnum |= InvalidationFlagsEnum.LocalPosChange;
				}
				if (flag)
				{
					widthHeight.x = value.width;
					widthHeight.y = value.height;
					invalidationFlagsEnum |= InvalidationFlagsEnum.LocalRectChange;
				}
				if (invalidationFlagsEnum != 0)
				{
					UpdateRecursively(invalidationFlagsEnum, sortingRange);
				}
			}
		}

		public Rect LocalRect
		{
			get
			{
				Vector2 vector = default(Vector2);
				vector.x = (0f - pivot.x) * widthHeight.x;
				vector.y = (0f - pivot.y) * widthHeight.y;
				return new Rect(vector.x, vector.y, widthHeight.x, widthHeight.y);
			}
		}

		public Vector2 TopLeft
		{
			get
			{
				Vector2 result = position;
				result.x -= pivot.x * widthHeight.x;
				result.y -= pivot.y * widthHeight.y;
				return result;
			}
			set
			{
				Vector2 vector = position;
				vector.x -= pivot.x * widthHeight.x;
				vector.y -= pivot.y * widthHeight.y;
				if (vector != value)
				{
					Vector2 vector2 = value;
					vector2.x += pivot.x * widthHeight.x;
					vector2.y += pivot.y * widthHeight.y;
					position.x = vector2.x;
					position.y = vector2.y;
					UpdateRecursively(InvalidationFlagsEnum.LocalPosChange, sortingRange);
				}
			}
		}

		public ref Matrix4x4 LocalToGlobalMatrix => ref localToGlobalMatrix;

		public MatrixFlags LocalToGlobalMatrixFlags => localToGlobalMatrixFlags;

		public Rect GlobalRect
		{
			get
			{
				Vector4 vector = default(Vector4);
				vector.x = (0f - pivot.x) * widthHeight.x;
				vector.y = (0f - pivot.y) * widthHeight.y;
				vector.z = 0f;
				vector.w = 1f;
				Vector4 vector2 = localToGlobalMatrix * vector;
				return new Rect(vector2.x, vector2.y, widthHeight.x, widthHeight.y);
			}
		}

		public Vector3 GlobalPosition => localToGlobalMatrix.GetColumn(3);

		public float Left
		{
			get
			{
				return position.x - pivot.x * widthHeight.x;
			}
			set
			{
				if (position.x - pivot.x * widthHeight.x != value)
				{
					position.x = value + pivot.x * widthHeight.x;
					UpdateRecursively(InvalidationFlagsEnum.LocalPosChange, sortingRange);
				}
			}
		}

		public float Top
		{
			get
			{
				return position.y - pivot.y * widthHeight.y;
			}
			set
			{
				if (position.y - pivot.y * widthHeight.y != value)
				{
					position.y = value + pivot.y * widthHeight.y;
					UpdateRecursively(InvalidationFlagsEnum.LocalPosChange, sortingRange);
				}
			}
		}

		public float Right
		{
			get
			{
				return position.x + (1f - pivot.x) * widthHeight.x;
			}
			set
			{
				if (position.x + (1f - pivot.x) * widthHeight.x != value)
				{
					position.x = value - (1f - pivot.x) * widthHeight.x;
					UpdateRecursively(InvalidationFlagsEnum.LocalPosChange, sortingRange);
				}
			}
		}

		public float Bottom
		{
			get
			{
				return position.y + (1f - pivot.y) * widthHeight.y;
			}
			set
			{
				if (position.y + (1f - pivot.y) * widthHeight.y != value)
				{
					position.y = value - (1f - pivot.y) * widthHeight.y;
					UpdateRecursively(InvalidationFlagsEnum.LocalPosChange, sortingRange);
				}
			}
		}

		public float Width
		{
			get
			{
				return widthHeight.x;
			}
			set
			{
				if (widthHeight.x != value)
				{
					widthHeight.x = value;
					UpdateRecursively(InvalidationFlagsEnum.LocalRectChange, sortingRange);
				}
			}
		}

		public float Height
		{
			get
			{
				return widthHeight.y;
			}
			set
			{
				if (widthHeight.y != value)
				{
					widthHeight.y = value;
					UpdateRecursively(InvalidationFlagsEnum.LocalRectChange, sortingRange);
				}
			}
		}

		public Vector2 WidthHeight
		{
			get
			{
				return widthHeight;
			}
			set
			{
				if (widthHeight != value)
				{
					widthHeight = value;
					UpdateRecursively(InvalidationFlagsEnum.LocalRectChange, sortingRange);
				}
			}
		}

		public Vector2 Pivot
		{
			get
			{
				return pivot;
			}
			set
			{
				if (pivot != value)
				{
					Vector2 previousPivot = pivot;
					pivot = value;
					OnPivotChanged(previousPivot, pivot);
				}
			}
		}

		public Vector2 Position2D
		{
			get
			{
				Vector2 result = default(Vector2);
				result.x = position.x;
				result.y = position.y;
				return result;
			}
			set
			{
				if (value != position.xy())
				{
					position = value;
					UpdateRecursively(InvalidationFlagsEnum.LocalPosChange, sortingRange);
				}
			}
		}

		public Vector3 Position
		{
			get
			{
				return position;
			}
			set
			{
				if (value != position)
				{
					position = value;
					UpdateRecursively(InvalidationFlagsEnum.LocalPosChange, sortingRange);
				}
			}
		}

		public Vector3 Rotation
		{
			get
			{
				return rotation;
			}
			set
			{
				if (value != rotation)
				{
					rotation = value;
					UpdateRecursively(InvalidationFlagsEnum.LocalRotationChange, sortingRange);
				}
			}
		}

		public Vector3 Scale
		{
			get
			{
				return scale;
			}
			set
			{
				if (value != scale)
				{
					scale = value;
					UpdateRecursively(InvalidationFlagsEnum.LocalScaleChange, sortingRange);
				}
			}
		}

		public float X
		{
			get
			{
				return position.x;
			}
			set
			{
				if (position.x != value)
				{
					position.x = value;
					UpdateRecursively(InvalidationFlagsEnum.LocalPosChange, sortingRange);
				}
			}
		}

		public float Y
		{
			get
			{
				return position.y;
			}
			set
			{
				if (position.y != value)
				{
					position.y = value;
					UpdateRecursively(InvalidationFlagsEnum.LocalPosChange, sortingRange);
				}
			}
		}

		public float Z
		{
			get
			{
				return position.z;
			}
			set
			{
				if (position.z != value)
				{
					position.z = value;
					UpdateRecursively(InvalidationFlagsEnum.LocalPosChange, sortingRange);
				}
			}
		}

		public UIBorderAnchor LeftAnchor
		{
			get
			{
				return leftAnchor;
			}
			set
			{
				if (leftAnchor != value)
				{
					leftAnchor = value;
					if (HasAnyAnchor)
					{
						UpdateRecursively(InvalidationFlagsEnum.AnchorCheck, sortingRange);
					}
				}
			}
		}

		public UIBorderAnchor RightAnchor
		{
			get
			{
				return rightAnchor;
			}
			set
			{
				if (rightAnchor != value)
				{
					rightAnchor = value;
					if (HasAnyAnchor)
					{
						UpdateRecursively(InvalidationFlagsEnum.AnchorCheck, sortingRange);
					}
				}
			}
		}

		public UIBorderAnchor TopAnchor
		{
			get
			{
				return topAnchor;
			}
			set
			{
				if (topAnchor != value)
				{
					topAnchor = value;
					if (HasAnyAnchor)
					{
						UpdateRecursively(InvalidationFlagsEnum.AnchorCheck, sortingRange);
					}
				}
			}
		}

		public UIBorderAnchor BottomAnchor
		{
			get
			{
				return bottomAnchor;
			}
			set
			{
				if (bottomAnchor != value)
				{
					bottomAnchor = value;
					if (HasAnyAnchor)
					{
						UpdateRecursively(InvalidationFlagsEnum.AnchorCheck, sortingRange);
					}
				}
			}
		}

		public UIPivotAnchor PivotXAnchor
		{
			get
			{
				return pivotXAnchor;
			}
			set
			{
				if (pivotXAnchor != value)
				{
					pivotXAnchor = value;
					if (HasAnyAnchor)
					{
						UpdateRecursively(InvalidationFlagsEnum.AnchorCheck, sortingRange);
					}
				}
			}
		}

		public UIPivotAnchor PivotYAnchor
		{
			get
			{
				return pivotYAnchor;
			}
			set
			{
				if (pivotYAnchor != value)
				{
					pivotYAnchor = value;
					if (HasAnyAnchor)
					{
						UpdateRecursively(InvalidationFlagsEnum.AnchorCheck, sortingRange);
					}
				}
			}
		}

		public int ResizeWeight
		{
			get
			{
				return resizeWeight;
			}
			set
			{
				if (resizeWeight != value)
				{
					resizeWeight = value;
				}
			}
		}

		public int LayerIdentifierSelf
		{
			get
			{
				return layerIdentifierSelf;
			}
			set
			{
				if (layerIdentifierSelf != value)
				{
					layerIdentifierSelf = value;
					UpdateRecursively(InvalidationFlagsEnum.LayerIdentifierCheck, sortingRange);
				}
			}
		}

		public int LayerIdentifierGlobally => layerIdentifierGlobally;

		public int GroupIndexSelf
		{
			get
			{
				return groupIndexSelf;
			}
			set
			{
				if (groupIndexSelf != value)
				{
					groupIndexSelf = value;
					UpdateRecursively(InvalidationFlagsEnum.GroupIndexCheck, sortingRange);
				}
			}
		}

		public int GroupIndexGlobally => groupIndexGlobally;

		public ref PerformanceList<UITransform> Children => ref children;

		public UITransform Parent => parent;

		public ref UIStyleController StyleController => ref styleController;

		public ref UIAtomId MatrixAtomId => ref matrixAtomId;

		internal IndexRange SortingRange => sortingRange;

		internal bool HasAnyAnchor
		{
			get
			{
				if (!leftAnchor.Attach && !rightAnchor.Attach && !pivotXAnchor.Attach && !topAnchor.Attach && !bottomAnchor.Attach)
				{
					return pivotYAnchor.Attach;
				}
				return true;
			}
		}

		internal IndexRange ChildrenIndexRange
		{
			get
			{
				if (sortingRange != IndexRange.Invalid)
				{
					int num = Math.Max(siblings.Count, 8);
					long min = sortingRange.Min + num + 1;
					long max = sortingRange.Max - num - 1;
					return new IndexRange(min, max);
				}
				return IndexRange.Invalid;
			}
		}

		protected override bool CanBeDisabled => false;

		internal bool IsPrefabModeRoot { get; set; }

		public event Action<UITransform, CollectionChangeAction> ChildrenChange;

		public event Action<bool> VisibleGloballyChange;

		public event Action<bool, bool> PositionOrSizeChange;

		public event Action<bool, bool> GlobalPositionOrSizeChange;

		public event Action PivotChange;

		public void ReserveChildren(int wantedNumber, Transform prefabTransform, string nameTemplate = "Item")
		{
			if (children.Count >= wantedNumber)
			{
				return;
			}
			using (new ScopedCostlyOperationsOnSubHierarchy(this))
			{
				for (int i = children.Count; i < wantedNumber; i++)
				{
					InstantiateChild(prefabTransform, nameTemplate + i.ToString("D3"));
				}
			}
		}

		public IEnumerator DoReserveChildren(int wantedNumber, Transform prefabTransform, string nameTemplate = "Item")
		{
			if (children.Count < wantedNumber)
			{
				yield return UIBehaviourAsynchronousLoader.StartAsyncLoading(new ReserveChildrenTask(this, wantedNumber, prefabTransform, nameTemplate));
			}
		}

		public void DestroyChildren()
		{
			using (new ScopedCostlyOperationsOnSubHierarchy(this))
			{
				for (int num = children.Count - 1; num >= 0; num--)
				{
					UnityEngine.Object.DestroyImmediate(children.Data[num].gameObject);
				}
				children.Clear();
			}
		}

		public UITransform InstantiateChild(Transform prefabTransform, string name = null)
		{
			_ = prefabTransform == null;
			Stopwatch stopwatch = default(Stopwatch);
			stopwatch.Start();
			Transform obj = UnityEngine.Object.Instantiate(prefabTransform, base.transform);
			if (string.IsNullOrEmpty(name))
			{
				name = prefabTransform.name;
			}
			obj.name = name;
			UITransform component = obj.GetComponent<UITransform>();
			if (UISettings.BudgetPerInstantiationWarning.Value > 0f)
			{
				_ = stopwatch.ElapsedMilliseconds;
				_ = (double)UISettings.BudgetPerInstantiationWarning.Value;
			}
			return component;
		}

		public IEnumerator DoInstantiateChild(Transform prefabTransform, WeakReference<UITransform> childReference, string name = null)
		{
			yield return UIBehaviourAsynchronousLoader.StartAsyncLoading(new InstantiateChildInternalTask(this, prefabTransform, childReference, name));
		}

		public void RefreshChildren<TData>(IList<TData> dataList, RefreshChildDelegate<UITransform, TData> refreshChildDelegate)
		{
			RefreshChildren(dataList, refreshChildDelegate, null);
		}

		public void RefreshChildren<TItem, TData>(IList<TData> dataList, RefreshChildDelegate<TItem, TData> refreshChildDelegate, ShowHideChildDelegate<TItem> showHideChildDelegate = null)
		{
			int num = dataList?.Count ?? 0;
			int count = children.Count;
			if (num > count)
			{
				Diagnostics.LogError($"Trying to call RefreshChildren on parent '{this}' for {num} objects but it only has {count} children.");
				return;
			}
			for (int i = 0; i < count; i++)
			{
				TItem component = children.Data[i].GetComponent<TItem>();
				if (component == null)
				{
					Diagnostics.LogError($"Trying to call the refresh delegate on child '{children.Data[i]}' but it doesn't have any component of type '{typeof(TItem)}'.");
					continue;
				}
				bool flag = i < num;
				if (showHideChildDelegate != null)
				{
					showHideChildDelegate(component, flag);
				}
				else
				{
					(component as UIComponent).UITransform.VisibleSelf = flag;
				}
				if (flag)
				{
					refreshChildDelegate(component, dataList[i], i);
				}
			}
		}

		public void HideChildren<TItem>(UpdateChildDelegate<TItem> hideChildDelegate)
		{
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				if (children.Data[i].VisibleSelf)
				{
					children.Data[i].VisibleSelf = false;
					TItem component = children.Data[i].GetComponent<TItem>();
					if (component == null)
					{
						Diagnostics.LogError($"Trying to call the hide delegate on child '{children.Data[i]}' but it doesn't have any component of type '{typeof(TItem)}'.");
					}
					else
					{
						hideChildDelegate(component);
					}
				}
			}
		}

		public IEnumerable<TItem> GetChildren<TItem>(bool visibleOnly = true)
		{
			int childrenCount = children.Count;
			int i = 0;
			while (i < childrenCount)
			{
				UITransform uITransform = children.Data[i];
				if (!visibleOnly || uITransform.VisibleSelf)
				{
					TItem component = uITransform.GetComponent<TItem>();
					if (component != null)
					{
						yield return component;
					}
				}
				int num = i + 1;
				i = num;
			}
		}

		public void SetGlobalPosition(Vector3 globalPosition)
		{
			Position += globalPosition - GlobalPosition;
		}

		public void AlignRightToRightmostVisibleChild(float horizontalMargin = 0f)
		{
			if (leftAnchor.Attach && rightAnchor.Attach)
			{
				Diagnostics.LogWarning($"Trying to fit {this} right border to one of its children's right border but it is attached to both left and right.");
				return;
			}
			Rect rect = Rect;
			UITransform uITransform = FindRightmostVisibleChild();
			if (uITransform != null)
			{
				rect.width = uITransform.Right + horizontalMargin;
			}
			else
			{
				rect.width = 0f;
			}
			if (Mathf.Abs(Rect.width - rect.width) > 0.01f)
			{
				widthHeight.x = rect.width;
				widthHeight.y = rect.height;
				position.x = rect.x + pivot.x * widthHeight.x;
				position.y = rect.y + pivot.y * widthHeight.y;
				UpdateRecursively(InvalidationFlagsEnum.LocalPosChange | InvalidationFlagsEnum.LocalRectCheck, sortingRange);
			}
		}

		public void AlignLeftToLeftmostVisibleChild(float horizontalMargin = 0f)
		{
			if (leftAnchor.Attach && rightAnchor.Attach)
			{
				Diagnostics.LogWarning($"Trying to fit {this} left border to one of its children's left border but it is attached to both left and right.");
				return;
			}
			Rect rect = Rect;
			UITransform uITransform = FindLeftmostVisibleChild();
			if (uITransform != null)
			{
				rect.xMin += uITransform.TopLeft.x - horizontalMargin;
			}
			else
			{
				rect.xMin += rect.width;
			}
			if (!(Mathf.Abs(Rect.x - rect.x) > 0.01f) && !(Mathf.Abs(Rect.width - rect.width) > 0.01f))
			{
				return;
			}
			float num = rect.x - TopLeft.x;
			widthHeight.x = rect.width;
			widthHeight.y = rect.height;
			position.x = rect.x + pivot.x * widthHeight.x;
			position.y = rect.y + pivot.y * widthHeight.y;
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				UITransform uITransform2 = children.Data[i];
				if (uITransform2.visibleSelf)
				{
					uITransform2.position.x += num;
					uITransform2.invalidationFlags |= InvalidationFlagsEnum.LocalPosChange;
				}
			}
			UpdateRecursively(InvalidationFlagsEnum.LocalPosChange | InvalidationFlagsEnum.LocalRectCheck | InvalidationFlagsEnum.Childs_LocalToGlobalMatrixCheck, sortingRange);
		}

		public void AlignBottomToBottommostVisibleChild(float verticalMargin = 0f)
		{
			if (topAnchor.Attach && bottomAnchor.Attach)
			{
				Diagnostics.LogWarning($"Trying to fit {this} bottom border to one of its children's bottom border but it is attached to top and bottom.");
				return;
			}
			Rect rect = Rect;
			UITransform uITransform = FindBottommostVisibleChild();
			if (uITransform != null)
			{
				rect.height = uITransform.Bottom + verticalMargin;
			}
			else
			{
				rect.height = 0f;
			}
			if (Mathf.Abs(Rect.height - rect.height) > 0.01f)
			{
				widthHeight.x = rect.width;
				widthHeight.y = rect.height;
				position.x = rect.x + pivot.x * widthHeight.x;
				position.y = rect.y + pivot.y * widthHeight.y;
				UpdateRecursively(InvalidationFlagsEnum.LocalPosChange | InvalidationFlagsEnum.LocalRectCheck, sortingRange);
			}
		}

		public void AlignTopToTopmostVisibleChild(float verticalMargin = 0f)
		{
			if (topAnchor.Attach && bottomAnchor.Attach)
			{
				Diagnostics.LogWarning($"Trying to fit {this} top border to one of its children's top border but it is attached to both top and bottom.");
				return;
			}
			Rect rect = Rect;
			UITransform uITransform = FindTopmostVisibleChild();
			if (uITransform != null)
			{
				rect.yMin += uITransform.TopLeft.y - verticalMargin;
			}
			else
			{
				rect.yMin += rect.height;
			}
			if (!(Mathf.Abs(Rect.y - rect.y) > 0.01f) && !(Mathf.Abs(Rect.height - rect.height) > 0.01f))
			{
				return;
			}
			float num = rect.y - TopLeft.y;
			widthHeight.x = rect.width;
			widthHeight.y = rect.height;
			position.x = rect.x + pivot.x * widthHeight.x;
			position.y = rect.y + pivot.y * widthHeight.y;
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				UITransform uITransform2 = children.Data[i];
				if (uITransform2.visibleSelf)
				{
					uITransform2.position.y += num;
					uITransform2.invalidationFlags |= InvalidationFlagsEnum.LocalPosChange;
				}
			}
			UpdateRecursively(InvalidationFlagsEnum.LocalPosChange | InvalidationFlagsEnum.LocalRectCheck | InvalidationFlagsEnum.Childs_LocalToGlobalMatrixCheck, sortingRange);
		}

		public bool Contains(Vector2 standardizedPosition)
		{
			if (localToGlobalMatrixFlags == (MatrixFlags)0)
			{
				Vector2 vector = standardizedPosition;
				vector.x -= localToGlobalMatrix.m03;
				vector.y -= localToGlobalMatrix.m13;
				vector.x += pivot.x * widthHeight.x;
				vector.y += pivot.y * widthHeight.y;
				if (vector.x >= 0f && vector.x < widthHeight.x && vector.y >= 0f)
				{
					return vector.y < widthHeight.y;
				}
				return false;
			}
			if (localToGlobalMatrixFlags == MatrixFlags.HasScale)
			{
				Vector2 vector2 = standardizedPosition;
				vector2.x -= localToGlobalMatrix.m03;
				vector2.y -= localToGlobalMatrix.m13;
				vector2.x /= localToGlobalMatrix.m00;
				vector2.y /= localToGlobalMatrix.m11;
				vector2.x += pivot.x * widthHeight.x;
				vector2.y += pivot.y * widthHeight.y;
				if (vector2.x >= 0f && vector2.x < widthHeight.x && vector2.y >= 0f)
				{
					return vector2.y < widthHeight.y;
				}
				return false;
			}
			Matrix4x4 inverse = localToGlobalMatrix.inverse;
			Vector4 vector3 = inverse * standardizedPosition.xy01();
			Vector4 vector4 = inverse * Vector3.forward;
			float num = vector3.z / (0f - vector4.z);
			Vector4 instance = vector3 + vector4 * num;
			return LocalRect.Contains(instance.xy());
		}

		public void OnReactivityChanged(UIReactivityState reactivityState, UITransform originUiTransform, bool instant)
		{
			styleController.UpdateReactivity(ref reactivityState, instant);
			if (originUiTransform != this && (siblingsFlagsUnion & UIIndexedComponent.EventReceiverFlags.IsUIControl) != 0)
			{
				return;
			}
			if ((siblingsFlagsUnion & UIIndexedComponent.EventReceiverFlags.OnReactivityChanged) != 0)
			{
				int count = siblings.Count;
				for (int i = 0; i < count; i++)
				{
					if ((siblings.Data[i].ReceiverFlags & UIIndexedComponent.EventReceiverFlags.OnReactivityChanged) != 0)
					{
						siblings.Data[i].IndexedComponent.InternalOnReactivityChanged(ref reactivityState, instant);
					}
				}
			}
			int count2 = children.Count;
			for (int j = 0; j < count2; j++)
			{
				children.Data[j].OnReactivityChanged(reactivityState, originUiTransform, instant);
			}
		}

		public bool IsStyleValueApplied(StaticString identifier)
		{
			return styleController.ContainsValue(identifier);
		}

		void IUIAnimationTarget.CreateAnimationItems(IUIAnimationItemsCollection animationItemsCollection)
		{
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.X);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.Y);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.Width);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.Height);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.LeftAnchor);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.LeftAnchorPercent);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.LeftAnchorMargin);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.LeftAnchorOffset);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.RightAnchor);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.RightAnchorPercent);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.RightAnchorMargin);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.RightAnchorOffset);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.TopAnchor);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.TopAnchorPercent);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.TopAnchorMargin);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.TopAnchorOffset);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.BottomAnchor);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.BottomAnchorPercent);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.BottomAnchorMargin);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.BottomAnchorOffset);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.PivotX);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.PivotXAttached);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.PivotXPercent);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.PivotXMinMargin);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.PivotXMaxMargin);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.PivotXOffset);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.PivotY);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.PivotYAttached);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.PivotYPercent);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.PivotYMinMargin);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.PivotYMaxMargin);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.PivotYOffset);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.Z);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.ScaleX);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.ScaleY);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.ScaleZ);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.RotationX);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.RotationY);
			IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, Mutators.RotationZ);
		}

		internal void InitializeRecursively(IndexRange sortingRange)
		{
			UpdateRecursively(InvalidationFlagsEnum.Checks, sortingRange);
		}

		internal void ResetRecursively()
		{
			UpdateRecursively(InvalidationFlagsEnum.None, IndexRange.Invalid);
		}

		internal void ForceUpdatePositionGloballyRecursively()
		{
			UpdateRecursively(InvalidationFlagsEnum.LocalPosChange | InvalidationFlagsEnum.LocalRectCheck, sortingRange);
		}

		internal void OnParentChanged(UITransform newParent)
		{
			if (parent != null)
			{
				parent.UnregisterChild(this);
			}
			else
			{
				UIHierarchyManager.Instance?.UnregisterRoot(this);
			}
			if (newParent != null)
			{
				newParent.LoadIfNecessary();
				newParent.RegisterChild(this);
			}
			else
			{
				UIHierarchyManager.Instance?.RegisterRoot(this);
			}
		}

		internal ComponentType[] FindNextParent<ComponentType>()
		{
			ComponentType[] array = null;
			UITransform uITransform = this;
			while ((array == null || array.Length == 0) && uITransform != null)
			{
				array = uITransform.GetComponents<ComponentType>();
				uITransform = uITransform.parent;
			}
			return array;
		}

		internal void RegisterSibling(UIIndexedComponent newSibling, UIIndexedComponent.EventReceiverFlags eventReceiverFlags)
		{
			int index = FindInsertionIndexForSibling(newSibling);
			IndexRange childrenIndexRange = ChildrenIndexRange;
			SiblingEntry item = new SiblingEntry
			{
				IndexedComponent = newSibling,
				ReceiverFlags = eventReceiverFlags
			};
			siblings.Insert(index, item);
			UpdateSiblingsFlagsUnion();
			bool flag = childrenIndexRange != ChildrenIndexRange;
			UpdateRecursively(InvalidationFlagsEnum.Siblings_OnTransformStateChange | (flag ? InvalidationFlagsEnum.Childs_ResetSortingIndex : InvalidationFlagsEnum.None), sortingRange);
		}

		internal void UnregisterSibling(UIIndexedComponent sibling)
		{
			IndexRange childrenIndexRange = ChildrenIndexRange;
			for (int i = 0; i < siblings.Count; i++)
			{
				if (siblings.Data[i].IndexedComponent == sibling)
				{
					siblings.RemoveAt(i);
					break;
				}
			}
			UpdateSiblingsFlagsUnion();
			bool flag = childrenIndexRange != ChildrenIndexRange;
			UpdateRecursively(InvalidationFlagsEnum.Siblings_OnTransformStateChange | (flag ? InvalidationFlagsEnum.Childs_ResetSortingIndex : InvalidationFlagsEnum.None), sortingRange);
		}

		internal T FindLastSibling<T>() where T : UIIndexedComponent
		{
			for (int num = siblings.Count - 1; num >= 0; num--)
			{
				T val = siblings.Data[num].IndexedComponent as T;
				if ((UnityEngine.Object)val != (UnityEngine.Object)null)
				{
					return val;
				}
			}
			return null;
		}

		internal string GetPath(UITransform root = null)
		{
			int num = base.name.IndexOf("  ");
			string text = ((num > 0) ? base.name.Substring(0, num) : base.name);
			if (parent == null)
			{
				return "/" + text;
			}
			if (parent == root)
			{
				return "./" + text;
			}
			return parent.GetPath(root) + "/" + text;
		}

		internal int GetVisibleChildrenCount()
		{
			int num = 0;
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				if (children.Data[i].visibleSelf)
				{
					num++;
				}
			}
			return num;
		}

		protected override void Load()
		{
			UIRenderingManager.Instance.LoadIfNecessary();
			base.Load();
			invalidationFlags = InvalidationFlagsEnum.Checks | InvalidationFlagsEnum.Siblings;
			CheckNanAndCorrect(ref position.x, 1f, "position.x");
			CheckNanAndCorrect(ref position.y, 1f, "position.y");
			CheckNanAndCorrect(ref position.z, 1f, "position.z");
			CheckNanAndCorrect(ref pivot.x, 1f, "pivot.x");
			CheckNanAndCorrect(ref pivot.y, 1f, "pivot.y");
			CheckNanAndCorrect(ref widthHeight.x, 1f, "width");
			CheckNanAndCorrect(ref widthHeight.y, 1f, "height");
			RegisterLoadedChildren();
			UITransform uITransform = FindParent();
			if (uITransform != null)
			{
				uITransform.LoadIfNecessary();
				uITransform.RegisterChild(this);
			}
			else
			{
				UIHierarchyManager.Instance?.RegisterRoot(this);
			}
			styleController.Bind(this);
			matrixAtomId = UIAtomContainer<Matrix4x4>.Allocate(ref localToGlobalMatrix);
		}

		protected override void Unload()
		{
			UIAtomContainer<Matrix4x4>.Deallocate(ref matrixAtomId);
			if (parent != null)
			{
				parent.UnregisterChild(this);
			}
			else
			{
				UIHierarchyManager.Instance?.UnregisterRoot(this);
			}
			styleController.Unbind();
			if (children.Count > 0)
			{
				UnregisterLoadedChildren();
			}
			sortingRange = IndexRange.Invalid;
			visibleGlobally = false;
			interactiveGlobally = false;
			layerIdentifierGlobally = -1;
			groupIndexGlobally = -1;
			ForwardToSiblingsOnTransformStateChange();
			siblings.Clear();
			siblingsFlagsUnion = UIIndexedComponent.EventReceiverFlags.None;
			invalidationFlags = InvalidationFlagsEnum.None;
			base.Unload();
		}

		[MutatorsProvider]
		private static void LoadStyles(MutatorsReceiver receiver)
		{
			receiver.Add(Mutators.X);
			receiver.Add(Mutators.Y);
			receiver.Add(Mutators.Width);
			receiver.Add(Mutators.Height);
			receiver.Add(Mutators.LeftAnchor);
			receiver.Add(Mutators.LeftAnchorPercent);
			receiver.Add(Mutators.LeftAnchorMargin);
			receiver.Add(Mutators.LeftAnchorOffset);
			receiver.Add(Mutators.RightAnchor);
			receiver.Add(Mutators.RightAnchorPercent);
			receiver.Add(Mutators.RightAnchorMargin);
			receiver.Add(Mutators.RightAnchorOffset);
			receiver.Add(Mutators.TopAnchor);
			receiver.Add(Mutators.TopAnchorPercent);
			receiver.Add(Mutators.TopAnchorMargin);
			receiver.Add(Mutators.TopAnchorOffset);
			receiver.Add(Mutators.BottomAnchor);
			receiver.Add(Mutators.BottomAnchorPercent);
			receiver.Add(Mutators.BottomAnchorMargin);
			receiver.Add(Mutators.BottomAnchorOffset);
			receiver.Add(Mutators.PivotX);
			receiver.Add(Mutators.PivotXAttached);
			receiver.Add(Mutators.PivotXPercent);
			receiver.Add(Mutators.PivotXMinMargin);
			receiver.Add(Mutators.PivotXMaxMargin);
			receiver.Add(Mutators.PivotXOffset);
			receiver.Add(Mutators.PivotY);
			receiver.Add(Mutators.PivotYAttached);
			receiver.Add(Mutators.PivotYPercent);
			receiver.Add(Mutators.PivotYMinMargin);
			receiver.Add(Mutators.PivotYMaxMargin);
			receiver.Add(Mutators.PivotYOffset);
			receiver.Add(Mutators.Z);
			receiver.Add(Mutators.ScaleX);
			receiver.Add(Mutators.ScaleY);
			receiver.Add(Mutators.ScaleZ);
			receiver.Add(Mutators.RotationX);
			receiver.Add(Mutators.RotationY);
			receiver.Add(Mutators.RotationZ);
		}

		private void RegisterChild(UITransform child)
		{
			if (children.IndexOf(child) >= 0)
			{
				Diagnostics.LogError("The child '" + child.name + "' has already been added to this.children");
				return;
			}
			int siblingIndex = child.transform.GetSiblingIndex();
			int count = children.Count;
			int num = 0;
			for (num = 0; num < count && children.Data[num].transform.GetSiblingIndex() <= siblingIndex; num++)
			{
			}
			child.parent = this;
			children.Insert(num, child);
			this.ChildrenChange?.Invoke(child, CollectionChangeAction.Add);
			UpdateRecursively(InvalidationFlagsEnum.Childs_ResetSortingIndex, sortingRange);
		}

		private void UnregisterChild(UITransform child)
		{
			int num = children.IndexOf(child);
			if (num < 0)
			{
				Diagnostics.LogError($"Could not find any child '{child}' to remove from this.children of {this}");
				return;
			}
			children.RemoveAt(num);
			child.UpdateRecursively(InvalidationFlagsEnum.None, IndexRange.Invalid);
			child.parent = null;
			this.ChildrenChange?.Invoke(child, CollectionChangeAction.Remove);
		}

		private void ReregisterChild(UITransform child)
		{
			int num = children.IndexOf(child);
			if (num >= 0)
			{
				children.RemoveAt(num);
			}
			int index = Mathf.Min(child.transform.GetSiblingIndex(), children.Count);
			children.Insert(index, child);
			this.ChildrenChange?.Invoke(child, CollectionChangeAction.Refresh);
			UpdateRecursively(InvalidationFlagsEnum.Childs_ResetSortingIndex, sortingRange);
		}

		private void RegisterLoadedChildren()
		{
			children.Clear();
			foreach (Transform item in base.transform)
			{
				UITransform component = item.GetComponent<UITransform>();
				if (component != null && component.Loaded)
				{
					UIHierarchyManager.Instance?.UnregisterRoot(component);
					RegisterChild(component);
				}
			}
		}

		private void UnregisterLoadedChildren()
		{
			foreach (Transform item in base.transform)
			{
				UITransform component = item.GetComponent<UITransform>();
				if (component != null && component.Loaded)
				{
					UnregisterChild(component);
					UIHierarchyManager.Instance?.RegisterRoot(component);
				}
			}
		}

		private void UpdateRecursively(InvalidationFlagsEnum additionalFlags, IndexRange sortingRange)
		{
			invalidationFlags |= additionalFlags;
			_ = this.sortingRange.IsValid;
			if (this.sortingRange != sortingRange)
			{
				this.sortingRange = sortingRange;
				invalidationFlags |= InvalidationFlagsEnum.Checks | InvalidationFlagsEnum.Siblings_OnTransformStateChange | InvalidationFlagsEnum.Childs_ResetSortingIndex;
			}
			else if (!sortingRange.IsValid)
			{
				invalidationFlags = InvalidationFlagsEnum.None;
				return;
			}
			if ((invalidationFlags & InvalidationFlagsEnum.AnchorCheck) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.AnchorCheck;
				if (this.sortingRange.IsValid && HasAnyAnchor)
				{
					RecomputeLocalPosAndSizeWithAnchoring();
				}
			}
			if ((invalidationFlags & InvalidationFlagsEnum.LocalRectCheck) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.LocalRectCheck;
				if (this.sortingRange.IsValid)
				{
					Vector2 vector = default(Vector2);
					vector.x = (0f - pivot.x) * widthHeight.x;
					vector.y = (0f - pivot.y) * widthHeight.y;
					Rect rect = new Rect(vector, widthHeight);
					if (rect != lastLocalRectChangeEvent)
					{
						bool flag = rect.position != lastLocalRectChangeEvent.position;
						bool flag2 = rect.size != lastLocalRectChangeEvent.size;
						lastLocalRectChangeEvent = rect;
						invalidationFlags |= (InvalidationFlagsEnum)(0x200 | (flag ? 2 : 0) | (flag ? 128 : 0) | (flag2 ? 256 : 0) | (flag2 ? 16384 : 0) | (flag ? 8192 : 0) | (flag2 ? 2097152 : 0) | (flag ? 1048576 : 0));
					}
				}
			}
			if ((invalidationFlags & InvalidationFlagsEnum.LocalToGlobalMatrixCheck) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.LocalToGlobalMatrixCheck;
				if (this.sortingRange.IsValid)
				{
					LocalToGlobalMatrixCheck();
				}
			}
			if ((invalidationFlags & InvalidationFlagsEnum.VisibleCheck) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.VisibleCheck;
				bool flag3 = this.sortingRange.IsValid && visibleSelf && (!(parent != null) || parent.visibleGlobally);
				if (flag3 != visibleGlobally)
				{
					visibleGlobally = flag3;
					invalidationFlags |= InvalidationFlagsEnum.Siblings_OnTransformStateChange | InvalidationFlagsEnum.Childs_VisibleCheck | InvalidationFlagsEnum.Invoke_VisibleGloballyChange;
				}
			}
			if ((invalidationFlags & InvalidationFlagsEnum.InteractiveCheck) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.InteractiveCheck;
				bool flag4 = this.sortingRange.IsValid && interactiveSelf && (!(parent != null) || parent.interactiveGlobally);
				if (flag4 != interactiveGlobally)
				{
					interactiveGlobally = flag4;
					invalidationFlags |= InvalidationFlagsEnum.Siblings_OnTransformStateChange | InvalidationFlagsEnum.Childs_InteractiveCheck;
				}
			}
			if ((invalidationFlags & InvalidationFlagsEnum.GroupIndexCheck) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.GroupIndexCheck;
				int num = ((!this.sortingRange.IsValid) ? (-1) : ((groupIndexSelf >= 0) ? groupIndexSelf : (parent ? parent.groupIndexGlobally : 0)));
				if (num != groupIndexGlobally)
				{
					groupIndexGlobally = num;
					invalidationFlags |= InvalidationFlagsEnum.Siblings_OnTransformStateChange | InvalidationFlagsEnum.Childs_GroupIndexCheck;
				}
			}
			if ((invalidationFlags & InvalidationFlagsEnum.LayerIdentifierCheck) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.LayerIdentifierCheck;
				int num2 = ((!this.sortingRange.IsValid) ? (-1) : ((layerIdentifierSelf >= 0) ? layerIdentifierSelf : (parent ? parent.layerIdentifierGlobally : 0)));
				if (num2 != layerIdentifierGlobally)
				{
					layerIdentifierGlobally = num2;
					invalidationFlags |= InvalidationFlagsEnum.Siblings_OnTransformStateChange | InvalidationFlagsEnum.Childs_LayerIdentifierCheck;
				}
			}
			if ((invalidationFlags & InvalidationFlagsEnum.Siblings_OnTransformStateChange) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.Siblings_OnTransformStateChange;
				ForwardToSiblingsOnTransformStateChange();
			}
			if ((invalidationFlags & (InvalidationFlagsEnum.Siblings_OnTransformPositionOrSizeChange_Position | InvalidationFlagsEnum.Siblings_OnTransformPositionOrSizeChange_Size)) != 0)
			{
				bool positionChanged = (invalidationFlags & InvalidationFlagsEnum.Siblings_OnTransformPositionOrSizeChange_Position) != 0;
				bool sizeChanged = (invalidationFlags & InvalidationFlagsEnum.Siblings_OnTransformPositionOrSizeChange_Size) != 0;
				invalidationFlags &= ~(InvalidationFlagsEnum.Siblings_OnTransformPositionOrSizeChange_Position | InvalidationFlagsEnum.Siblings_OnTransformPositionOrSizeChange_Size);
				if ((siblingsFlagsUnion & UIIndexedComponent.EventReceiverFlags.OnTransformPositionOrSizeChanged) != 0)
				{
					int count = siblings.Count;
					for (int i = 0; i < count; i++)
					{
						if ((siblings.Data[i].ReceiverFlags & UIIndexedComponent.EventReceiverFlags.OnTransformPositionOrSizeChanged) != 0)
						{
							siblings.Data[i].IndexedComponent.InternalOnTransformPositionOrSizeChanged(positionChanged, sizeChanged);
						}
					}
				}
			}
			if ((invalidationFlags & InvalidationFlagsEnum.Siblings_OnTransformLocalRectChange) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.Siblings_OnTransformLocalRectChange;
				if ((siblingsFlagsUnion & UIIndexedComponent.EventReceiverFlags.OnTransformLocalRectChanged) != 0)
				{
					int count2 = siblings.Count;
					for (int j = 0; j < count2; j++)
					{
						if ((siblings.Data[j].ReceiverFlags & UIIndexedComponent.EventReceiverFlags.OnTransformLocalRectChanged) != 0)
						{
							siblings.Data[j].IndexedComponent.InternalOnTransformLocalRectChanged();
						}
					}
				}
			}
			if ((invalidationFlags & InvalidationFlagsEnum.Siblings_OnTransformGlobalPositionChange) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.Siblings_OnTransformGlobalPositionChange;
				if ((siblingsFlagsUnion & UIIndexedComponent.EventReceiverFlags.OnTransformGlobalPositionChanged) != 0)
				{
					int count3 = siblings.Count;
					for (int k = 0; k < count3; k++)
					{
						if ((siblings.Data[k].ReceiverFlags & UIIndexedComponent.EventReceiverFlags.OnTransformGlobalPositionChanged) != 0)
						{
							siblings.Data[k].IndexedComponent.InternalOnTransformGlobalPositionChanged();
						}
					}
				}
			}
			if ((invalidationFlags & InvalidationFlagsEnum.Siblings_OnTransformGlobalSizeChange) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.Siblings_OnTransformGlobalSizeChange;
				if ((siblingsFlagsUnion & UIIndexedComponent.EventReceiverFlags.OnTransformGlobalSizeChanged) != 0)
				{
					int count4 = siblings.Count;
					for (int l = 0; l < count4; l++)
					{
						if ((siblings.Data[l].ReceiverFlags & UIIndexedComponent.EventReceiverFlags.OnTransformGlobalSizeChanged) != 0)
						{
							siblings.Data[l].IndexedComponent.InternalOnTransformGlobalSizeChanged();
						}
					}
				}
			}
			if ((invalidationFlags & InvalidationFlagsEnum.Childs) != 0)
			{
				if (children.Count > 0)
				{
					ForwardToChildUpdateRecursively();
				}
				else
				{
					invalidationFlags &= ~InvalidationFlagsEnum.Childs;
				}
			}
			if ((invalidationFlags & (InvalidationFlagsEnum.Invoke_PositionOrSizeChange_Position | InvalidationFlagsEnum.Invoke_PositionOrSizeChange_Size)) != 0)
			{
				bool arg = (invalidationFlags & InvalidationFlagsEnum.Invoke_PositionOrSizeChange_Position) != 0;
				bool arg2 = (invalidationFlags & InvalidationFlagsEnum.Invoke_PositionOrSizeChange_Size) != 0;
				invalidationFlags &= ~(InvalidationFlagsEnum.Invoke_PositionOrSizeChange_Position | InvalidationFlagsEnum.Invoke_PositionOrSizeChange_Size);
				this.PositionOrSizeChange?.Invoke(arg, arg2);
			}
			if ((invalidationFlags & (InvalidationFlagsEnum.Invoke_GlobalPositionOrSizeChange_Position | InvalidationFlagsEnum.Invoke_GlobalPositionOrSizeChange_Size)) != 0)
			{
				bool arg3 = (invalidationFlags & InvalidationFlagsEnum.Invoke_GlobalPositionOrSizeChange_Position) != 0;
				bool arg4 = (invalidationFlags & InvalidationFlagsEnum.Invoke_GlobalPositionOrSizeChange_Size) != 0;
				invalidationFlags &= ~(InvalidationFlagsEnum.Invoke_GlobalPositionOrSizeChange_Position | InvalidationFlagsEnum.Invoke_GlobalPositionOrSizeChange_Size);
				this.GlobalPositionOrSizeChange?.Invoke(arg3, arg4);
			}
			if ((invalidationFlags & InvalidationFlagsEnum.Invoke_VisibleGloballyChange) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.Invoke_VisibleGloballyChange;
				this.VisibleGloballyChange?.Invoke(visibleGlobally);
			}
			if ((invalidationFlags & InvalidationFlagsEnum.Invoke_OnPivotChange) != 0)
			{
				invalidationFlags &= ~InvalidationFlagsEnum.Invoke_OnPivotChange;
				this.PivotChange?.Invoke();
			}
		}

		private bool LocalToGlobalMatrixCheck(bool apply = true)
		{
			Matrix4x4 matrix4x;
			MatrixFlags matrixFlags;
			if (!sortingRange.IsValid)
			{
				matrix4x = Matrix4x4.identity;
				matrixFlags = (MatrixFlags)0;
			}
			else if (parent != null)
			{
				bool flag = rotation.x != 0f || rotation.y != 0f || rotation.z != 0f;
				bool flag2 = scale.x != 1f || scale.y != 1f || scale.z != 1f;
				if (flag || flag2)
				{
					Quaternion q = Quaternion.Euler(rotation);
					Vector3 pos = position;
					Vector2 vector = Vector2.Scale(parent.pivot, parent.widthHeight);
					pos.x -= vector.x;
					pos.y -= vector.y;
					MatrixFlags matrixFlags2 = (flag ? MatrixFlags.HasRotation : ((MatrixFlags)0)) | (flag2 ? MatrixFlags.HasScale : ((MatrixFlags)0));
					matrixFlags = parent.localToGlobalMatrixFlags | matrixFlags2;
					Matrix4x4 matrix4x2 = Matrix4x4.TRS(pos, q, scale);
					matrix4x = parent.localToGlobalMatrix * matrix4x2;
				}
				else
				{
					Vector2 vector2 = default(Vector2);
					vector2.x = parent.pivot.x * parent.widthHeight.x;
					vector2.y = parent.pivot.y * parent.widthHeight.y;
					Vector3 point = default(Vector3);
					point.x = position.x - vector2.x;
					point.y = position.y - vector2.y;
					point.z = position.z;
					matrix4x = parent.localToGlobalMatrix;
					matrixFlags = parent.localToGlobalMatrixFlags;
					switch (matrixFlags)
					{
					case (MatrixFlags)0:
						matrix4x.m03 += point.x;
						matrix4x.m13 += point.y;
						matrix4x.m23 += point.z;
						break;
					case MatrixFlags.HasScale:
						matrix4x.m03 += point.x * matrix4x.m00;
						matrix4x.m13 += point.y * matrix4x.m11;
						matrix4x.m23 += point.z * matrix4x.m22;
						break;
					default:
					{
						Vector3 vector3 = matrix4x.MultiplyPoint(point);
						matrix4x.m03 = vector3.x;
						matrix4x.m13 = vector3.y;
						matrix4x.m23 = vector3.z;
						break;
					}
					}
				}
			}
			else
			{
				matrix4x = Matrix4x4.identity;
				matrixFlags = (MatrixFlags)0;
			}
			bool flag3 = true;
			flag3 = localToGlobalMatrixFlags == matrixFlags && ((localToGlobalMatrixFlags == (MatrixFlags)0) ? (localToGlobalMatrix.m03 == matrix4x.m03 && localToGlobalMatrix.m13 == matrix4x.m13 && localToGlobalMatrix.m23 == matrix4x.m23) : ((localToGlobalMatrixFlags != MatrixFlags.HasScale) ? (localToGlobalMatrix == matrix4x) : (localToGlobalMatrix.m03 == matrix4x.m03 && localToGlobalMatrix.m13 == matrix4x.m13 && localToGlobalMatrix.m23 == matrix4x.m23 && localToGlobalMatrix.m00 == matrix4x.m00 && localToGlobalMatrix.m11 == matrix4x.m11 && localToGlobalMatrix.m22 == matrix4x.m22)));
			if (!flag3 && apply)
			{
				localToGlobalMatrix = matrix4x;
				localToGlobalMatrixFlags = matrixFlags;
				if (matrixAtomId.IsValid)
				{
					UIAtomContainer<Matrix4x4>.SetData(ref matrixAtomId, ref localToGlobalMatrix);
				}
				invalidationFlags |= InvalidationFlagsEnum.LocalToGlobalMatrixChange;
			}
			return flag3;
		}

		private void ForwardToSiblingsOnTransformStateChange()
		{
			int count = siblings.Count;
			for (int i = 0; i < count; i++)
			{
				IndexRange indexRange = ((!SortingRange.IsValid) ? IndexRange.Invalid : new IndexRange(sortingRange.Min + 1 + i, sortingRange.Max - i - 1));
				siblings.Data[i].IndexedComponent?.OnTransformStateChanged(indexRange, visibleGlobally, interactiveGlobally, layerIdentifierGlobally, groupIndexGlobally);
			}
		}

		private void ForwardToChildUpdateRecursively()
		{
			bool flag = (invalidationFlags & InvalidationFlagsEnum.Childs_LocalToGlobalMatrixCheck) != 0;
			bool flag2 = (invalidationFlags & InvalidationFlagsEnum.Childs_AnchorCheck) != 0;
			bool flag3 = (invalidationFlags & InvalidationFlagsEnum.Childs_LayerIdentifierCheck) != 0;
			bool flag4 = (invalidationFlags & InvalidationFlagsEnum.Childs_GroupIndexCheck) != 0;
			bool flag5 = (invalidationFlags & InvalidationFlagsEnum.Childs_VisibleCheck) != 0;
			bool flag6 = (invalidationFlags & InvalidationFlagsEnum.Childs_InteractiveCheck) != 0;
			bool num = (invalidationFlags & InvalidationFlagsEnum.Childs_ResetSortingIndex) != 0;
			InvalidationFlagsEnum additionalFlags = (flag ? InvalidationFlagsEnum.LocalToGlobalMatrixCheck : InvalidationFlagsEnum.None) | (flag2 ? InvalidationFlagsEnum.AnchorCheck : InvalidationFlagsEnum.None) | (flag3 ? InvalidationFlagsEnum.LayerIdentifierCheck : InvalidationFlagsEnum.None) | (flag4 ? InvalidationFlagsEnum.GroupIndexCheck : InvalidationFlagsEnum.None) | (flag5 ? InvalidationFlagsEnum.VisibleCheck : InvalidationFlagsEnum.None) | (flag6 ? InvalidationFlagsEnum.InteractiveCheck : InvalidationFlagsEnum.None);
			invalidationFlags &= ~InvalidationFlagsEnum.Childs;
			int count = children.Count;
			if (num)
			{
				if (count <= 0)
				{
					return;
				}
				if (sortingRange != IndexRange.Invalid)
				{
					int childCount = base.transform.childCount;
					IndexRange childrenIndexRange = ChildrenIndexRange;
					IndexRange indexRange = default(IndexRange);
					int num2 = 2;
					while (num2 < count && num2 < childCount)
					{
						num2 *= 2;
					}
					long num3 = childrenIndexRange.Extent / num2;
					for (int i = 0; i < count; i++)
					{
						indexRange.Min = childrenIndexRange.Min + num3 * i;
						indexRange.Max = indexRange.Min + num3;
						children.Data[i].UpdateRecursively(additionalFlags, indexRange);
					}
				}
				else
				{
					for (int j = 0; j < count; j++)
					{
						children.Data[j].UpdateRecursively(additionalFlags, IndexRange.Invalid);
					}
				}
			}
			else
			{
				for (int k = 0; k < count; k++)
				{
					children.Data[k].UpdateRecursively(additionalFlags, children.Data[k].sortingRange);
				}
			}
		}

		private int FindInsertionIndexForSibling(UIIndexedComponent newSibling)
		{
			int count = siblings.Count;
			if (count == 0)
			{
				return 0;
			}
			int num = 0;
			UIIndexedComponent[] components = GetComponents<UIIndexedComponent>();
			int num2 = components.Length;
			for (int i = 0; i < num2; i++)
			{
				UIIndexedComponent uIIndexedComponent = components[i];
				if (uIIndexedComponent == newSibling)
				{
					return num;
				}
				if (uIIndexedComponent == siblings.Data[num].IndexedComponent)
				{
					num++;
					if (num == count)
					{
						return num;
					}
				}
			}
			return -1;
		}

		private UITransform FindParent()
		{
			Transform transform = base.transform;
			Transform transform2 = ((transform != null) ? transform.parent : null);
			if (transform2 == null)
			{
				return null;
			}
			return transform2.GetComponent<UITransform>();
		}

		private void OnAnchoringChanged()
		{
			if (HasAnyAnchor)
			{
				UpdateRecursively(InvalidationFlagsEnum.AnchorCheck, sortingRange);
			}
		}

		private void OnPositionChanged(Vector3 previousPosition, Vector3 position)
		{
			UpdateRecursively(InvalidationFlagsEnum.LocalPosChange | InvalidationFlagsEnum.LocalRectCheck, sortingRange);
		}

		private void OnWidthHeightChanged(Vector2 previousWidthHeight, Vector2 widthHeight)
		{
			UpdateRecursively(InvalidationFlagsEnum.LocalRectChange, sortingRange);
		}

		private void OnRotationChanged(Vector3 previousRotation, Vector3 rotation)
		{
			UpdateRecursively(InvalidationFlagsEnum.LocalRotationChange, sortingRange);
		}

		private void OnScaleChanged(Vector3 previousScale, Vector3 scale)
		{
			UpdateRecursively(InvalidationFlagsEnum.LocalScaleChange, sortingRange);
		}

		private void OnPivotChanged(Vector2 previousPivot, Vector2 pivot)
		{
			Vector2 vector = Vector2.Scale(pivot - previousPivot, widthHeight);
			position.x += vector.x;
			position.y += vector.y;
			UpdateRecursively(InvalidationFlagsEnum.LocalPosChange | InvalidationFlagsEnum.LocalRectCheck | InvalidationFlagsEnum.Invoke_OnPivotChange, sortingRange);
		}

		private bool RecomputeLocalPosAndSizeWithAnchoring(bool apply = true)
		{
			Vector2 vector = ((parent != null) ? parent.widthHeight : UIHierarchyManager.Instance.StandardizedWidthHeight);
			Rect newPosition = Rect;
			ComputeHorizontalAnchoring(vector.x, ref newPosition);
			ComputeVerticalAnchoring(vector.y, ref newPosition);
			Vector2 vector2 = new Vector2(newPosition.width, newPosition.height);
			Vector3 vector3 = new Vector3(newPosition.x + pivot.x * vector2.x, newPosition.y + pivot.y * vector2.y, position.z);
			bool flag = widthHeight != vector2;
			bool flag2 = position != vector3;
			bool flag3 = flag || flag2;
			if (apply && flag3)
			{
				if (flag)
				{
					widthHeight = vector2;
				}
				if (flag2)
				{
					position = vector3;
				}
				InvalidationFlagsEnum invalidationFlagsEnum = (flag ? InvalidationFlagsEnum.LocalRectCheck : InvalidationFlagsEnum.None) | (flag2 ? InvalidationFlagsEnum.LocalPosChange : InvalidationFlagsEnum.None);
				invalidationFlags |= invalidationFlagsEnum & ~InvalidationFlagsEnum.AnchorCheck;
			}
			return !flag3;
		}

		private void ComputeHorizontalAnchoring(float parentWidth, ref Rect newPosition)
		{
			if (!leftAnchor.Attach && !rightAnchor.Attach && !pivotXAnchor.Attach)
			{
				return;
			}
			if (pivotXAnchor.Attach)
			{
				float num = parentWidth - pivotXAnchor.MinMargin - pivotXAnchor.MaxMargin;
				float num2 = pivotXAnchor.MinMargin + num * pivotXAnchor.Percent + pivotXAnchor.Offset;
				if (leftAnchor.Attach && leftAnchor.Percent <= pivotXAnchor.Percent)
				{
					float num3 = leftAnchor.Margin + (parentWidth - leftAnchor.Margin) * leftAnchor.Percent + leftAnchor.Offset;
					newPosition.width = (num2 - num3) * 2f;
				}
				else if (rightAnchor.Attach && rightAnchor.Percent >= pivotXAnchor.Percent)
				{
					float num4 = (parentWidth - rightAnchor.Margin) * rightAnchor.Percent - rightAnchor.Offset;
					newPosition.width = (num4 - num2) * 2f;
				}
				newPosition.x = num2 - newPosition.width * pivot.x;
			}
			else if (leftAnchor.Attach && !rightAnchor.Attach)
			{
				newPosition.x = leftAnchor.Margin + (parentWidth - leftAnchor.Margin) * leftAnchor.Percent + leftAnchor.Offset;
			}
			else if (!leftAnchor.Attach && rightAnchor.Attach)
			{
				newPosition.x = (parentWidth - rightAnchor.Margin) * rightAnchor.Percent - Width - rightAnchor.Offset;
			}
			else if (leftAnchor.Attach && rightAnchor.Attach && leftAnchor.Percent <= rightAnchor.Percent)
			{
				newPosition.x = leftAnchor.Margin + (parentWidth - leftAnchor.Margin - rightAnchor.Margin) * leftAnchor.Percent + leftAnchor.Offset;
				newPosition.width = (parentWidth - leftAnchor.Margin - rightAnchor.Margin) * (rightAnchor.Percent - leftAnchor.Percent) - rightAnchor.Offset - leftAnchor.Offset;
			}
		}

		private void ComputeVerticalAnchoring(float parentHeight, ref Rect newPosition)
		{
			if (!topAnchor.Attach && !bottomAnchor.Attach && !pivotYAnchor.Attach)
			{
				return;
			}
			if (pivotYAnchor.Attach)
			{
				float num = parentHeight - pivotYAnchor.MinMargin - pivotYAnchor.MaxMargin;
				float num2 = pivotYAnchor.MinMargin + num * pivotYAnchor.Percent + pivotYAnchor.Offset;
				if (topAnchor.Attach && topAnchor.Percent <= pivotYAnchor.Percent)
				{
					float num3 = topAnchor.Margin + (parentHeight - topAnchor.Margin) * topAnchor.Percent + topAnchor.Offset;
					newPosition.height = (num2 - num3) * 2f;
				}
				else if (bottomAnchor.Attach && bottomAnchor.Percent >= pivotYAnchor.Percent)
				{
					float num4 = (parentHeight - bottomAnchor.Margin) * bottomAnchor.Percent - bottomAnchor.Offset;
					newPosition.height = (num4 - num2) * 2f;
				}
				newPosition.y = num2 - newPosition.height * pivot.y;
			}
			else if (topAnchor.Attach && !bottomAnchor.Attach)
			{
				newPosition.y = topAnchor.Margin + (parentHeight - topAnchor.Margin) * topAnchor.Percent + topAnchor.Offset;
			}
			else if (!topAnchor.Attach && bottomAnchor.Attach)
			{
				newPosition.y = (parentHeight - bottomAnchor.Margin) * bottomAnchor.Percent - Height - bottomAnchor.Offset;
			}
			else if (topAnchor.Attach && bottomAnchor.Attach && topAnchor.Percent <= bottomAnchor.Percent)
			{
				newPosition.y = topAnchor.Margin + (parentHeight - topAnchor.Margin - bottomAnchor.Margin) * topAnchor.Percent + topAnchor.Offset;
				newPosition.height = (parentHeight - topAnchor.Margin - bottomAnchor.Margin) * (bottomAnchor.Percent - topAnchor.Percent) - bottomAnchor.Offset - topAnchor.Offset;
			}
		}

		private UITransform FindLastVisibleChild()
		{
			for (int num = children.Count - 1; num >= 0; num--)
			{
				if (children.Data[num].VisibleSelf)
				{
					return children.Data[num];
				}
			}
			return null;
		}

		private UITransform FindLeftmostVisibleChild()
		{
			UITransform uITransform = null;
			for (int i = 0; i < children.Count; i++)
			{
				UITransform uITransform2 = children.Data[i];
				if (uITransform2.VisibleSelf && (uITransform == null || uITransform2.GlobalRect.xMin < uITransform.GlobalRect.xMin))
				{
					uITransform = uITransform2;
				}
			}
			return uITransform;
		}

		private UITransform FindRightmostVisibleChild()
		{
			UITransform uITransform = null;
			for (int i = 0; i < children.Count; i++)
			{
				UITransform uITransform2 = children.Data[i];
				if (uITransform2.VisibleSelf && (uITransform == null || uITransform2.GlobalRect.xMax > uITransform.GlobalRect.xMax))
				{
					uITransform = uITransform2;
				}
			}
			return uITransform;
		}

		private UITransform FindTopmostVisibleChild()
		{
			UITransform uITransform = null;
			for (int i = 0; i < children.Count; i++)
			{
				UITransform uITransform2 = children.Data[i];
				if (uITransform2.VisibleSelf && (uITransform == null || uITransform2.GlobalRect.yMin < uITransform.GlobalRect.yMin))
				{
					uITransform = uITransform2;
				}
			}
			return uITransform;
		}

		private UITransform FindBottommostVisibleChild()
		{
			UITransform uITransform = null;
			for (int i = 0; i < children.Count; i++)
			{
				UITransform uITransform2 = children.Data[i];
				if (uITransform2.VisibleSelf && (uITransform == null || uITransform2.GlobalRect.yMax > uITransform.GlobalRect.yMax))
				{
					uITransform = uITransform2;
				}
			}
			return uITransform;
		}

		private void CheckNanAndCorrect(ref float value, float defaultValue, string propertyName)
		{
			if (float.IsNaN(value))
			{
				string message = $"In '{this}' property '{propertyName}' has a NaN value";
				Diagnostics.LogError(message);
				UnityEngine.Debug.LogError(message, base.transform);
				value = defaultValue;
			}
		}

		private void UpdateSiblingsFlagsUnion()
		{
			siblingsFlagsUnion = UIIndexedComponent.EventReceiverFlags.None;
			int count = siblings.Count;
			for (int i = 0; i < count; i++)
			{
				siblingsFlagsUnion |= siblings.Data[i].ReceiverFlags;
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (base.Loaded && SortingRange.IsValid)
			{
				UpdateRecursively(InvalidationFlagsEnum.Checks, sortingRange);
			}
		}

		private void OnTransformParentChanged()
		{
			if (base.Loaded)
			{
				UITransform uITransform = FindParent();
				if (!(parent == uITransform))
				{
					OnParentChanged(uITransform);
				}
			}
		}

		private void OnTransformChildrenChanged()
		{
			bool flag = false;
			int count = children.Count;
			int num = 0;
			foreach (Transform item in base.transform)
			{
				UITransform component = item.GetComponent<UITransform>();
				if (component == null || !component.Loaded)
				{
					continue;
				}
				if (component != children.Data[num])
				{
					for (int i = num + 1; i < count; i++)
					{
						if (children.Data[i] == component)
						{
							children.Data[i] = children.Data[num];
							children.Data[num] = component;
							flag = true;
							break;
						}
					}
				}
				num++;
			}
			if (flag)
			{
				this.ChildrenChange?.Invoke(null, CollectionChangeAction.Refresh);
				UpdateRecursively(InvalidationFlagsEnum.Childs_ResetSortingIndex, sortingRange);
			}
		}

		public override string ToString()
		{
			return GetPath();
		}

		[Conditional("UNITY_EDITOR")]
		internal void AssertChildrenOrdering()
		{
			int num = 0;
			foreach (Transform item in base.transform)
			{
				UITransform component = item.GetComponent<UITransform>();
				if (component != null && component.Loaded)
				{
					if (num >= children.Count)
					{
						break;
					}
					num++;
				}
			}
		}
	}
}
