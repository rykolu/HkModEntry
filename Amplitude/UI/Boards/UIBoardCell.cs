using System;
using Amplitude.UI.Boards.Filters;
using Amplitude.UI.Patterns;
using Amplitude.UI.Renderers;

namespace Amplitude.UI.Boards
{
	public abstract class UIBoardCell<T> : UIComponent, IUIBoardCell, IBrickDefinitionProvider where T : class
	{
		public UIBoardColumnDefinition ColumnDefinition { get; private set; }

		public Type BrickDefinitionType => ColumnDefinitionType;

		protected virtual Type ColumnDefinitionType => typeof(UIBoardColumnDefinition);

		protected T Model { get; private set; }

		public virtual void Load(IUIBoardProxy proxy, UIBoardColumnDefinition definition)
		{
			ColumnDefinition = definition;
			UITransform component = GetComponent<UITransform>();
			definition.ApplySize(proxy, component);
			if (definition.UseDefaultBackground && !string.IsNullOrEmpty(proxy.BackgroundStyle))
			{
				ApplyBackground(proxy.BackgroundStyle);
			}
		}

		public new virtual void Unload()
		{
			ColumnDefinition = null;
		}

		public virtual int CompareTo(IUIBoardCell other)
		{
			return 0;
		}

		void IUIBoardCell.Bind<Model>(Model model)
		{
			if (this.Model != null)
			{
				Diagnostics.LogWarning($"Binding {GetType()} while it's already bound to {model.ToString()}");
				((IUIBoardCell)this).Unbind();
			}
			this.Model = model as T;
			if (this.Model != null)
			{
				Bind(this.Model);
			}
		}

		void IUIBoardCell.Unbind()
		{
			Unbind();
			Model = null;
		}

		void IUIBoardCell.Refresh()
		{
			Refresh();
		}

		bool IUIBoardCell.Filter(UIBoardFilterController filters)
		{
			IUIBoardFilter filter = null;
			if (filters.TryFind(ColumnDefinition.Name, out filter))
			{
				return Filter(filter);
			}
			return true;
		}

		protected virtual void Bind(T model)
		{
		}

		protected virtual void Unbind()
		{
		}

		protected virtual void Refresh()
		{
		}

		protected virtual bool Filter(IUIBoardFilter filter)
		{
			return true;
		}

		protected virtual void ApplyBackground(string tags)
		{
			UIImage component = GetComponent<UIImage>();
			if (component != null)
			{
				component.StyleController.SetStyleNames(tags.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries));
			}
		}
	}
}
