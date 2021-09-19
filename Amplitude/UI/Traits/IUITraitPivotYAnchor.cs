namespace Amplitude.UI.Traits
{
	public interface IUITraitPivotYAnchor : IUITrait<UIPivotAnchor>, IUITrait<float>, IUITrait<bool>
	{
		UIPivotAnchor PivotYAnchor { get; set; }
	}
}
