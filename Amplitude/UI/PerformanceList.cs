using System;

namespace Amplitude.UI
{
	public struct PerformanceList<T>
	{
		public T[] Data;

		public int Count;

		public int Capacity;

		public PerformanceList(int startCapacity)
		{
			Count = 0;
			Capacity = startCapacity;
			Data = new T[Capacity];
		}

		public void Clear()
		{
			Count = 0;
		}

		public void ClearReleaseMemory()
		{
			Data = null;
			Capacity = 0;
			Count = 0;
		}

		public void ClearArray()
		{
			if (Data != null)
			{
				Array.Clear(Data, 0, Data.Length);
			}
			Count = 0;
		}

		public void Add(T item)
		{
			if (Count + 1 >= Capacity)
			{
				Capacity = (Count + 1) * 2;
				Array.Resize(ref Data, Capacity);
			}
			Data[Count] = item;
			Count++;
		}

		public void Add(ref T item)
		{
			if (Count + 1 >= Capacity)
			{
				Capacity = (Count + 1) * 2;
				Array.Resize(ref Data, Capacity);
			}
			Data[Count] = item;
			Count++;
		}

		public int Add()
		{
			if (Count + 1 >= Capacity)
			{
				Capacity = (Count + 1) * 2;
				Array.Resize(ref Data, Capacity);
			}
			int count = Count;
			Data[Count] = default(T);
			Count++;
			return count;
		}

		public void Insert(int index, T item)
		{
			if (Count + 1 >= Capacity)
			{
				Capacity = (Count + 1) * 2;
				Array.Resize(ref Data, (Count + 1) * 2);
			}
			int num = Count - index;
			for (int i = 0; i < num; i++)
			{
				Data[Count - i] = Data[Count - 1 - i];
			}
			Data[index] = item;
			Count++;
		}

		public void RemoveAt(int index)
		{
			int length = Count - index - 1;
			Array.Copy(Data, index + 1, Data, index, length);
			Count--;
			Data[Count] = default(T);
		}

		public void RemoveRange(int index, int length)
		{
			int length2 = Count - index - length;
			Array.Copy(Data, index + length, Data, index, length2);
			Array.Clear(Data, Count - length, length);
			Count -= length;
		}

		public void RemoveBack()
		{
			Count--;
			Data[Count] = default(T);
		}

		public void Resize(int count)
		{
			if (Capacity < count)
			{
				Capacity = count;
				Array.Resize(ref Data, Capacity);
			}
			Count = count;
		}

		public void Reserve(int count)
		{
			if (Capacity < count)
			{
				Capacity = count;
				Array.Resize(ref Data, Capacity);
			}
		}

		public int IndexOf(T element)
		{
			if (Data == null)
			{
				return -1;
			}
			return Array.IndexOf(Data, element, 0, Count);
		}

		public int LastIndexOf(T element)
		{
			if (Data == null)
			{
				return -1;
			}
			return Array.LastIndexOf(Data, element, 0, Count);
		}
	}
}
