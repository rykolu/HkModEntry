using System;
using System.Collections.Generic;

namespace Amplitude.UI.Boards
{
	public class UIBoardEntriesComparer : IUIBoardEntriesComparerReadOnly
	{
		private struct OrderParameter
		{
			public readonly StaticString Column;

			public readonly int Order;

			public OrderParameter(StaticString column, int order)
			{
				Column = column;
				Order = order;
			}

			public OrderParameter Reverse()
			{
				return new OrderParameter(Column, -Order);
			}
		}

		private List<OrderParameter> sortingOrders = new List<OrderParameter>();

		private IUIBoardProxy proxy;

		public int OrdersCount => sortingOrders.Count;

		public event Action Change;

		public UIBoardEntriesComparer(IUIBoardProxy proxy)
		{
			this.proxy = proxy;
		}

		public StaticString GetOrder(int index, out int order)
		{
			if (index >= 0 && index < sortingOrders.Count)
			{
				order = sortingOrders[index].Order;
				return sortingOrders[index].Column;
			}
			order = 0;
			return StaticString.Empty;
		}

		public int FindOrder(StaticString column)
		{
			int ordersCount = OrdersCount;
			for (int i = 0; i < ordersCount; i++)
			{
				if (sortingOrders[i].Column == column)
				{
					return sortingOrders[i].Order;
				}
			}
			return 0;
		}

		public void Push(StaticString columnKey, bool reverse)
		{
			if (sortingOrders.Count > 0 && sortingOrders[0].Column == columnKey)
			{
				sortingOrders[0] = sortingOrders[0].Reverse();
				return;
			}
			for (int num = sortingOrders.Count - 1; num >= 0; num--)
			{
				if (sortingOrders[num].Column == columnKey)
				{
					sortingOrders.RemoveAt(num);
					break;
				}
			}
			sortingOrders.Insert(0, new OrderParameter(columnKey, (!reverse) ? 1 : (-1)));
			this.Change?.Invoke();
		}

		public void Clear()
		{
			sortingOrders.Clear();
			this.Change?.Invoke();
		}

		public int Compare(IUIBoardEntry left, IUIBoardEntry right)
		{
			return left.CompareTo(right, this, proxy);
		}
	}
}
