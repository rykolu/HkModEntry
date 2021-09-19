using UnityEngine;

namespace Amplitude.UI.Traits
{
	public interface IUITraitColor : IUITrait<Color>
	{
		Color Color { get; set; }
	}
}
