using System.Collections.Generic;
using Amplitude.Framework;
using UnityEngine;

namespace Amplitude.UI.Patterns.Bricks
{
	public class BricksCompoundDefinition<T> : DatatableElement where T : BrickDefinition
	{
		[SerializeField]
		[HideInInspector]
		protected List<T> brickDefinitions = new List<T>();

		public int BrickDefinitionsCount
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

		public T GetBrickDefinition(int index)
		{
			if (brickDefinitions != null && index >= 0 && index < brickDefinitions.Count)
			{
				return brickDefinitions[index];
			}
			return null;
		}
	}
}
