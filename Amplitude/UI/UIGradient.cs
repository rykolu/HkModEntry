using System;
using UnityEngine;

namespace Amplitude.UI
{
	[Serializable]
	public class UIGradient : ICloneable
	{
		public enum OrientationMode
		{
			Linear,
			Radial,
			Circular
		}

		[Serializable]
		public struct Reference
		{
			[SerializeField]
			private UIGradient[] ptr;

			public UIGradient GetValue()
			{
				if (ptr != null && ptr.Length != 0)
				{
					return ptr[0];
				}
				ptr = null;
				return null;
			}

			public void SetValue(UIGradient gradient)
			{
				if (gradient != null)
				{
					ptr = new UIGradient[1];
					ptr[0] = gradient;
				}
				else
				{
					ptr = null;
				}
			}
		}

		[SerializeField]
		private Gradient gradient = new Gradient();

		[SerializeField]
		[Range((float)Math.PI * -2f, (float)Math.PI * 2f)]
		private float lineAngle;

		[SerializeField]
		private Vector2 radialPosition = Vector2.zero;

		[SerializeField]
		private OrientationMode orientation;

		[NonSerialized]
		private uint[] bufferContent;

		[NonSerialized]
		private UIAtomId bufferAtomId = UIAtomId.Invalid;

		[NonSerialized]
		private int bufferAtomSize;

		public float LineAngle
		{
			get
			{
				return lineAngle;
			}
			set
			{
				lineAngle = value;
				InvalidateBufferContent();
			}
		}

		public Vector2 RadialPosition
		{
			get
			{
				return radialPosition;
			}
			set
			{
				radialPosition = value;
				InvalidateBufferContent();
			}
		}

		public OrientationMode Orientation
		{
			get
			{
				return orientation;
			}
			set
			{
				orientation = value;
				InvalidateBufferContent();
			}
		}

		public GradientAlphaKey[] AlphaKeys
		{
			get
			{
				return gradient.alphaKeys;
			}
			set
			{
				gradient.alphaKeys = value;
				InvalidateBufferContent();
			}
		}

		public GradientColorKey[] ColorKeys
		{
			get
			{
				return gradient.colorKeys;
			}
			set
			{
				gradient.colorKeys = value;
				InvalidateBufferContent();
			}
		}

		public object Clone()
		{
			UIGradient obj = new UIGradient
			{
				gradient = new Gradient()
			};
			GradientColorKey[] array = null;
			if (gradient.colorKeys != null)
			{
				array = new GradientColorKey[gradient.colorKeys.Length];
				Array.Copy(gradient.colorKeys, array, gradient.colorKeys.Length);
			}
			else
			{
				array = new GradientColorKey[0];
			}
			GradientAlphaKey[] array2 = null;
			if (gradient.alphaKeys != null)
			{
				array2 = new GradientAlphaKey[gradient.alphaKeys.Length];
				Array.Copy(gradient.alphaKeys, array2, gradient.alphaKeys.Length);
			}
			else
			{
				array2 = new GradientAlphaKey[0];
			}
			obj.gradient.SetKeys(array, array2);
			obj.gradient.mode = gradient.mode;
			obj.lineAngle = lineAngle;
			obj.radialPosition = radialPosition;
			obj.orientation = orientation;
			obj.InvalidateBufferContent();
			return obj;
		}

		public void InvalidateBufferContent()
		{
			bufferContent = null;
			if (bufferAtomId.IsValid)
			{
				GetOrCreateBufferContent();
				if (bufferAtomSize != bufferContent.Length)
				{
					UIAtomContainer<uint>.Deallocate(ref bufferAtomId);
					bufferAtomSize = bufferContent.Length;
					bufferAtomId = UIAtomContainer<uint>.Allocate(bufferAtomSize);
				}
				UIAtomContainer<uint>.SetData(ref bufferAtomId, bufferContent);
			}
		}

		public UIAtomId GetAtomId()
		{
			bool flag = bufferContent == null;
			GetOrCreateBufferContent();
			if (bufferAtomSize != bufferContent.Length)
			{
				if (bufferAtomId.IsValid)
				{
					UIAtomContainer<uint>.Deallocate(ref bufferAtomId);
				}
				bufferAtomSize = bufferContent.Length;
				bufferAtomId = UIAtomContainer<uint>.Allocate(bufferAtomSize);
				flag = true;
			}
			if (flag)
			{
				UIAtomContainer<uint>.SetData(ref bufferAtomId, bufferContent);
			}
			return bufferAtomId;
		}

		private uint[] GetOrCreateBufferContent()
		{
			if (bufferContent == null)
			{
				uint num = (uint)gradient.alphaKeys.Length;
				uint num2 = (uint)gradient.colorKeys.Length;
				uint value = 5 + AlignTo4(num * 4) + AlignTo4(num2 * 4);
				bufferContent = new uint[AlignTo4(value) >> 2];
				bufferContent[0] = 0u;
				switch (orientation)
				{
				case OrientationMode.Linear:
				{
					uint num6 = (uint)(Mathf.Clamp01(lineAngle / ((float)Math.PI * 2f)) * 1.07374182E+09f) & 0x3FFFFFFFu;
					bufferContent[0] = num6;
					break;
				}
				case OrientationMode.Radial:
				{
					uint num4 = (uint)(Mathf.Clamp01(radialPosition.x * 0.5f + 0.5f) * 32767f);
					uint num5 = (uint)(Mathf.Clamp01(radialPosition.y * 0.5f + 0.5f) * 32767f);
					bufferContent[0] = 0x40000000u | (num4 << 15) | num5;
					break;
				}
				case OrientationMode.Circular:
				{
					uint num3 = (uint)(Mathf.Clamp01(lineAngle / ((float)Math.PI * 2f)) * 1.07374182E+09f) & 0x3FFFFFFFu;
					bufferContent[0] = 0x80000000u | num3;
					break;
				}
				}
				bufferContent[1] = (num2 << 16) | num;
				uint num7 = 2u;
				for (int i = 0; i < num2; i++)
				{
					Color32 color = gradient.colorKeys[i].color;
					uint num8 = EncodeFloat01To8bits(gradient.colorKeys[i].time);
					bufferContent[num7 + i] = (num8 << 24) | (uint)(color.b << 16) | (uint)(color.g << 8) | color.r;
				}
				num7 += num2;
				for (int j = 0; j < num; j++)
				{
					GradientAlphaKey gradientAlphaKey = gradient.alphaKeys[j];
					uint num9 = EncodeFloat01To16bits(gradientAlphaKey.alpha);
					uint num10 = EncodeFloat01To16bits(gradientAlphaKey.time);
					bufferContent[num7 + j] = (num9 << 16) | num10;
				}
			}
			return bufferContent;
		}

		private uint EncodeFloat01To8bits(float value)
		{
			return (uint)(value * 255f);
		}

		private uint EncodeFloat01To16bits(float value)
		{
			return (uint)(value * 65535f);
		}

		private uint AlignTo4(uint value)
		{
			return (value + 3) & 0xFFFFFFFCu;
		}
	}
}
