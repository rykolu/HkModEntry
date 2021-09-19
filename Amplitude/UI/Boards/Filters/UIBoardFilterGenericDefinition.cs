using System;
using UnityEngine;

namespace Amplitude.UI.Boards.Filters
{
	public class UIBoardFilterGenericDefinition : UIBoardFilterDefinition, ISerializationCallbackReceiver
	{
		[NonSerialized]
		private Type filterType;

		[SerializeField]
		[HideInInspector]
		private string filterTypeName = string.Empty;

		public Type FilterType
		{
			get
			{
				return filterType;
			}
			internal set
			{
				filterType = value;
				filterTypeName = ((value != null) ? value.AssemblyQualifiedName : string.Empty);
			}
		}

		public override IUIBoardFilter Create()
		{
			if (FilterType != null)
			{
				return Activator.CreateInstance(filterType) as IUIBoardFilter;
			}
			return null;
		}

		public override string ToString()
		{
			return ((filterType != null) ? (filterType.ToString() + " ") : string.Empty) + "[Generic]";
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (!string.IsNullOrEmpty(filterTypeName))
			{
				filterType = Type.GetType(filterTypeName);
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (filterType != null)
			{
				filterTypeName = filterType.AssemblyQualifiedName;
			}
		}
	}
}
