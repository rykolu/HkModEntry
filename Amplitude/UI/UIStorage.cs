using System;

namespace Amplitude.UI
{
	public struct UIStorage<T> : IDisposable where T : struct
	{
		public struct Page
		{
			public int AllocatorData;

			public bool ComputeBufferDirty;

			public int LastSynchronisationDate;

			private AllocatorTypeEnum allocatorType;

			public AllocatorTypeEnum AllocatorType => allocatorType;

			public void Initialize(AllocatorTypeEnum allocatorType)
			{
				ComputeBufferDirty = true;
				this.allocatorType = allocatorType;
				LastSynchronisationDate = -1;
			}
		}

		public readonly int PageCapacity;

		public T[] Memory;

		public PerformanceList<Page> Pages;

		public int BufferSize
		{
			get
			{
				if (Memory == null)
				{
					return 0;
				}
				return Memory.Length;
			}
		}

		public int Capacity => Pages.Count * PageCapacity;

		public int PageCount => Pages.Count;

		public T this[int index] => Memory[index];

		public UIStorage(int pageCapacity = 64, int startingPageCount = 3)
		{
			PageCapacity = pageCapacity;
			Pages = default(PerformanceList<Page>);
			int num = startingPageCount * pageCapacity;
			Memory = new T[num];
		}

		public void Assign(int index, ref T data)
		{
			Memory[index] = data;
		}

		public void Dispose()
		{
			Pages.ClearReleaseMemory();
			Memory = null;
		}

		public int AllocatePage(AllocatorTypeEnum allocatorType)
		{
			int count = Pages.Count;
			Pages.Add(default(Page));
			int num = PageCapacity * Pages.Count;
			if (((Memory != null) ? Memory.Length : 0) < num)
			{
				Array.Resize(ref Memory, num);
			}
			Pages.Data[count].Initialize(allocatorType);
			return count;
		}
	}
}
