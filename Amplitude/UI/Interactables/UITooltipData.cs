using System;
using Amplitude.UI.Tooltips;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[Serializable]
	public struct UITooltipData
	{
		public static readonly UITooltipData Empty = new UITooltipData(null, null, null, null);

		[SerializeField]
		public string Message;

		[SerializeField]
		public UITooltipClassDefinition Class;

		[NonSerialized]
		public object Target;

		[NonSerialized]
		public object Context;

		public UITooltipData(UITooltipClassDefinition @class, string message, object target, object context)
		{
			Class = @class;
			Message = message;
			Target = target;
			Context = context;
		}

		public static bool operator ==(UITooltipData left, UITooltipData right)
		{
			if (left.Message == right.Message && left.Class == right.Class && left.Target == right.Target)
			{
				return left.Context == right.Context;
			}
			return false;
		}

		public static bool operator !=(UITooltipData left, UITooltipData right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (obj is UITooltipData)
			{
				return this == (UITooltipData)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
