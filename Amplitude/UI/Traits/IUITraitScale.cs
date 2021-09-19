using UnityEngine;

namespace Amplitude.UI.Traits
{
	public interface IUITraitScale : IUITrait<Vector3>
	{
		Vector3 Scale { get; set; }
	}
}
