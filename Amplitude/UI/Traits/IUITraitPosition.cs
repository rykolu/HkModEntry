namespace Amplitude.UI.Traits
{
	public interface IUITraitPosition : IUITrait<float>
	{
		float X { get; set; }

		float Y { get; set; }

		float Width { get; set; }

		float Height { get; set; }
	}
}
