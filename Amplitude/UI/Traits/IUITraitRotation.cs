using UnityEngine;

namespace Amplitude.UI.Traits
{
	public interface IUITraitRotation : IUITrait<float>
	{
		Vector3 Rotation { get; set; }
	}
}
