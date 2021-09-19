using System;
using System.Diagnostics;

namespace Amplitude.UI
{
	public class SortedSet<T> where T : struct
	{
		private PerformanceList<T> items;

		private AllocFreeSorter.CompareRef<T> comparer;

		private bool sorted = true;

		public int Count => items.Count;

		public T[] Data => items.Data;

		[Obsolete("Please Use Data")]
		public T this[int index] => items.Data[index];

		public SortedSet(AllocFreeSorter.CompareRef<T> comparer)
		{
			this.comparer = comparer;
		}

		public void Add(ref T item)
		{
			sorted = false;
			items.Add(item);
		}

		public void Sort()
		{
			if (!sorted)
			{
				AllocFreeSorter.Sort(items.Data, items.Count, comparer);
				sorted = true;
			}
		}

		public void Clear()
		{
			items.Clear();
			sorted = true;
		}

		[Conditional("UNITY_EDITOR")]
		public void CheckConsistency()
		{
			int count = items.Count;
			for (int i = 1; i < count; i++)
			{
			}
		}
	}
}
