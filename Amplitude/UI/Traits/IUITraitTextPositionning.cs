namespace Amplitude.UI.Traits
{
	public interface IUITraitTextPositionning : IUITrait<bool>, IUITrait<int>
	{
		bool UseAscent { get; set; }

		int InterLineAdditionalSpacing { get; set; }

		int InterParagraphAdditionalSpacing { get; set; }
	}
}
