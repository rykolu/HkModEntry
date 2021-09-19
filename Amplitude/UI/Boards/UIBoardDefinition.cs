using System;
using System.Collections.Generic;
using Amplitude.UI.Boards.Filters;
using Amplitude.UI.Patterns.Bricks;
using UnityEngine;

namespace Amplitude.UI.Boards
{
	[Serializable]
	[CreateAssetMenu(fileName = "NewBoard.asset", menuName = "Amplitude/UI/BoardDefinition")]
	public class UIBoardDefinition : BricksCompoundDefinition<UIBoardColumnDefinition>
	{
		[SerializeField]
		[HideInInspector]
		private List<UIBoardFilterDefinition> globalFilterDefinitions = new List<UIBoardFilterDefinition>();

		[SerializeField]
		[Tooltip("When sorting according to that Column, should disabled entries be put at the end? Ignored if the Sorting is disabled.")]
		private bool sortDisabledEntriesAtTheEnd;

		public int ColumnsCount
		{
			get
			{
				if (brickDefinitions == null)
				{
					return 0;
				}
				return brickDefinitions.Count;
			}
		}

		public bool SortDisabledEntriesAtTheEnd => sortDisabledEntriesAtTheEnd;

		internal List<UIBoardColumnDefinition> ColumnDefinitions => brickDefinitions;

		internal List<UIBoardFilterDefinition> GlobalFilters => globalFilterDefinitions;

		public UIBoardColumnDefinition GetColumnDefinition(int index)
		{
			if (index >= 0 && index < ColumnsCount)
			{
				return brickDefinitions[index];
			}
			return null;
		}

		public UIBoardColumnDefinition GetColumnDefinition(StaticString columnName)
		{
			int count = brickDefinitions.Count;
			for (int i = 0; i < count; i++)
			{
				if (brickDefinitions[i].Name == columnName)
				{
					return brickDefinitions[i];
				}
			}
			return null;
		}
	}
}
