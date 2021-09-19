using System;
using System.Collections.Generic;

namespace Amplitude.UI.Boards.Filters
{
	public class UIBoardFilterController : IUIBoardGlobalFilterProvider
	{
		private IUIBoardFilter[] globalFilters;

		private Dictionary<StaticString, IUIBoardFilter> filters = new Dictionary<StaticString, IUIBoardFilter>();

		public int GlobalFiltersCount => globalFilters.Length;

		public event Action<IUIBoardFilter> FilterChanged
		{
			add
			{
				int num = globalFilters.Length;
				for (int i = 0; i < num; i++)
				{
					globalFilters[i].FilterChanged += value;
				}
				foreach (KeyValuePair<StaticString, IUIBoardFilter> filter in filters)
				{
					filter.Value.FilterChanged += value;
				}
			}
			remove
			{
				int num = globalFilters.Length;
				for (int i = 0; i < num; i++)
				{
					globalFilters[i].FilterChanged -= value;
				}
				foreach (KeyValuePair<StaticString, IUIBoardFilter> filter in filters)
				{
					filter.Value.FilterChanged -= value;
				}
			}
		}

		public UIBoardFilterController(UIBoardDefinition definition)
		{
			int num = ((definition.GlobalFilters != null) ? definition.GlobalFilters.Count : 0);
			globalFilters = new IUIBoardFilter[num];
			for (int i = 0; i < num; i++)
			{
				globalFilters[i] = definition.GlobalFilters[i].Create();
			}
			int columnsCount = definition.ColumnsCount;
			for (int j = 0; j < columnsCount; j++)
			{
				UIBoardColumnDefinition uIBoardColumnDefinition = definition.ColumnDefinitions[j];
				if (uIBoardColumnDefinition.Filter != null)
				{
					filters.Add(uIBoardColumnDefinition.Name, uIBoardColumnDefinition.Filter.Create());
				}
			}
		}

		public bool TryFind(StaticString filterKey, out IUIBoardFilter filter)
		{
			return filters.TryGetValue(filterKey, out filter);
		}

		public IUIBoardFilter GetGlobalFilter(int index)
		{
			return globalFilters[index];
		}

		public T GetGlobalFilter<T>() where T : class, IUIBoardFilter
		{
			int num = ((globalFilters != null) ? globalFilters.Length : 0);
			for (int i = 0; i < num; i++)
			{
				T val = globalFilters[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}
	}
}
