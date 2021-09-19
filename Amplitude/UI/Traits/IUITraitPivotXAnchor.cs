namespace Amplitude.UI.Traits
{
	public interface IUITraitPivotXAnchor : IUITrait<UIPivotAnchor>, IUITrait<float>, IUITrait<bool>
	{
		UIPivotAnchor PivotXAnchor { get; set; }
	}
}
