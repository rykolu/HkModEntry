using System;

namespace Amplitude.UI
{
	public struct UILinearAllocator<T> : IDisposable where T : struct
	{
		public enum Constructor
		{
			Default
		}

		private const int UnitializedTag = -32;

		private PerformanceList<int> pageIndexes;

		private int currentPage;

		private int currentPageIndex;

		private int nextIndexInPage;

		public int AllocatedPageCount => pageIndexes.Count;

		public UILinearAllocator(Constructor c)
		{
			pageIndexes = default(PerformanceList<int>);
			currentPage = -1;
			currentPageIndex = -32;
			nextIndexInPage = -32;
		}

		[Obsolete("Use Allocate(ref T value)")]
		public int Allocate(ref UIStorage<T> storage)
		{
			T value = new T();
			return Allocate(ref storage, ref value);
		}

		public int Allocate(ref UIStorage<T> storage, ref T value)
		{
			if (nextIndexInPage == storage.PageCapacity || nextIndexInPage == -32)
			{
				Grow(ref storage);
			}
			int num = currentPageIndex * storage.PageCapacity + nextIndexInPage;
			storage.Memory[num] = value;
			storage.Pages.Data[currentPageIndex].ComputeBufferDirty = true;
			nextIndexInPage++;
			return num;
		}

		public int Allocate(ref UIStorage<T> storage, int size)
		{
			if (nextIndexInPage + size >= storage.PageCapacity || nextIndexInPage == -32)
			{
				Grow(ref storage);
			}
			int num = currentPageIndex * storage.PageCapacity + nextIndexInPage;
			for (int i = 0; i < size; i++)
			{
				storage.Memory[num + i] = default(T);
			}
			storage.Pages.Data[currentPageIndex].ComputeBufferDirty = true;
			nextIndexInPage += size;
			return num;
		}

		public void Deallocate(ref UIStorage<T> storage, int index)
		{
		}

		public void Reset()
		{
			currentPage = -1;
			currentPageIndex = -32;
			nextIndexInPage = -32;
		}

		public void Dispose()
		{
		}

		private void Grow(ref UIStorage<T> storage)
		{
			if (currentPage + 1 == pageIndexes.Count)
			{
				int item = storage.AllocatePage(AllocatorTypeEnum.Linear);
				pageIndexes.Add(item);
			}
			currentPage++;
			currentPageIndex = pageIndexes.Data[currentPage];
			nextIndexInPage = 0;
		}
	}
}
