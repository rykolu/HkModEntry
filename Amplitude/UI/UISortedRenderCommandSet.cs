using System.Collections;

namespace Amplitude.UI
{
	public struct UISortedRenderCommandSet
	{
		private PerformanceList<UIRenderCommand> items;

		private PerformanceList<UIRenderCommand> doubleBufferItems;

		private PerformanceList<UIRenderCommand> pendingItems;

		public int Count => items.Count;

		public UISortedRenderCommandSet(bool createLists)
		{
			items = default(PerformanceList<UIRenderCommand>);
			doubleBufferItems = default(PerformanceList<UIRenderCommand>);
			pendingItems = default(PerformanceList<UIRenderCommand>);
		}

		public void CheckConsistency()
		{
		}

		public void Add(ref UIRenderCommand item)
		{
			pendingItems.Add(ref item);
		}

		public void Clear()
		{
			items.Clear();
			pendingItems.Clear();
			doubleBufferItems.Clear();
		}

		public bool Remove(long sortingIndex, int layerIndex, UIBehaviour owner)
		{
			bool flag = RemoveSorted(items.Data, items.Count, sortingIndex, layerIndex, owner);
			if (!flag)
			{
				flag = Remove(pendingItems.Data, pendingItems.Count, sortingIndex, layerIndex, owner);
			}
			return flag;
		}

		public void Render(UIPrimitiveDrawer primitiveDrawer)
		{
			if (pendingItems.Count > 0)
			{
				AllocFreeSorter.Sort(pendingItems.Data, pendingItems.Count, CompareRenderCommand);
				int count = pendingItems.Count;
				int count2 = items.Count;
				int num = 0;
				int num2 = 0;
				while (num2 < count2 || num < count)
				{
					long num3 = ((num2 < count2) ? items.Data[num2].SortingIndex : long.MaxValue);
					long num4 = ((num2 < count2) ? items.Data[num2].LayerIndex : long.MaxValue);
					long num5 = ((num < count) ? pendingItems.Data[num].SortingIndex : long.MaxValue);
					long num6 = ((num < count) ? pendingItems.Data[num].LayerIndex : long.MaxValue);
					if ((num4 != num6) ? (num4 < num6) : (num3 < num5))
					{
						if (items.Data[num2].Owner != null)
						{
							doubleBufferItems.Add(items.Data[num2]);
						}
						num2++;
					}
					else
					{
						if (pendingItems.Data[num].Owner != null)
						{
							doubleBufferItems.Add(pendingItems.Data[num]);
						}
						num++;
					}
				}
				PerformanceList<UIRenderCommand> performanceList = items;
				items = doubleBufferItems;
				doubleBufferItems = performanceList;
				doubleBufferItems.Clear();
				pendingItems.Clear();
			}
			if (!UISettings.EnableRender.Value)
			{
				return;
			}
			int count3 = items.Count;
			for (int i = 0; i < count3; i++)
			{
				if (items.Data[i].Owner != null)
				{
					items.Data[i].Render(primitiveDrawer);
				}
			}
		}

		public IEnumerator GetEnumerator()
		{
			int count = items.Count;
			int i = 0;
			while (i < count)
			{
				yield return items.Data[i];
				int num = i + 1;
				i = num;
			}
		}

		private static bool CheckConsistency(UIRenderCommand[] datas, int count)
		{
			for (int i = 1; i < count; i++)
			{
				if (CompareRenderCommand(ref datas[i - 1], ref datas[i]) > 0)
				{
					return false;
				}
			}
			return true;
		}

		private static bool Remove(UIRenderCommand[] datas, int count, long sortingIndex, int layerIndex, UIBehaviour owner)
		{
			for (int i = 0; i < count; i++)
			{
				ref UIRenderCommand reference = ref datas[i];
				if (reference.SortingIndex == sortingIndex && reference.LayerIndex == layerIndex && reference.Owner == owner)
				{
					reference.Clear();
					return true;
				}
			}
			return false;
		}

		private static bool RemoveSorted(UIRenderCommand[] datas, int count, long sortingIndex, int layerIndex, UIBehaviour owner)
		{
			int num = 0;
			int num2 = count;
			while (num < num2)
			{
				int num3 = (num + num2) / 2;
				ref UIRenderCommand reference = ref datas[num3];
				if (reference.LayerIndex == layerIndex)
				{
					if (reference.SortingIndex == sortingIndex)
					{
						if (reference.Owner == owner)
						{
							num = num3;
							break;
						}
						bool flag = false;
						for (int i = num; i < num2; i++)
						{
							if (datas[i].Owner == owner && datas[i].SortingIndex == sortingIndex && datas[i].LayerIndex == layerIndex)
							{
								num = i;
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							num = num2;
						}
						break;
					}
					if (reference.SortingIndex < sortingIndex)
					{
						num = num3 + 1;
					}
					else
					{
						num2 = num3;
					}
				}
				else if (reference.LayerIndex < layerIndex)
				{
					num = num3 + 1;
				}
				else
				{
					num2 = num3;
				}
			}
			if (num < num2)
			{
				datas[num].Clear();
				return true;
			}
			return false;
		}

		private static int CompareRenderCommand(ref UIRenderCommand left, ref UIRenderCommand right)
		{
			if (left.LayerIndex == right.LayerIndex)
			{
				if (left.SortingIndex == right.SortingIndex)
				{
					return 0;
				}
				if (left.SortingIndex >= right.SortingIndex)
				{
					return 1;
				}
				return -1;
			}
			if (left.LayerIndex >= right.LayerIndex)
			{
				return 1;
			}
			return -1;
		}
	}
}
