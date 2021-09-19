using System;
using Amplitude.UI.Interactables;
using Amplitude.UI.Patterns;
using UnityEngine;

namespace Amplitude.UI.Tooltips
{
	public class UITooltipBrick : UIComponent, IBrickDefinitionProvider
	{
		[SerializeField]
		private bool requiresTarget;

		[SerializeField]
		private bool requiresContext;

		[SerializeField]
		private bool isDecoration;

		private UITooltipBrickDefinition definition;

		public bool RequiresTarget => requiresTarget;

		public bool RequiresContext => requiresContext;

		public bool IsDecoration => isDecoration;

		public virtual Type BrickDefinitionType => typeof(UITooltipBrickDefinition);

		protected UITooltipBrickDefinition Definition => definition;

		public virtual void PostLoad()
		{
		}

		public virtual void PreUnload()
		{
		}

		public bool Bind(UITooltipBrickDefinition definition, UITooltipData tooltipData)
		{
			this.definition = definition;
			object obj = null;
			object obj2 = null;
			if (tooltipData.Target != null)
			{
				obj = tooltipData.Target;
			}
			if (RequiresTarget && obj == null)
			{
				Diagnostics.LogError(57uL, "'{0}': Brick '{1}' requires a target but none is provided.", tooltipData.Class, ToString());
				return false;
			}
			if (tooltipData.Context != null)
			{
				obj2 = tooltipData.Context;
			}
			if (RequiresContext && obj2 == null)
			{
				Diagnostics.LogError(57uL, "'{0}': Brick '{1}' requires a context but none is provided.", tooltipData.Class, ToString());
				return false;
			}
			return Bind(tooltipData.Message, obj, obj2);
		}

		public virtual void Unbind()
		{
			definition = null;
		}

		public override string ToString()
		{
			string text = GetType().ToString();
			return text.Substring(text.LastIndexOf('.') + 1);
		}

		protected virtual bool Bind(string message, object target, object context)
		{
			return true;
		}
	}
}
