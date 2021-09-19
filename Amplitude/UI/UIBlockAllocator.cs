using System;
using Amplitude.Framework;
using UnityEngine;

namespace Amplitude.UI
{
	public struct UIBlockAllocator<T> : IDisposable where T : struct
	{
		private struct PageExtension
		{
			private const byte Free = 0;

			private const byte Splitted = 127;

			private const byte Allocated = byte.MaxValue;

			private byte[] blocks;

			private int baseAdress;

			private int pageSize;

			private int levelCount;

			public void Initialize(int baseAdress, int minBlockSize, int pageSize)
			{
				this.baseAdress = baseAdress;
				this.pageSize = pageSize;
				int num = Amplitude.Framework.Math.Log2(minBlockSize);
				int num2 = Amplitude.Framework.Math.Log2(pageSize);
				levelCount = num2 - num + 1;
				int num3 = (1 << levelCount) - 1;
				blocks = new byte[num3];
			}

			public bool Allocate(int size, out int adress)
			{
				int num = Amplitude.Framework.Math.NextLog2(size);
				int num2 = Mathf.Min(Amplitude.Framework.Math.Log2(pageSize) - num, levelCount - 1);
				int num3 = BlockCountAtLevel(num2);
				int num4 = num3 - 1;
				for (int i = 0; i < num3; i++)
				{
					int num5 = num4 + i;
					if (blocks[num5] != 0)
					{
						continue;
					}
					blocks[num5] = byte.MaxValue;
					int num6 = num2 - 1;
					int num7 = i;
					while (num6 >= 0)
					{
						int num8 = num7 / 2;
						int num9 = FirstBlockIndexAtLevel(num6) + num8;
						blocks[num9] = 127;
						num7 = num8;
						num6--;
					}
					int num10 = i;
					int j = num2 + 1;
					int num11 = 2;
					for (; j < levelCount; j++)
					{
						int num12 = FirstBlockIndexAtLevel(j) + num10 * 2;
						for (int k = 0; k < num11; k++)
						{
							blocks[num12 + k] = byte.MaxValue;
						}
						num11 <<= 1;
						num10 <<= 1;
					}
					adress = baseAdress + BlockLocalAdress(i, num2);
					return true;
				}
				adress = -1;
				return false;
			}

			public void Deallocate(int adress)
			{
				int localAdress = adress - baseAdress;
				int num = 0;
				for (int num2 = levelCount - 1; num2 > 0; num2--)
				{
					int num3 = BlockIndexAtLevel(localAdress, num2 - 1);
					if (blocks[num3] == 127)
					{
						num = num2;
						break;
					}
				}
				int num4 = BlockIndexAtLevel(localAdress, num);
				blocks[num4] = 0;
				int num5 = IndexAtLevel(localAdress, num);
				int num6 = num - 1;
				while (num6 >= 0)
				{
					int num7 = ((num5 % 2 == 0) ? (num5 + 1) : (num5 - 1));
					int num8 = FirstBlockIndexAtLevel(num6 + 1) + num7;
					if (blocks[num8] == 0)
					{
						int num9 = num5 / 2;
						int num10 = FirstBlockIndexAtLevel(num6) + num9;
						blocks[num10] = 0;
						num5 = num9;
						num6--;
						continue;
					}
					break;
				}
			}

			public void Dispose()
			{
				blocks = null;
				baseAdress = 0;
				pageSize = 0;
				levelCount = 0;
			}

			private int BlockCountAtLevel(int level)
			{
				return 1 << level;
			}

			private int FirstBlockIndexAtLevel(int level)
			{
				return (1 << level) - 1;
			}

			private int BlockSizeAtLevel(int level)
			{
				return pageSize >> level;
			}

			private int BlockIndexAtLevel(int localAdress, int level)
			{
				return FirstBlockIndexAtLevel(level) + localAdress / BlockSizeAtLevel(level);
			}

			private int BlockLocalAdress(int blockIndexAtLevel, int level)
			{
				return (pageSize >> level) * blockIndexAtLevel;
			}

			private int IndexAtLevel(int localAdress, int level)
			{
				return localAdress / BlockSizeAtLevel(level);
			}
		}

		private PerformanceList<PageExtension> pageExtensions;

		private int minBlockSize;

		private int lastAllocationPageIndex;

		public int AllocatedPageCount => pageExtensions.Count;

		public UIBlockAllocator(int minBlockSize)
		{
			this.minBlockSize = minBlockSize;
			pageExtensions = default(PerformanceList<PageExtension>);
			lastAllocationPageIndex = -1;
		}

		public int Allocate(ref UIStorage<T> storage, int size)
		{
			if (lastAllocationPageIndex != -1 && pageExtensions.Data[lastAllocationPageIndex].Allocate(size, out var adress))
			{
				return adress;
			}
			for (int num = pageExtensions.Count - 1; num >= 0; num--)
			{
				if (pageExtensions.Data[num].Allocate(size, out adress))
				{
					lastAllocationPageIndex = num;
					return adress;
				}
			}
			Grow(ref storage);
			pageExtensions.Data[pageExtensions.Count - 1].Allocate(size, out adress);
			return adress;
		}

		public void Deallocate(ref UIStorage<T> storage, int index)
		{
			int num = index / storage.PageCapacity;
			_ = index % storage.PageCapacity;
			int allocatorData = storage.Pages.Data[num].AllocatorData;
			pageExtensions.Data[allocatorData].Deallocate(index);
		}

		public void Dispose()
		{
			int count = pageExtensions.Count;
			for (int i = 0; i < count; i++)
			{
				pageExtensions.Data[i].Dispose();
			}
			minBlockSize = 0;
			pageExtensions.ClearReleaseMemory();
		}

		private void Grow(ref UIStorage<T> storage)
		{
			int capacity = storage.Capacity;
			int num = storage.AllocatePage(AllocatorTypeEnum.Block);
			int num2 = pageExtensions.Add();
			pageExtensions.Data[num2].Initialize(capacity, minBlockSize, storage.PageCapacity);
			storage.Pages.Data[num].AllocatorData = num2;
		}
	}
}
