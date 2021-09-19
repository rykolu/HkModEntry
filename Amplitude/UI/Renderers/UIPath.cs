using System;
using System.Collections.Generic;
using Amplitude.UI.Animations;
using Amplitude.UI.Styles;
using Amplitude.UI.Styles.Scene;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[RequireComponent(typeof(UITransform))]
	[ExecuteInEditMode]
	public class UIPath : UIRenderer
	{
		public enum CommandType
		{
			MoveTo,
			LineTo,
			QuadraticBezierTo,
			CubicBezierTo
		}

		public static class Mutators
		{
			public static readonly MutatorSet<UIPath, UITexture> Texture = new MutatorSet<UIPath, UITexture>(ItemIdentifiers.Texture, (UIPath t) => t.Texture, delegate(UIPath t, UITexture value)
			{
				t.Texture = value;
			});

			public static readonly MutatorSet<UIPath, Color> Color = new MutatorSet<UIPath, Color>(ItemIdentifiers.Color, (UIPath t) => t.Color, delegate(UIPath t, Color value)
			{
				t.Color = value;
			});

			public static readonly MutatorSet<UIPath, UIGradient> Gradient = new MutatorSet<UIPath, UIGradient>(ItemIdentifiers.Gradient, (UIPath t) => t.Gradient, delegate(UIPath t, UIGradient value)
			{
				t.Gradient = value;
			});

			public static readonly MutatorSet<UIPath, float> Thickness = new MutatorSet<UIPath, float>(ItemIdentifiers.Thickness, (UIPath t) => t.Thickness, delegate(UIPath t, float value)
			{
				t.Thickness = value;
			});
		}

		[SerializeField]
		private UITexture texture;

		[SerializeField]
		private Color color = Color.white;

		[SerializeField]
		private UIGradient.Reference gradient;

		[SerializeField]
		[UIMaterialId(UIPrimitiveType.Curve)]
		private UIMaterialId material;

		[SerializeField]
		private PathControlPoint[] controlPoints;

		[SerializeField]
		[Range(0f, 10f)]
		private float thickness = 1f;

		[SerializeField]
		[Min(0f)]
		private int numberOfSegments;

		[SerializeField]
		private UIBlendType blendType;

		[NonSerialized]
		private PathBuilder pathBuilder;

		[NonSerialized]
		private PerformanceList<UIPrimitiveDrawer.CurveAtomIds> curveAtomIds;

		[NonSerialized]
		private Material resolvedMaterial;

		public UIBlendType BlendType
		{
			get
			{
				return blendType;
			}
			set
			{
				blendType = value;
				UpdateRenderParameters();
			}
		}

		public UITexture Texture
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

		public Color Color
		{
			get
			{
				return color;
			}
			set
			{
				if (color != value)
				{
					color = value;
					UpdateRenderParameters();
				}
			}
		}

		public UIGradient Gradient
		{
			get
			{
				return gradient.GetValue();
			}
			set
			{
				if (value != gradient.GetValue())
				{
					gradient.SetValue(value);
					OnGradientChanged();
				}
			}
		}

		public float Thickness
		{
			get
			{
				return thickness;
			}
			set
			{
				if (thickness != value)
				{
					thickness = value;
					UpdateRenderParameters();
				}
			}
		}

		public PathControlPoint[] ControlPoints
		{
			get
			{
				return controlPoints;
			}
			set
			{
				if (controlPoints != value)
				{
					controlPoints = value;
					Build();
				}
			}
		}

		public void Rebuild()
		{
			Build();
		}

		public override void CreateAnimationItems(IUIAnimationItemsCollection animationItemsCollection)
		{
			base.CreateAnimationItems(animationItemsCollection);
			animationItemsCollection.Add(Mutators.Color);
			animationItemsCollection.Add(Mutators.Gradient);
			animationItemsCollection.Add(Mutators.Thickness);
		}

		protected override void Load()
		{
			base.Load();
			UpdateRenderParameters();
			pathBuilder = default(PathBuilder);
			Build();
		}

		protected override void Unload()
		{
			pathBuilder.Clear();
			DeallocateCurveAtoms();
			base.Unload();
		}

		protected override void Render(UIPrimitiveDrawer drawer)
		{
			if (color.a != 0f)
			{
				int count = curveAtomIds.Count;
				for (int i = 0; i < count; i++)
				{
					drawer.Curve(ref curveAtomIds.Data[i], texture, resolvedMaterial);
				}
			}
		}

		protected override void OnPropertyChanged()
		{
			base.OnPropertyChanged();
			UpdateRenderParameters();
			_ = texture;
			if (texture.Texture != null)
			{
				texture.Unload();
				texture.RequestAsset();
			}
		}

		protected override void OnTransformVisibleGloballyChanged(bool previouslyVisible, bool currentlyVisible)
		{
			base.OnTransformVisibleGloballyChanged(previouslyVisible, currentlyVisible);
			if (currentlyVisible)
			{
				texture.RequestAsset();
			}
		}

		[MutatorsProvider]
		private static void LoadStyles(MutatorsReceiver receiver)
		{
			receiver.Add(Mutators.Texture);
			receiver.Add(Mutators.Color);
			receiver.Add(Mutators.Gradient);
			receiver.Add(Mutators.Thickness);
		}

		private void Build()
		{
			PathControlPoint[] array = controlPoints;
			int num = ((array != null) ? array.Length : 0);
			if (num > 0)
			{
				pathBuilder.Start(controlPoints[0].Position);
				PathControlPoint pathControlPoint = controlPoints[0];
				for (int i = 1; i < num; i++)
				{
					PathControlPoint pathControlPoint2 = controlPoints[i];
					if (pathControlPoint.RightHandle == Vector2.zero && pathControlPoint2.LeftHandle == Vector2.zero)
					{
						pathBuilder.AbsoluteLineTo(pathControlPoint2.Position);
						pathControlPoint = pathControlPoint2;
					}
					else if ((pathControlPoint.RightHandlePosition - pathControlPoint2.LeftHandlePosition).SqrMagnitude() < 25f)
					{
						pathBuilder.AbsoluteBezierTo(pathControlPoint2.LeftHandlePosition, pathControlPoint2.Position);
						pathControlPoint = pathControlPoint2;
					}
					else
					{
						pathBuilder.AbsoluteBezierTo(pathControlPoint.RightHandlePosition, pathControlPoint2.LeftHandlePosition, pathControlPoint2.Position);
						pathControlPoint = pathControlPoint2;
					}
				}
				pathBuilder.End();
			}
			UpdateRenderParameters();
		}

		private void DeallocateCurveAtoms()
		{
			int count = curveAtomIds.Count;
			for (int i = 0; i < count; i++)
			{
				UIAtomContainer<UIPrimitiveCurveData>.Deallocate(ref curveAtomIds.Data[i].CurveId);
				UIAtomContainer<UIPrimitiveCurveSegmentData>.Deallocate(ref curveAtomIds.Data[i].SegmentsId);
				curveAtomIds.Data[i].SegmentCount = 0;
			}
			curveAtomIds.Clear();
		}

		private void OnControlPointsChanged()
		{
			Build();
		}

		private void OnGradientChanged()
		{
			gradient.GetValue()?.InvalidateBufferContent();
			UpdateRenderParameters();
		}

		private void UpdateRenderParameters()
		{
			if (UIRenderingManager.Instance == null)
			{
				return;
			}
			texture.RequestAsset();
			StaticString x = material.Id;
			if (StaticString.IsNullOrEmpty(x))
			{
				x = UIMaterialCollection.DefaultMaterialNameId;
			}
			resolvedMaterial = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Curve, blendType, x);
			int gradientId = gradient.GetValue()?.GetAtomId().Index ?? (-1);
			DeallocateCurveAtoms();
			List<Curve> curves = pathBuilder.Curves;
			if (curves == null)
			{
				return;
			}
			_ = ref UITransform.LocalToGlobalMatrix;
			int count = curves.Count;
			curveAtomIds.Resize(count);
			UIPrimitiveCurveData value = default(UIPrimitiveCurveData);
			UIPrimitiveCurveSegmentData data = default(UIPrimitiveCurveSegmentData);
			for (int i = 0; i < count; i++)
			{
				Curve curve = curves[i];
				value.Type = curve.Type;
				value.Start = curve.Start;
				value.End = curve.End;
				value.Control0 = curve.Control0;
				value.Control1 = curve.Control1;
				value.Texcoords = new Vector4(texture.Coordinates.x, texture.Coordinates.y, texture.Coordinates.width, texture.Coordinates.height);
				value.Abscissas.x = curve.StartAbscissa;
				value.Abscissas.y = curve.EndAbscissa;
				value.TransformId = UITransform.MatrixAtomId.Index;
				value.GradientId = gradientId;
				value.Color = color;
				value.Thickness = thickness;
				UIAtomId curveId = UIAtomContainer<UIPrimitiveCurveData>.Allocate(ref value);
				curveAtomIds.Data[i].CurveId = curveId;
				data.CurveId = curveId.Index;
				float num = curve.UpperBoundLength();
				int num2 = ((numberOfSegments > 0) ? numberOfSegments : Mathf.CeilToInt(num / 5f * Mathf.Max(1f, thickness / 5f)));
				UIAtomId id = UIAtomContainer<UIPrimitiveCurveSegmentData>.Allocate(num2);
				curveAtomIds.Data[i].SegmentsId = id;
				curveAtomIds.Data[i].SegmentCount = num2;
				for (int j = 0; j < num2; j++)
				{
					float x2 = (float)j / (float)num2;
					float y = (float)(j + 1) / (float)num2;
					data.PositionOnCurve.x = x2;
					data.PositionOnCurve.y = y;
					UIAtomContainer<UIPrimitiveCurveSegmentData>.SetData(ref id, j, ref data);
				}
			}
		}
	}
}
