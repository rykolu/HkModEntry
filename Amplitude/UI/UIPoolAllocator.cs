using System;
using Unity.Collections;

namespace Amplitude.UI
{
	public struct UIPoolAllocator<T> : IDisposable where T : struct
	{
		public enum Constructor
		{
			Default
		}

		private struct PageExtension
		{
			public NativeArray<int> FreeIndexes;

			public void Initialize(int capacity)
			{
				FreeIndexes = new NativeArray<int>(capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			}

			public void Dispose()
			{
				FreeIndexes.Dispose();
			}
		}

		private const int AllocatedTag = -1;

		private const int OutOfSpaceTag = -2;

		private PerformanceList<PageExtension> pageExtensions;

		private int nextFreeIndex;

		public int AllocatedPageCount => pageExtensions.Count;

		public UIPoolAllocator(Constructor c)
		{
			nextFreeIndex = -2;
			pageExtensions = default(PerformanceList<PageExtension>);
		}

		public int Allocate(ref UIStorage<T> storage)
		{
			T value = new T();
			return Allocate(ref storage, ref value);
		}

		public int Allocate(ref UIStorage<T> storage, ref T value)
		{
			if (nextFreeIndex == -2)
			{
				Grow(ref storage);
			}
			int num = nextFreeIndex;
			int num2 = num / storage.PageCapacity;
			int index = num % storage.PageCapacity;
			int allocatorData = storage.Pages.Data[num2].AllocatorData;
			storage.Memory[num] = value;
			storage.Pages.Data[num2].ComputeBufferDirty = true;
			nextFreeIndex = pageExtensions.Data[allocatorData].FreeIndexes[index];
			pageExtensions.Data[allocatorData].FreeIndexes[index] = -1;
			return num;
		}

		public void Deallocate(ref UIStorage<T> storage, int index)
		{
			int num = index / storage.PageCapacity;
			int index2 = index % storage.PageCapacity;
			int allocatorData = storage.Pages.Data[num].AllocatorData;
			pageExtensions.Data[allocatorData].FreeIndexes[index2] = nextFreeIndex;
			nextFreeIndex = index;
		}

		public void Dispose()
		{
			int count = pageExtensions.Count;
			for (int i = 0; i < count; i++)
			{
				pageExtensions.Data[i].Dispose();
			}
		}

		private void Grow(ref UIStorage<T> storage)
		{
			int capacity = storage.Capacity;
			int num = storage.AllocatePage(AllocatorTypeEnum.Pool);
			int num2 = pageExtensions.Add();
			pageExtensions.Data[num2].Initialize(storage.PageCapacity);
			storage.Pages.Data[num].AllocatorData = num2;
			for (int i = 0; i < storage.PageCapacity - 1; i++)
			{
				pageExtensions.Data[num2].FreeIndexes[i] = capacity + i + 1;
			}
			pageExtensions.Data[num2].FreeIndexes[storage.PageCapacity - 1] = -2;
			nextFreeIndex = capacity;
		}
	}
}
