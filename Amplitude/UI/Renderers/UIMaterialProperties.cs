using System;
using Amplitude.Graphics.Rendering;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	public class UIMaterialProperties
	{
		private static readonly StaticString ColorKey = new StaticString("Color");

		private static readonly StaticString AlphaKey = new StaticString("Alpha");

		private static readonly StaticString SaturationKey = new StaticString("Saturation");

		private Color color = Color.white;

		private MaterialPropertyBlendMode colorBlendMode;

		private float alpha = 1f;

		private MaterialPropertyBlendMode alphaBlendMode;

		private float saturation = 1f;

		private MaterialPropertyBlendMode saturationBlendMode;

		public UIMaterialProperties()
		{
		}

		public UIMaterialProperties(UIMaterialPropertiesEntry[] properties)
		{
			int num = properties.Length;
			for (int i = 0; i < num; i++)
			{
				string name = properties[i].Name;
				StaticString colorKey = ColorKey;
				if (name == colorKey.ToString())
				{
					color = properties[i].Data;
					colorBlendMode = properties[i].BlendMode;
					continue;
				}
				string name2 = properties[i].Name;
				colorKey = AlphaKey;
				if (name2 == colorKey.ToString())
				{
					alpha = properties[i].Data.x;
					alphaBlendMode = properties[i].BlendMode;
					continue;
				}
				string name3 = properties[i].Name;
				colorKey = SaturationKey;
				if (name3 == colorKey.ToString())
				{
					saturation = properties[i].Data.x;
					saturationBlendMode = properties[i].BlendMode;
				}
			}
		}

		public float GetFloat(StaticString identifier)
		{
			if (identifier == AlphaKey)
			{
				return alpha;
			}
			if (identifier == SaturationKey)
			{
				return saturation;
			}
			return 0f;
		}

		public void SetFloat(StaticString identifier, float value)
		{
			if (identifier == AlphaKey)
			{
				alpha = value;
			}
			else if (identifier == SaturationKey)
			{
				saturation = value;
			}
		}

		public Vector3 GetVector3(StaticString identifier)
		{
			if (identifier == ColorKey)
			{
				return new Vector3(color.r, color.g, color.b);
			}
			return Vector3.zero;
		}

		public void SetVector3(StaticString identifier, Vector3 value)
		{
			if (identifier == ColorKey)
			{
				color.r = value.x;
				color.g = value.y;
				color.b = value.z;
			}
		}

		public Vector4 GetVector4(StaticString identifier)
		{
			if (identifier == ColorKey)
			{
				return color;
			}
			return Vector4.zero;
		}

		public void SetVector4(StaticString identifier, Vector4 value)
		{
			if (identifier == ColorKey)
			{
				color = value;
			}
		}

		public void ApplyPropertiesFrom(UIMaterialProperties other)
		{
			switch (other.colorBlendMode)
			{
			case MaterialPropertyBlendMode.Multiply:
				color *= other.color;
				break;
			case MaterialPropertyBlendMode.Override:
				color = other.color;
				break;
			default:
				throw new NotImplementedException();
			}
			switch (other.alphaBlendMode)
			{
			case MaterialPropertyBlendMode.Multiply:
				alpha *= other.alpha;
				break;
			case MaterialPropertyBlendMode.Override:
				alpha = other.alpha;
				break;
			default:
				throw new NotImplementedException();
			}
			switch (other.saturationBlendMode)
			{
			case MaterialPropertyBlendMode.Multiply:
				saturation *= other.saturation;
				break;
			case MaterialPropertyBlendMode.Override:
				saturation = other.saturation;
				break;
			default:
				throw new NotImplementedException();
			}
		}

		public void WriteTo(ref DynamicWriteBuffer1D<float> buffer)
		{
			buffer.Write(alpha);
			buffer.Write(saturation);
			buffer.Write(color.r);
			buffer.Write(color.g);
			buffer.Write(color.b);
			buffer.Write(color.a);
		}

		public void Reset()
		{
			color = Color.white;
			colorBlendMode = MaterialPropertyBlendMode.Multiply;
			alpha = 1f;
			alphaBlendMode = MaterialPropertyBlendMode.Multiply;
			saturation = 1f;
			saturationBlendMode = MaterialPropertyBlendMode.Multiply;
		}
	}
}
