using System;
using UnityEngine;

namespace Amplitude.UI.Boards
{
	[Serializable]
	public class UIBoardReflectionColumnDefinition : UIBoardColumnDefinition
	{
		[SerializeField]
		public string Property = string.Empty;
	}
}
