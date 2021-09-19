namespace Amplitude.UI.Traits
{
	public interface IUITraitBottomBorderAnchor : IUITrait<UIBorderAnchor>, IUITrait<float>, IUITrait<bool>
	{
		UIBorderAnchor BottomAnchor { get; set; }
	}
}
