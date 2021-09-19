using UnityEngine;

namespace Amplitude.UI.Patterns.Bricks
{
	public class BrickDefinition : ScriptableObject
	{
		public Transform Prefab;

		public virtual void Copy(BrickDefinition source)
		{
			Prefab = source.Prefab;
		}
	}
}
