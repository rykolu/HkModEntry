using System;
using Amplitude.UI.Boards.Filters;
using Amplitude.UI.Patterns.Bricks;
using UnityEngine;

namespace Amplitude.UI.Boards
{
	[Serializable]
	public class UIBoardColumnDefinition : BrickDefinition
	{
		[SerializeField]
		[Tooltip("NB: Make sure the UIBoardCell implementation do implement 'CompareTo'!")]
		public bool IsSortable = true;

		[SerializeField]
		public bool UseDefaultBackground = true;

		[SerializeField]
		public string SizeDescription = string.Empty;

		[SerializeField]
		[HideInInspector]
		public UIBoardFilterDefinition Filter;

		[SerializeField]
		[HideInInspector]
		private string serializableName = string.Empty;

		[NonSerialized]
		private new StaticString name = StaticString.Empty;

		public StaticString Name
		{
			get
			{
				if (StaticString.IsNullOrEmpty(name))
				{
					name = new StaticString(serializableName);
				}
				return name;
			}
		}

		public static UIBoardColumnDefinition Create(Type cellDefinitionType, UIBoardColumnDefinition original = null)
		{
			UIBoardColumnDefinition uIBoardColumnDefinition = ScriptableObject.CreateInstance(cellDefinitionType) as UIBoardColumnDefinition;
			if (original != null)
			{
				uIBoardColumnDefinition.Copy(original);
			}
			return uIBoardColumnDefinition;
		}

		public static UIBoardColumnDefinition Create(UIBoardColumnDefinition original = null)
		{
			UIBoardColumnDefinition uIBoardColumnDefinition = ScriptableObject.CreateInstance<UIBoardColumnDefinition>();
			if (original != null)
			{
				uIBoardColumnDefinition.Copy(original);
			}
			return uIBoardColumnDefinition;
		}

		public void ApplySize(IUIBoardProxy proxy, UITransform cell)
		{
			if (string.IsNullOrEmpty(SizeDescription))
			{
				return;
			}
			int result = 0;
			bool flag = SizeDescription.EndsWith("*");
			if (flag && SizeDescription.Length == 1)
			{
				result = 1;
			}
			else if (!int.TryParse(flag ? SizeDescription.Substring(0, SizeDescription.Length - 1) : SizeDescription, out result))
			{
				return;
			}
			if (flag)
			{
				cell.ResizeWeight = result;
			}
			else
			{
				cell.ResizeWeight = 0;
				switch (proxy.EntriesOrientation)
				{
				case Orientation.Horizontal:
					cell.Height = result;
					break;
				case Orientation.Vertical:
					cell.Width = result;
					break;
				}
			}
			bool attach = proxy.EntriesOrientation == Orientation.Horizontal;
			cell.LeftAnchor = new UIBorderAnchor(attach, 0f, 0f, 0f);
			cell.RightAnchor = new UIBorderAnchor(attach, 1f, 0f, 0f);
			bool attach2 = proxy.EntriesOrientation == Orientation.Vertical;
			cell.TopAnchor = new UIBorderAnchor(attach2, 0f, 0f, 0f);
			cell.BottomAnchor = new UIBorderAnchor(attach2, 1f, 0f, 0f);
		}

		public override void Copy(BrickDefinition other)
		{
			base.Copy(other);
			UIBoardColumnDefinition uIBoardColumnDefinition = other as UIBoardColumnDefinition;
			if (uIBoardColumnDefinition != null)
			{
				serializableName = uIBoardColumnDefinition.serializableName;
				name = uIBoardColumnDefinition.name;
				UseDefaultBackground = uIBoardColumnDefinition.UseDefaultBackground;
				SizeDescription = uIBoardColumnDefinition.SizeDescription;
			}
		}

		public override string ToString()
		{
			string empty = string.Empty;
			if (!StaticString.IsNullOrEmpty(Name))
			{
				empty = Name.ToString();
			}
			else
			{
				string text = GetType().ToString();
				empty = text.Substring(text.LastIndexOf('.') + 1);
			}
			if (Prefab != null)
			{
				string text2 = Prefab.ToString();
				string text3 = text2.Substring(0, text2.IndexOf('(') - 1);
				empty = empty + " - " + text3;
			}
			if (!string.IsNullOrEmpty(SizeDescription))
			{
				empty = empty + " (" + SizeDescription + ")";
			}
			return empty;
		}

		internal void ResetNameCache()
		{
			name = StaticString.Empty;
		}
	}
}
