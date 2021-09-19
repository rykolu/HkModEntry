using System;
using UnityEngine;

namespace Amplitude.UI.Boards.Filters
{
	[Serializable]
	public abstract class UIBoardFilterDefinition : ScriptableObject
	{
		public abstract IUIBoardFilter Create();
	}
}
