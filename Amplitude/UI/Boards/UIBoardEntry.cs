using System;
using System.Collections.Generic;
using Amplitude.UI.Boards.Filters;
using Amplitude.UI.Interactables;
using UnityEngine;

namespace Amplitude.UI.Boards
{
	public class UIBoardEntry : UIComponent, IUIBoardEntry
	{
		[NonSerialized]
		protected IUIBoardCell[] cells;

		private static List<IUIBoardCell> tmpCellsList;

		[SerializeField]
		private UITransform cellsTable;

		[SerializeField]
		[Tooltip("Optional")]
		private UIControl control;

		[SerializeField]
		[Tooltip("Optional")]
		private UITooltip tooltip;

		public UITransform Transform => GetComponent<UITransform>();

		public UITransform CellsTable => cellsTable;

		public bool Selected => (control as IUIToggle)?.State ?? false;

		public bool Enabled => GetComponent<UITransform>().InteractiveSelf;

		public bool IsBound { get; private set; }

		public event Action<IUIBoardEntry, bool> Switch;

		public event Action<IUIBoardEntry> Select;

		public void Load(IUIBoardProxy proxy)
		{
			int columnsCount = proxy.Definition.ColumnsCount;
			if (tmpCellsList == null)
			{
				tmpCellsList = new List<IUIBoardCell>();
			}
			cells = new IUIBoardCell[columnsCount];
			for (int i = 0; i < columnsCount; i++)
			{
				IUIBoardCell iUIBoardCell = CreateCell(proxy, i);
				if (iUIBoardCell != null)
				{
					tmpCellsList.Add(iUIBoardCell);
				}
			}
			cells = tmpCellsList.ToArray();
			tmpCellsList.Clear();
			RegisterInteractivity(proxy);
			PostLoad();
		}

		public new void Unload()
		{
			Unbind();
			PreUnload();
			UnregisterInteractivity();
			if (cells != null)
			{
				Array.Clear(cells, 0, cells.Length);
			}
			for (int num = CellsTable.Children.Count - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(CellsTable.Children.Data[num].gameObject);
			}
		}

		public void Bind<TData>(TData data) where TData : class
		{
			int num = ((cells != null) ? cells.Length : 0);
			for (int i = 0; i < num; i++)
			{
				cells[i].Bind(data);
			}
			SetSelected(value: false);
			IsBound = true;
			PostBind();
		}

		public void Unbind()
		{
			PreUnbind();
			IsBound = false;
			SetSelected(value: false);
			int num = ((cells != null) ? cells.Length : 0);
			for (int i = 0; i < num; i++)
			{
				cells[i].Unbind();
			}
			if (tooltip != null)
			{
				tooltip.Unbind();
			}
		}

		public void Refresh()
		{
			int num = ((cells != null) ? cells.Length : 0);
			for (int i = 0; i < num; i++)
			{
				cells[i].Refresh();
			}
		}

		public bool Filter(UIBoardFilterController filters)
		{
			bool flag = DoFilter(filters);
			int num = ((cells != null) ? cells.Length : 0);
			int num2 = 0;
			while (flag && num2 < num)
			{
				flag &= cells[num2].Filter(filters);
				num2++;
			}
			return flag;
		}

		public void SetSelected(bool value, bool silent = true)
		{
			IUIToggle iUIToggle = control as IUIToggle;
			if (iUIToggle != null && iUIToggle.State != value)
			{
				iUIToggle.State = value;
				if (!silent)
				{
					this.Switch?.Invoke(this, value);
				}
			}
		}

		public void SetEnabled(bool valid, UITooltipData data)
		{
			UITransform component = GetComponent<UITransform>();
			if (valid != component.InteractiveSelf)
			{
				component.InteractiveSelf = valid;
			}
			if (tooltip != null)
			{
				tooltip.Unbind();
				tooltip.UITransform.VisibleSelf = valid;
				if (!valid)
				{
					tooltip.Bind(data);
				}
			}
		}

		int IUIBoardEntry.CompareTo(IUIBoardEntry other, IUIBoardEntriesComparerReadOnly comparer, IUIBoardProxy proxy)
		{
			if (IsBound && other.IsBound)
			{
				UIBoardDefinition definition = proxy.Definition;
				if (definition.SortDisabledEntriesAtTheEnd && (Enabled ^ other.Enabled))
				{
					if (Enabled)
					{
						return -1;
					}
					return 1;
				}
				int ordersCount = comparer.OrdersCount;
				for (int i = 0; i < ordersCount; i++)
				{
					int order = 0;
					StaticString order2 = comparer.GetOrder(i, out order);
					definition.GetColumnDefinition(order2);
					IUIBoardCell iUIBoardCell = ((IUIBoardEntry)this).FindCell(order2);
					IUIBoardCell iUIBoardCell2 = other.FindCell(order2);
					if (iUIBoardCell != null && iUIBoardCell2 != null)
					{
						int num = iUIBoardCell.CompareTo(iUIBoardCell2);
						if (num != 0)
						{
							return num * order;
						}
						continue;
					}
					if (iUIBoardCell != null)
					{
						return 1;
					}
					if (iUIBoardCell2 != null)
					{
						return -1;
					}
					return 0;
				}
			}
			else
			{
				if (IsBound)
				{
					return 1;
				}
				if (other.IsBound)
				{
					return -1;
				}
			}
			return 0;
		}

		IUIBoardCell IUIBoardEntry.FindCell(StaticString column)
		{
			int num = ((cells != null) ? cells.Length : 0);
			for (int i = 0; i < num; i++)
			{
				if (cells[i].ColumnDefinition.Name == column)
				{
					return cells[i];
				}
			}
			return null;
		}

		protected virtual void PostLoad()
		{
		}

		protected virtual void PreUnload()
		{
		}

		protected virtual void PostBind()
		{
		}

		protected virtual void PreUnbind()
		{
		}

		protected virtual bool DoFilter(IUIBoardGlobalFilterProvider globalFilters)
		{
			return true;
		}

		private IUIBoardCell CreateCell(IUIBoardProxy boardProxy, int cellIndex)
		{
			UIBoardColumnDefinition columnDefinition = boardProxy.Definition.GetColumnDefinition(cellIndex);
			if (columnDefinition == null || columnDefinition.Prefab == null)
			{
				Diagnostics.LogError("Invalid CellDefinition");
				return null;
			}
			UITransform uITransform = CellsTable.InstantiateChild(columnDefinition.Prefab);
			IUIBoardCell component = uITransform.GetComponent<IUIBoardCell>();
			if (component != null)
			{
				component.Load(boardProxy, columnDefinition);
			}
			else
			{
				Diagnostics.LogError("Invalid TableCell prefab.");
				UnityEngine.Object.Destroy(uITransform.gameObject);
			}
			return component;
		}

		private void RegisterInteractivity(IUIBoardProxy proxy)
		{
			switch (proxy.Interactivity)
			{
			case UIBoard.UIBoardInteractivityType.MultiSelection:
				if (TryRegister(proxy, control as IUIToggle, register: true))
				{
					return;
				}
				break;
			case UIBoard.UIBoardInteractivityType.RadioSelection:
				if (TryRegister(proxy, control as IUIToggle, register: true) || TryRegister(proxy, control as IUIButton, register: true))
				{
					return;
				}
				break;
			default:
				return;
			}
			Diagnostics.Log($"BoardEntry interactivity is incorrect. Board interactivity {proxy.Interactivity} cannot be managed properly!");
		}

		private void UnregisterInteractivity()
		{
			if (!TryUnregister(control as IUIToggle))
			{
				TryUnregister(control as IUIButton);
			}
		}

		private bool TryRegister(IUIBoardProxy proxy, IUIToggle toggle, bool register)
		{
			if (toggle != null)
			{
				toggle.ClickDoesntSwitchOff = proxy.Interactivity == UIBoard.UIBoardInteractivityType.RadioSelection;
				toggle.HandleDoubleClicks = proxy.Interactivity != UIBoard.UIBoardInteractivityType.None;
				toggle.Switch += OnToggleSwitch;
				if (proxy.HandleDoubleClicks && toggle.HandleDoubleClicks)
				{
					toggle.DoubleLeftClick += OnToggleDoubleLeftClick;
				}
				return true;
			}
			return false;
		}

		private bool TryRegister(IUIBoardProxy proxy, IUIButton button, bool register)
		{
			if (button != null)
			{
				button.HandleDoubleClicks = proxy.Interactivity != UIBoard.UIBoardInteractivityType.None;
				button.LeftClick += OnButtonLeftClick;
				if (proxy.HandleDoubleClicks && button.HandleDoubleClicks)
				{
					button.DoubleLeftClick += OnButtonDoubleLeftClick;
				}
				return true;
			}
			return false;
		}

		private bool TryUnregister(IUIToggle toggle)
		{
			if (toggle != null)
			{
				toggle.Switch -= OnToggleSwitch;
				toggle.DoubleLeftClick -= OnToggleDoubleLeftClick;
				return true;
			}
			return false;
		}

		private bool TryUnregister(IUIButton button)
		{
			if (button != null)
			{
				button.LeftClick -= OnButtonLeftClick;
				button.DoubleLeftClick -= OnButtonDoubleLeftClick;
				return true;
			}
			return false;
		}

		private void OnToggleSwitch(IUIToggle toggle, bool state)
		{
			this.Switch?.Invoke(this, state);
		}

		private void OnButtonLeftClick(IUIButton toggle)
		{
			this.Select?.Invoke(this);
		}

		private void OnButtonDoubleLeftClick(IUIButton button)
		{
			this.Select?.Invoke(this);
		}

		private void OnToggleDoubleLeftClick(IUIToggle toggle, bool state)
		{
			if (state)
			{
				this.Select?.Invoke(this);
			}
		}
	}
}
