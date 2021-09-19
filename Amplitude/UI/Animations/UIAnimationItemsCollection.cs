using System;
using Amplitude.Extensions;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationItemsCollection : IUIAnimationItemsReadOnlyCollection, ISerializationCallbackReceiver
	{
		[Serializable]
		private struct SerializableItem
		{
			[SerializeField]
			private string type;

			[SerializeField]
			private UIAnimationItemParams parameters;

			[SerializeField]
			private float[] floatMinMax;

			[SerializeField]
			private Color[] colorMinMax;

			[SerializeField]
			private Vector3[] vector3MinMax;

			public void Serialize(IUIAnimationItemSerializable item)
			{
				if (item != null)
				{
					type = item.GetType().AssemblyQualifiedName;
					parameters = item.Parameters;
					IUIAnimationInterpolator interpolator = item.Interpolator;
					SerializeInterpolator(interpolator);
				}
			}

			public IUIAnimationItemSerializable Deserialize()
			{
				if (string.IsNullOrEmpty(type))
				{
					return null;
				}
				IUIAnimationItemSerializable iUIAnimationItemSerializable = UIAnimationItemUtils.Create(Type.GetType(type), parameters);
				if (iUIAnimationItemSerializable != null)
				{
					DeserializeInterpolator(iUIAnimationItemSerializable.Interpolator);
				}
				return iUIAnimationItemSerializable;
			}

			private void SerializeInterpolator(IUIAnimationInterpolator interpolator)
			{
				if (!TrySerializeInterpolator(interpolator, ref floatMinMax) && !TrySerializeInterpolator(interpolator, ref colorMinMax) && !TrySerializeInterpolator(interpolator, ref vector3MinMax))
				{
					Diagnostics.LogError($"Cannot serialize UIAnimationInterpolator {interpolator.GetType()}");
				}
			}

			private bool TrySerializeInterpolator<Type>(IUIAnimationInterpolator interpolator, ref Type[] minMax)
			{
				UIAnimationInterpolator<Type> uIAnimationInterpolator = interpolator as UIAnimationInterpolator<Type>;
				if (uIAnimationInterpolator != null)
				{
					minMax = new Type[2] { uIAnimationInterpolator.Min, uIAnimationInterpolator.Max };
					return true;
				}
				return false;
			}

			private void DeserializeInterpolator(IUIAnimationInterpolator interpolator)
			{
				if (!TryDesezializeInterpolator(interpolator, floatMinMax) && !TryDesezializeInterpolator(interpolator, colorMinMax) && !TryDesezializeInterpolator(interpolator, vector3MinMax))
				{
					Diagnostics.LogError($"Cannot deserialize UIAnimationInterpolator {interpolator.GetType()}");
				}
			}

			private bool TryDesezializeInterpolator<Type>(IUIAnimationInterpolator interpolator, Type[] minMax)
			{
				if (minMax != null)
				{
					UIAnimationInterpolator<Type> uIAnimationInterpolator = interpolator as UIAnimationInterpolator<Type>;
					if (uIAnimationInterpolator != null)
					{
						uIAnimationInterpolator.Min = minMax[0];
						uIAnimationInterpolator.Max = minMax[1];
						return true;
					}
				}
				return false;
			}
		}

		[NonSerialized]
		private IUIAnimationItem[] items;

		[SerializeField]
		private SerializableItem[] serializableItems;

		public int Length
		{
			get
			{
				if (items == null)
				{
					return 0;
				}
				return items.Length;
			}
		}

		public IUIAnimationItem this[int index] => items[index];

		public void Append(IUIAnimationItem item)
		{
			items = items.Append(item);
		}

		public void Insert(IUIAnimationItem item, int insertIndex)
		{
			items = items.Insert(insertIndex, item);
		}

		public void Remove(int deletedIndex, int count = 1)
		{
			items = items.RemoveAt(deletedIndex, count);
		}

		public void Swap(int index1, int index2)
		{
			IUIAnimationItem iUIAnimationItem = items[index1];
			items[index1] = items[index2];
			items[index2] = iUIAnimationItem;
		}

		public void Clear()
		{
			items = null;
			serializableItems = null;
		}

		public void Copy(IUIAnimationItemsReadOnlyCollection other)
		{
			int length = other.Length;
			serializableItems = new SerializableItem[length];
			for (int i = 0; i < other.Length; i++)
			{
				IUIAnimationItemSerializable iUIAnimationItemSerializable = other[i] as IUIAnimationItemSerializable;
				if (iUIAnimationItemSerializable != null)
				{
					serializableItems[i] = default(SerializableItem);
					serializableItems[i].Serialize(iUIAnimationItemSerializable);
				}
			}
			((ISerializationCallbackReceiver)this).OnAfterDeserialize();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (serializableItems != null)
			{
				int num = serializableItems.Length;
				items = new IUIAnimationItem[num];
				for (int i = 0; i < num; i++)
				{
					items[i] = serializableItems[i].Deserialize() as IUIAnimationItem;
				}
				serializableItems = null;
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (items == null)
			{
				return;
			}
			int length = Length;
			serializableItems = new SerializableItem[length];
			for (int i = 0; i < length; i++)
			{
				IUIAnimationItemSerializable iUIAnimationItemSerializable = this[i] as IUIAnimationItemSerializable;
				if (iUIAnimationItemSerializable != null)
				{
					serializableItems[i].Serialize(iUIAnimationItemSerializable);
				}
			}
		}
	}
}
