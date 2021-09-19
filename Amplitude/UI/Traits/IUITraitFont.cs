using Amplitude.Graphics.Text;

namespace Amplitude.UI.Traits
{
	public interface IUITraitFont : IUITrait<FontFamily>, IUITrait<FontFace>, IUITrait<int>, IUITrait<bool>, IUITrait<FontRenderingMode>
	{
		FontFamily FontFamily { get; set; }

		FontFace FontFace { get; set; }

		uint FontSize { get; set; }

		FontRenderingMode RenderingMode { get; set; }

		bool ForceCaps { get; set; }
	}
}
