using Amplitude.UI.Boards.Filters;
using Amplitude.UI.Patterns;

namespace Amplitude.UI.Boards
{
	public interface IUIBoardCell : IBrickDefinitionProvider
	{
		UIBoardColumnDefinition ColumnDefinition { get; }

		void Load(IUIBoardProxy proxy, UIBoardColumnDefinition definition);

		void Unload();

		void Bind<Model>(Model model);

		void Unbind();

		void Refresh();

		bool Filter(UIBoardFilterController filters);

		int CompareTo(IUIBoardCell other);
	}
}
