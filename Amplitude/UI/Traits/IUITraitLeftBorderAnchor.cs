namespace Amplitude.UI.Traits
{
	public interface IUITraitLeftBorderAnchor : IUITrait<UIBorderAnchor>, IUITrait<float>, IUITrait<bool>
	{
		UIBorderAnchor LeftAnchor { get; set; }
	}
}
