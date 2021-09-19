namespace Amplitude.UI.Traits
{
	public interface IUITraitTopBorderAnchor : IUITrait<UIBorderAnchor>, IUITrait<float>, IUITrait<bool>
	{
		UIBorderAnchor TopAnchor { get; set; }
	}
}
