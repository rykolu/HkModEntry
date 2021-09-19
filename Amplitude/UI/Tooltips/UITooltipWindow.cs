using System;
using System.Collections;
using System.Collections.Generic;
using Amplitude.Framework;
using Amplitude.UI.Interactables;
using Amplitude.UI.Layouts;
using Amplitude.UI.Windows;
using UnityEngine;

namespace Amplitude.UI.Tooltips
{
	public class UITooltipWindow : UIWindow
	{
		protected struct BoundTooltipBrick
		{
			public readonly int InstanceId;

			public readonly UITooltipBrick Brick;

			public BoundTooltipBrick(int id, UITooltipBrick brick)
			{
				InstanceId = id;
				Brick = brick;
			}
		}

		private struct AnchorDescription
		{
			public static AnchorDescription Empty;

			public UITooltipAnchorMode AnchorMode;

			public WeakReference Anchor;

			public bool AnchorStickToCursor
			{
				get
				{
					if (AnchorMode != 0 && AnchorMode != UITooltipAnchorMode.Left_Free && AnchorMode != UITooltipAnchorMode.Right_Free && AnchorMode != UITooltipAnchorMode.Top_Free)
					{
						return AnchorMode == UITooltipAnchorMode.Bottom_Free;
					}
					return true;
				}
			}

			public AnchorDescription(UITooltip tooltip)
			{
				if (tooltip.Anchor == null && tooltip.AnchorMode != 0)
				{
					Diagnostics.LogWarning(57uL, "Incorrect tooltip anchor: Anchor:NULL - Mode:{0} ({1})", tooltip.AnchorMode, tooltip.ToString());
					AnchorMode = UITooltipAnchorMode.Free;
					Anchor = null;
				}
				else
				{
					AnchorMode = tooltip.AnchorMode;
					Anchor = ((tooltip.Anchor != null) ? new WeakReference(tooltip.Anchor) : null);
				}
			}
		}

		private const float DefaultDelay = 0.3f;

		private const float DefaultDelayIfTooltipShown = 0f;

		[SerializeField]
		private UITooltipClassDefinition defaultClass;

		[SerializeField]
		private UITransform tooltipBox;

		[SerializeField]
		private UITable1D bricksTable;

		[SerializeField]
		private float offset;

		private UITransform bricksTableTransform;

		private Dictionary<int, Stack<UITooltipBrick>> bricksPoolByInstanceId = new Dictionary<int, Stack<UITooltipBrick>>();

		private UITooltip currentTooltip;

		private List<BoundTooltipBrick> currentTooltipBricks = new List<BoundTooltipBrick>();

		private AnchorDescription currentAnchor;

		private UnityCoroutine delayedShowCoroutine;

		protected UITooltip CurrentTooltip => currentTooltip;

		protected List<BoundTooltipBrick> CurrentTooltipBricks => currentTooltipBricks;

		protected virtual float TooltipHeight => tooltipBox.Height;

		protected virtual float TooltipWidth => tooltipBox.Width;

		protected override void Load()
		{
			base.Load();
			bricksTableTransform = bricksTable?.GetComponent<UITransform>();
		}

		protected override void Unload()
		{
			bricksTableTransform = null;
			base.Unload();
		}

		protected override void SpecificUpdate()
		{
			base.SpecificUpdate();
			if (currentAnchor.AnchorStickToCursor)
			{
				UpdatePosition();
			}
		}

		protected override IEnumerator PostLoad()
		{
			yield return base.PostLoad();
			if (defaultClass == null)
			{
				Diagnostics.LogWarning(57uL, "No default TooltipClass specified in TooltipWindow");
			}
			IDatabase<UITooltipClassDefinition> database = Databases.GetDatabase<UITooltipClassDefinition>();
			if (database == null)
			{
				Diagnostics.LogError("Could not find the UITooltipClassDefinition database.");
				yield break;
			}
			foreach (UITooltipClassDefinition tooltipClassDefinition in database)
			{
				int bricksCount = tooltipClassDefinition.BrickDefinitionsCount;
				int i = 0;
				while (i < bricksCount)
				{
					UITooltipBrickDefinition brickDefinition = tooltipClassDefinition.GetBrickDefinition(i);
					if (brickDefinition == null || brickDefinition.Prefab == null)
					{
						Diagnostics.LogWarning(57uL, "TooltipBrick {0} in '{1}' is not properly initialized or does not exist.", i, tooltipClassDefinition.ToString());
					}
					else
					{
						PushBrick(brickDefinition, null);
						yield return null;
					}
					int num = i + 1;
					i = num;
				}
				UnbindTooltip();
			}
			UITooltipManager.Instance.TooltipUpdate += TooltipManager_TooltipUpdate;
		}

		protected override void PreUnload()
		{
			if (delayedShowCoroutine != null && !delayedShowCoroutine.IsFinished)
			{
				StopCoroutine(delayedShowCoroutine.Coroutine);
				delayedShowCoroutine = null;
			}
			if (UITooltipManager.Instance != null)
			{
				UITooltipManager.Instance.TooltipUpdate -= TooltipManager_TooltipUpdate;
			}
			foreach (KeyValuePair<int, Stack<UITooltipBrick>> item in bricksPoolByInstanceId)
			{
				item.Value.Clear();
			}
			bricksPoolByInstanceId.Clear();
			int count = currentTooltipBricks.Count;
			for (int i = 0; i < count; i++)
			{
				currentTooltipBricks[i].Brick.PreUnload();
			}
			currentTooltipBricks.Clear();
			bricksTable?.SetComparer(null);
			base.PreUnload();
		}

		protected override void OnBeginShow(bool instant)
		{
			bricksTable?.ArrangeChildren();
			tooltipBox.Height = bricksTableTransform.Height;
			UpdatePosition();
		}

		protected override void OnEndHide(bool instant)
		{
			UnbindTooltip();
			base.OnEndHide(instant);
		}

		protected virtual IEnumerator DoShowTooltipAfterDelay(UITooltip tooltip, float delay)
		{
			if (delay > 0f)
			{
				yield return Coroutine.DoWaitForSeconds(delay);
			}
			if (!(tooltip == null))
			{
				if (BindTooltip(tooltip))
				{
					UpdateBrickOrder();
					ShowTooltip(tooltip);
				}
				else
				{
					UnbindTooltip();
				}
			}
		}

		protected virtual void ShowTooltip(UITooltip tooltip)
		{
			IUIWindowsService service = Services.GetService<IUIWindowsService>();
			if (service != null)
			{
				service.ShowWindow(this);
				return;
			}
			Diagnostics.LogWarning(57uL, "No WindowsManagerService available. Showing TooltipWindow anyway but something is probably wrong in your context.");
			RequestShow(instant: false);
		}

		protected void UpdateBrickOrder()
		{
			int num = -1;
			int count = CurrentTooltipBricks.Count;
			for (int i = 0; i < count; i++)
			{
				BoundTooltipBrick boundTooltipBrick = CurrentTooltipBricks[i];
				UITooltipBrick brick = boundTooltipBrick.Brick;
				if (!(brick == null) && brick.UITransform.VisibleSelf)
				{
					int num2 = boundTooltipBrick.Brick.transform.GetSiblingIndex();
					if (num > num2)
					{
						num2 = num + 1;
						brick.transform.SetSiblingIndex(num2);
					}
					num = num2;
				}
			}
		}

		protected virtual void HideTooltip(bool instant = false)
		{
			if (delayedShowCoroutine != null && !delayedShowCoroutine.IsFinished)
			{
				StopCoroutine(delayedShowCoroutine.Coroutine);
				delayedShowCoroutine = null;
				UnbindTooltip();
				return;
			}
			IUIWindowsService service = Services.GetService<IUIWindowsService>();
			if (service != null)
			{
				service.HideWindow(this, instant);
				return;
			}
			Diagnostics.LogWarning(57uL, "No WindowsManagerService available. Hiding TooltipWindow anyway but something is probably wrong in your context.");
			RequestHide(instant);
		}

		protected bool BindTooltip(UITooltip tooltip)
		{
			currentTooltip = tooltip;
			currentAnchor = new AnchorDescription(currentTooltip);
			if (currentAnchor.Anchor != null && currentAnchor.Anchor.Target != null && !currentAnchor.AnchorStickToCursor)
			{
				(currentAnchor.Anchor.Target as UITransform).GlobalPositionOrSizeChange += Anchor_GlobalPositionOrSizeChange;
			}
			UITooltipClassDefinition tooltipClass = currentTooltip.TooltipClass;
			if (tooltipClass == null)
			{
				tooltipClass = defaultClass;
			}
			bool flag = false;
			int brickDefinitionsCount = tooltipClass.BrickDefinitionsCount;
			for (int i = 0; i < brickDefinitionsCount; i++)
			{
				UITooltipBrickDefinition brickDefinition = tooltipClass.GetBrickDefinition(i);
				if (brickDefinition == null || brickDefinition.Prefab == null)
				{
					Diagnostics.LogWarning(57uL, "TooltipBrick {0} in '{1}' is not properly initialized or does not exist.", i, tooltipClass.ToString());
				}
				else
				{
					UITooltipBrick uITooltipBrick = PushBrick(brickDefinition, currentTooltip);
					flag |= uITooltipBrick.UITransform.VisibleSelf && !uITooltipBrick.IsDecoration;
				}
			}
			return flag;
		}

		protected void UnbindTooltip()
		{
			if (currentAnchor.Anchor != null && !currentAnchor.AnchorStickToCursor)
			{
				UITransform uITransform = currentAnchor.Anchor.Target as UITransform;
				if (uITransform != null)
				{
					uITransform.GlobalPositionOrSizeChange -= Anchor_GlobalPositionOrSizeChange;
				}
			}
			foreach (BoundTooltipBrick currentTooltipBrick in currentTooltipBricks)
			{
				currentTooltipBrick.Brick.Unbind();
				currentTooltipBrick.Brick.UITransform.VisibleSelf = false;
				bricksPoolByInstanceId[currentTooltipBrick.InstanceId].Push(currentTooltipBrick.Brick);
			}
			currentTooltipBricks.Clear();
			currentAnchor = AnchorDescription.Empty;
			currentTooltip = null;
		}

		protected virtual float GetDelay(UITooltip tooltip)
		{
			return 0.3f;
		}

		protected virtual float GetDelayIfTooltipShown(UITooltip tooltip)
		{
			return 0f;
		}

		protected virtual void TooltipManager_TooltipUpdate(object sender, UITooltipEventsArg args)
		{
			if (!UITooltipManager.Instance.ShowTooltips)
			{
				HideTooltip(instant: true);
				return;
			}
			switch (args.EventType)
			{
			case UITooltipEventsArg.Type.HoveredStart:
			{
				float arg = GetDelay(args.Tooltip);
				if (base.Visibility != VisibilityState.Invisible)
				{
					HideTooltip(instant: true);
					arg = GetDelayIfTooltipShown(args.Tooltip);
				}
				if (args.Tooltip.IsBound)
				{
					delayedShowCoroutine = UnityCoroutine.StartCoroutine(this, DoShowTooltipAfterDelay, args.Tooltip, arg, OnTooltipException);
				}
				break;
			}
			case UITooltipEventsArg.Type.HoveredStop:
				HideTooltip();
				break;
			case UITooltipEventsArg.Type.Update:
				HideTooltip(instant: true);
				if (args.Tooltip.IsBound)
				{
					delayedShowCoroutine = UnityCoroutine.StartCoroutine(this, DoShowTooltipAfterDelay, args.Tooltip, GetDelayIfTooltipShown(args.Tooltip), OnTooltipException);
				}
				break;
			default:
				Diagnostics.LogError("Unknown tooltip event type '{0}'", args.EventType);
				break;
			}
		}

		private void UpdatePosition()
		{
			Vector2 mousePosition = UIInteractivityManager.Instance.GetMousePosition(UITooltipManager.Instance.TooltipView);
			UITransform uITransform = ((currentAnchor.Anchor != null && currentAnchor.Anchor.Target != null) ? (currentAnchor.Anchor.Target as UITransform) : null);
			UITooltipAnchorMode uITooltipAnchorMode = ((uITransform != null) ? currentAnchor.AnchorMode : UITooltipAnchorMode.Free);
			Vector2 vector = ComputeAnchorPosition(uITransform, uITooltipAnchorMode, mousePosition);
			Vector2 zero = Vector2.zero;
			switch (uITooltipAnchorMode)
			{
			case UITooltipAnchorMode.Free:
				zero.x = vector.x;
				zero.y = vector.y - TooltipHeight;
				break;
			case UITooltipAnchorMode.Right_Free:
			case UITooltipAnchorMode.Right_Top:
			case UITooltipAnchorMode.Right_Center:
			case UITooltipAnchorMode.Right_Bottom:
				zero.x = vector.x + offset;
				goto IL_0189;
			case UITooltipAnchorMode.Top_Left:
			case UITooltipAnchorMode.Bottom_Left:
				zero.x = vector.x;
				goto IL_0189;
			case UITooltipAnchorMode.Left_Free:
			case UITooltipAnchorMode.Left_Top:
			case UITooltipAnchorMode.Left_Center:
			case UITooltipAnchorMode.Left_Bottom:
				zero.x = vector.x - offset - TooltipWidth;
				goto IL_0189;
			case UITooltipAnchorMode.Top_Right:
			case UITooltipAnchorMode.Bottom_Right:
				zero.x = vector.x - TooltipWidth;
				goto IL_0189;
			case UITooltipAnchorMode.Top_Free:
			case UITooltipAnchorMode.Top_Center:
			case UITooltipAnchorMode.Bottom_Free:
			case UITooltipAnchorMode.Bottom_Center:
				zero.x = vector.x - TooltipWidth * 0.5f;
				goto IL_0189;
			default:
				{
					Diagnostics.LogWarning(57uL, "Unknown AnchorMode '{0}'", uITooltipAnchorMode);
					zero.x = vector.x;
					goto IL_0189;
				}
				IL_0189:
				switch (uITooltipAnchorMode)
				{
				case UITooltipAnchorMode.Top_Free:
				case UITooltipAnchorMode.Top_Left:
				case UITooltipAnchorMode.Top_Center:
				case UITooltipAnchorMode.Top_Right:
					zero.y = vector.y - offset - TooltipHeight;
					break;
				case UITooltipAnchorMode.Left_Bottom:
				case UITooltipAnchorMode.Right_Bottom:
					zero.y = vector.y - TooltipHeight;
					break;
				case UITooltipAnchorMode.Bottom_Free:
				case UITooltipAnchorMode.Bottom_Left:
				case UITooltipAnchorMode.Bottom_Center:
				case UITooltipAnchorMode.Bottom_Right:
					zero.y = vector.y + offset;
					break;
				case UITooltipAnchorMode.Left_Top:
				case UITooltipAnchorMode.Right_Top:
					zero.y = vector.y;
					break;
				case UITooltipAnchorMode.Left_Free:
				case UITooltipAnchorMode.Left_Center:
				case UITooltipAnchorMode.Right_Free:
				case UITooltipAnchorMode.Right_Center:
					zero.y = vector.y - TooltipHeight * 0.5f;
					break;
				default:
					Diagnostics.LogWarning(57uL, "Unknown AnchorMode '{0}'", uITooltipAnchorMode);
					zero.y = vector.y - TooltipHeight;
					break;
				}
				break;
			}
			tooltipBox.X = zero.x - UITransform.X;
			tooltipBox.Y = zero.y - UITransform.Y;
			tooltipBox.X = Mathf.Max(tooltipBox.X, 0f);
			tooltipBox.X = Mathf.Min(UITransform.Width - TooltipWidth, tooltipBox.X);
			tooltipBox.Y = Mathf.Max(tooltipBox.Y, 0f);
			tooltipBox.Y = Mathf.Min(UITransform.Height - TooltipHeight, tooltipBox.Y);
		}

		private Rect ComputeAnchorRectInTooltipView(UITransform anchor)
		{
			UIView uIView = UITooltipManager.Instance.GetViewForGroupIndex(anchor.GroupIndexGlobally);
			if (uIView == null)
			{
				uIView = UIHierarchyManager.Instance.MainFullscreenView;
			}
			Rect globalRect = anchor.GlobalRect;
			Matrix4x4 projectionMatrix = uIView.ProjectionMatrix;
			Matrix4x4 matrix4x = UITooltipManager.Instance.TooltipView.ProjectionMatrix.inverse * projectionMatrix;
			Vector4 vector = new Vector4(globalRect.xMin, globalRect.yMin, 0f, 1f);
			Vector4 vector2 = new Vector4(globalRect.xMax, globalRect.yMax, 0f, 1f);
			Vector2 vector3 = matrix4x * vector;
			Vector2 vector4 = matrix4x * vector2;
			return Rect.MinMaxRect(vector3.x, vector3.y, vector4.x, vector4.y);
		}

		private Vector2 ComputeAnchorPosition(UITransform anchor, UITooltipAnchorMode anchorMode, Vector2 mousePositionInTooltipView)
		{
			Vector2 zero = Vector2.zero;
			Rect rect = ((anchor != null) ? ComputeAnchorRectInTooltipView(anchor) : Rect.zero);
			switch (anchorMode)
			{
			case UITooltipAnchorMode.Top_Left:
			case UITooltipAnchorMode.Bottom_Left:
			case UITooltipAnchorMode.Left_Free:
			case UITooltipAnchorMode.Left_Top:
			case UITooltipAnchorMode.Left_Center:
			case UITooltipAnchorMode.Left_Bottom:
				zero.x = rect.xMin;
				break;
			case UITooltipAnchorMode.Top_Right:
			case UITooltipAnchorMode.Bottom_Right:
			case UITooltipAnchorMode.Right_Free:
			case UITooltipAnchorMode.Right_Top:
			case UITooltipAnchorMode.Right_Center:
			case UITooltipAnchorMode.Right_Bottom:
				zero.x = rect.xMax;
				break;
			case UITooltipAnchorMode.Top_Center:
			case UITooltipAnchorMode.Bottom_Center:
				zero.x = rect.xMin + 0.5f * rect.width;
				break;
			default:
				zero.x = mousePositionInTooltipView.x;
				break;
			}
			switch (anchorMode)
			{
			case UITooltipAnchorMode.Top_Free:
			case UITooltipAnchorMode.Top_Left:
			case UITooltipAnchorMode.Top_Center:
			case UITooltipAnchorMode.Top_Right:
			case UITooltipAnchorMode.Left_Top:
			case UITooltipAnchorMode.Right_Top:
				zero.y = rect.yMin;
				break;
			case UITooltipAnchorMode.Bottom_Free:
			case UITooltipAnchorMode.Bottom_Left:
			case UITooltipAnchorMode.Bottom_Center:
			case UITooltipAnchorMode.Bottom_Right:
			case UITooltipAnchorMode.Left_Bottom:
			case UITooltipAnchorMode.Right_Bottom:
				zero.y = rect.yMax;
				break;
			case UITooltipAnchorMode.Left_Center:
			case UITooltipAnchorMode.Right_Center:
				zero.y = rect.yMin + 0.5f * rect.height;
				break;
			default:
				zero.y = mousePositionInTooltipView.y;
				break;
			}
			return zero;
		}

		private UITooltipBrick PushBrick(UITooltipBrickDefinition brickDefinition, UITooltip tooltip)
		{
			int instanceID = brickDefinition.Prefab.GetInstanceID();
			Stack<UITooltipBrick> value = null;
			if (!bricksPoolByInstanceId.TryGetValue(instanceID, out value))
			{
				value = new Stack<UITooltipBrick>();
				bricksPoolByInstanceId.Add(instanceID, value);
			}
			UITooltipBrick uITooltipBrick = null;
			if (value.Count > 0)
			{
				uITooltipBrick = value.Pop();
			}
			else
			{
				uITooltipBrick = bricksTableTransform.InstantiateChild(brickDefinition.Prefab).GetComponent<UITooltipBrick>();
				uITooltipBrick.PostLoad();
			}
			if (uITooltipBrick != null)
			{
				currentTooltipBricks.Add(new BoundTooltipBrick(instanceID, uITooltipBrick));
				if (tooltip != null)
				{
					bool visibleSelf = uITooltipBrick.Bind(brickDefinition, tooltip.Data);
					uITooltipBrick.GetComponent<UITransform>().VisibleSelf = visibleSelf;
				}
			}
			return uITooltipBrick;
		}

		private UITooltipBrick CreateTooltipBrick(UITooltipBrickDefinition definition)
		{
			UITransform uITransform = bricksTableTransform.InstantiateChild(definition.Prefab);
			UITooltipBrick component = uITransform.GetComponent<UITooltipBrick>();
			if (component == null)
			{
				Diagnostics.LogError(57uL, "Prefab '{0}' does not contain any TooltipBrick instance.", definition.Prefab.gameObject.ToString());
				UnityEngine.Object.Destroy(uITransform.gameObject);
				uITransform = null;
				return null;
			}
			return component;
		}

		private void OnTooltipException(object sender, CoroutineExceptionEventArgs args)
		{
			Diagnostics.LogException(57uL, args.Exception);
			UnbindTooltip();
		}

		private void Anchor_GlobalPositionOrSizeChange(bool positionChanged, bool sizeChanged)
		{
			UpdatePosition();
		}
	}
}
