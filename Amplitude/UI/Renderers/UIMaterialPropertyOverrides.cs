using System;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[Serializable]
	public struct UIMaterialPropertyOverrides
	{
		[SerializeField]
		private UIMaterialPropertyOverride[] items;

		[NonSerialized]
		private UIAtomId bufferAtomId;

		[NonSerialized]
		private int bufferAtomSize;

		[NonSerialized]
		private uint[] bufferContent;

		[NonSerialized]
		private PerformanceList<UIMaterialPropertyOverride> usedItems;

		[NonSerialized]
		private MaterialPropertyFieldInfo[] usedMaterialPropertiesInfo;

		public UIMaterialPropertyOverride[] Items => items;

		public bool Empty
		{
			get
			{
				if (items != null)
				{
					return items.Length == 0;
				}
				return true;
			}
		}

		public void OnLoad()
		{
			if (items != null && items.Length == 0)
			{
				items = null;
			}
			bufferAtomId = UIAtomId.Invalid;
			bufferAtomSize = 0;
			ResetBufferContent();
		}

		public void OnUnload()
		{
			if (bufferAtomId.IsValid)
			{
				UIAtomContainer<uint>.Deallocate(ref bufferAtomId);
			}
			bufferAtomSize = 0;
			bufferContent = null;
			usedItems.ClearReleaseMemory();
			usedMaterialPropertiesInfo = null;
		}

		public void SetFloatValue(int index, float value)
		{
			items[index].Vector = new Vector4(value, value, value, value);
			OnValidate();
		}

		public float GetFloatValue(int index)
		{
			return items[index].Vector.x;
		}

		public void SetFloatValue(string name, float value)
		{
			int num = ((items != null) ? items.Length : 0);
			for (int i = 0; i < num; i++)
			{
				if (items[i].Name == name)
				{
					items[i].Vector = new Vector4(value, value, value, value);
					OnValidate();
					return;
				}
			}
			Array.Resize(ref items, num + 1);
			items[num].Name = name;
			items[num].Type = UIMaterialPropertyOverride.PropertyType.Float;
			items[num].Vector = new Vector4(value, value, value, value);
			OnValidate();
		}

		public float GetFloatValue(string name)
		{
			int num = ((items != null) ? items.Length : 0);
			for (int i = 0; i < num; i++)
			{
				if (items[i].Name == name)
				{
					return items[i].Vector.x;
				}
			}
			Diagnostics.LogWarning("Property '" + name + "' not found.");
			return 0f;
		}

		public void SetVectorValue(int index, Vector4 value)
		{
			items[index].Vector = value;
			OnValidate();
		}

		public Vector4 GetVectorValue(int index)
		{
			return items[index].Vector;
		}

		public void SetVectorValue(string name, Vector4 value)
		{
			int num = ((items != null) ? items.Length : 0);
			for (int i = 0; i < num; i++)
			{
				if (items[i].Name == name)
				{
					items[i].Vector = value;
					OnValidate();
					return;
				}
			}
			Array.Resize(ref items, num + 1);
			items[num].Name = name;
			items[num].Type = UIMaterialPropertyOverride.PropertyType.Vector;
			items[num].Vector = value;
			OnValidate();
		}

		public Vector4 GetVectorValue(string name)
		{
			int num = ((items != null) ? items.Length : 0);
			for (int i = 0; i < num; i++)
			{
				if (items[i].Name == name)
				{
					return items[i].Vector;
				}
			}
			Diagnostics.LogWarning("Property '" + name + "' not found.");
			return Vector4.zero;
		}

		public void SetColorValue(int index, Color value)
		{
			items[index].Vector = value;
			OnValidate();
		}

		public Color GetColorValue(int index)
		{
			return items[index].Vector;
		}

		public void SetColorValue(string name, Color value)
		{
			int num = ((items != null) ? items.Length : 0);
			for (int i = 0; i < num; i++)
			{
				if (items[i].Name == name)
				{
					items[i].Vector = value;
					OnValidate();
					return;
				}
			}
			Array.Resize(ref items, num + 1);
			items[num].Name = name;
			items[num].Type = UIMaterialPropertyOverride.PropertyType.Color;
			items[num].Vector = value;
			OnValidate();
		}

		public Color GetColorValue(string name)
		{
			int num = ((items != null) ? items.Length : 0);
			for (int i = 0; i < num; i++)
			{
				if (items[i].Name == name)
				{
					return items[i].Vector;
				}
			}
			Diagnostics.LogWarning("Property '" + name + "' not found.");
			return Color.black;
		}

		public UIAtomId GetAtomId(MaterialPropertyFieldInfo[] materialPropertiesInfo)
		{
			if (usedMaterialPropertiesInfo != materialPropertiesInfo)
			{
				PopulateUsedItems(materialPropertiesInfo);
				ResetBufferContent();
				usedMaterialPropertiesInfo = materialPropertiesInfo;
			}
			GetOrCreateBufferContent();
			if (!bufferAtomId.IsValid && bufferContent != null)
			{
				bufferAtomSize = bufferContent.Length;
				bufferAtomId = UIAtomContainer<uint>.Allocate(bufferAtomSize);
				UIAtomContainer<uint>.SetData(ref bufferAtomId, bufferContent);
			}
			return bufferAtomId;
		}

		public void OnValidate()
		{
			if (bufferContent != null)
			{
				SynchronizeUsedItemValues();
				FillBufferContent();
				if (bufferAtomId.IsValid)
				{
					UIAtomContainer<uint>.SetData(ref bufferAtomId, bufferContent);
				}
			}
		}

		public void AddToMaterialPropertyBlock(MaterialPropertyBlock propertyBlock)
		{
			int num = ((items != null) ? items.Length : 0);
			for (int i = 0; i < num; i++)
			{
				switch (items[i].Type)
				{
				case UIMaterialPropertyOverride.PropertyType.Float:
					propertyBlock.SetFloat(items[i].Name, items[i].Vector.x);
					break;
				case UIMaterialPropertyOverride.PropertyType.Color:
					propertyBlock.SetColor(items[i].Name, items[i].Vector);
					break;
				case UIMaterialPropertyOverride.PropertyType.Vector:
					propertyBlock.SetVector(items[i].Name, items[i].Vector);
					break;
				}
			}
		}

		private void PopulateUsedItems(MaterialPropertyFieldInfo[] materialPropertiesInfo)
		{
			usedItems.Clear();
			int num = materialPropertiesInfo.Length;
			UIMaterialPropertyOverride item = default(UIMaterialPropertyOverride);
			for (int i = 0; i < num; i++)
			{
				string name = materialPropertiesInfo[i].Name;
				bool flag = false;
				int num2 = ((items != null) ? items.Length : 0);
				for (int j = 0; j < num2; j++)
				{
					if (items[j].Name == name)
					{
						usedItems.Add(ref items[j]);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					item.Name = materialPropertiesInfo[i].Name;
					item.Vector = materialPropertiesInfo[i].DefaultValue;
					switch (materialPropertiesInfo[i].Type)
					{
					case MaterialPropertyType.Float:
					case MaterialPropertyType.Percent:
						item.Type = UIMaterialPropertyOverride.PropertyType.Float;
						break;
					case MaterialPropertyType.Vector2:
					case MaterialPropertyType.Vector3:
					case MaterialPropertyType.Vector4:
						item.Type = UIMaterialPropertyOverride.PropertyType.Vector;
						break;
					case MaterialPropertyType.Color:
						item.Type = UIMaterialPropertyOverride.PropertyType.Color;
						break;
					default:
						throw new NotImplementedException();
					}
					usedItems.Add(ref item);
				}
			}
			if (usedItems.Count != 0)
			{
				Array.Sort(usedItems.Data, 0, usedItems.Count);
			}
		}

		private void SynchronizeUsedItemValues()
		{
			int num = items.Length;
			int count = usedItems.Count;
			for (int i = 0; i < count; i++)
			{
				for (int j = 0; j < num; j++)
				{
					if (items[j].Name == usedItems.Data[i].Name)
					{
						usedItems.Data[i].Vector = items[j].Vector;
					}
				}
			}
		}

		private void ResetBufferContent()
		{
			bufferContent = null;
			if (bufferAtomId.IsValid)
			{
				UIAtomContainer<uint>.Deallocate(ref bufferAtomId);
				bufferAtomSize = 0;
			}
		}

		private uint[] GetOrCreateBufferContent()
		{
			int count = usedItems.Count;
			if (bufferContent == null && count != 0)
			{
				int num = 0;
				for (int i = 0; i < count; i++)
				{
					switch (usedItems.Data[i].Type)
					{
					case UIMaterialPropertyOverride.PropertyType.Float:
						num++;
						break;
					case UIMaterialPropertyOverride.PropertyType.Vector:
					case UIMaterialPropertyOverride.PropertyType.Color:
						num += 4;
						break;
					}
				}
				bufferContent = new uint[num];
				FillBufferContent();
			}
			return bufferContent;
		}

		private void FillBufferContent()
		{
			int count = usedItems.Count;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				switch (usedItems.Data[i].Type)
				{
				case UIMaterialPropertyOverride.PropertyType.Float:
					bufferContent[num] = AsUint(usedItems.Data[i].Vector.x);
					num++;
					break;
				case UIMaterialPropertyOverride.PropertyType.Color:
				{
					Color linear = ((Color)usedItems.Data[i].Vector).linear;
					bufferContent[num] = AsUint(linear.r);
					bufferContent[num + 1] = AsUint(linear.g);
					bufferContent[num + 2] = AsUint(linear.b);
					bufferContent[num + 3] = AsUint(linear.a);
					num += 4;
					break;
				}
				case UIMaterialPropertyOverride.PropertyType.Vector:
					bufferContent[num] = AsUint(usedItems.Data[i].Vector.x);
					bufferContent[num + 1] = AsUint(usedItems.Data[i].Vector.y);
					bufferContent[num + 2] = AsUint(usedItems.Data[i].Vector.z);
					bufferContent[num + 3] = AsUint(usedItems.Data[i].Vector.w);
					num += 4;
					break;
				}
			}
		}

		private uint AsUint(float f)
		{
			return BitConverter.ToUInt32(BitConverter.GetBytes(f), 0);
		}
	}
}
