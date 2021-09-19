namespace Amplitude.UI.Traits
{
	public interface IUITraitRightBorderAnchor : IUITrait<UIBorderAnchor>, IUITrait<float>, IUITrait<bool>
	{
		UIBorderAnchor RightAnchor { get; set; }
	}
}
