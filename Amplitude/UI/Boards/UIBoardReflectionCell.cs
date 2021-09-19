using System;
using System.Reflection;

namespace Amplitude.UI.Boards
{
	public abstract class UIBoardReflectionCell<PropertyType> : UIBoardCell<object> where PropertyType : IComparable
	{
		private string propertyName = string.Empty;

		private PropertyInfo property;

		protected override Type ColumnDefinitionType => typeof(UIBoardReflectionColumnDefinition);

		public override int CompareTo(IUIBoardCell other)
		{
			UIBoardReflectionCell<PropertyType> uIBoardReflectionCell = (UIBoardReflectionCell<PropertyType>)other;
			return GetPropertyValue().CompareTo(uIBoardReflectionCell.GetPropertyValue());
		}

		public override void Load(IUIBoardProxy boardProxy, UIBoardColumnDefinition definition)
		{
			base.Load(boardProxy, definition);
			UIBoardReflectionColumnDefinition uIBoardReflectionColumnDefinition = definition as UIBoardReflectionColumnDefinition;
			if (uIBoardReflectionColumnDefinition != null)
			{
				propertyName = uIBoardReflectionColumnDefinition.Property;
			}
		}

		protected PropertyType GetPropertyValue()
		{
			return (PropertyType)property.GetValue(base.Model, null);
		}

		protected override void Bind(object model)
		{
			property = model.GetType().GetProperty(propertyName);
		}

		protected override void Unbind()
		{
			property = null;
		}
	}
}
