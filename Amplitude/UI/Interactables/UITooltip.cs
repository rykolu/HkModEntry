using System;
using Amplitude.UI.Tooltips;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UITransform))]
	public class UITooltip : UIInteractable
	{
		[SerializeField]
		[HideInInspector]
		private UITooltipData uiTooltipData;

		[SerializeField]
		[HideInInspector]
		private UITransform anchor;

		[SerializeField]
		[HideInInspector]
		private UITooltipAnchorMode anchorMode;

		[SerializeField]
		[HideInInspector]
		private bool useDefaultAnchor;

		private bool isBound;

		public UITooltipData Data => uiTooltipData;

		public string Message
		{
			get
			{
				return uiTooltipData.Message;
			}
			set
			{
				if (isBound && uiTooltipData.Class == null && uiTooltipData.Target == null && uiTooltipData.Context == null)
				{
					uiTooltipData.Message = value;
					OnContentChanged();
				}
				else
				{
					Bind(new UITooltipData
					{
						Message = value
					});
				}
			}
		}

		public UITooltipClassDefinition TooltipClass => uiTooltipData.Class;

		public object Target => uiTooltipData.Target;

		public object Context => uiTooltipData.Context;

		public UITransform Anchor
		{
			get
			{
				return anchor;
			}
			set
			{
				if (anchor != value)
				{
					anchor = value;
					TooltipResponder.OnTooltipModified();
				}
			}
		}

		public UITooltipAnchorMode AnchorMode
		{
			get
			{
				return anchorMode;
			}
			set
			{
				if (anchorMode != value)
				{
					anchorMode = value;
					TooltipResponder.OnTooltipModified();
				}
			}
		}

		public bool UseDefaultAnchor
		{
			get
			{
				return useDefaultAnchor;
			}
			set
			{
				if (useDefaultAnchor != value)
				{
					useDefaultAnchor = value;
					TooltipResponder.OnTooltipModified();
				}
			}
		}

		public bool IsBound => isBound;

		private UITooltipResponder TooltipResponder => (UITooltipResponder)base.Responder;

		public event Action<UITooltip, bool> Hover
		{
			add
			{
				TooltipResponder.Hover += value;
			}
			remove
			{
				TooltipResponder.Hover -= value;
			}
		}

		public void Bind(UITooltipData data)
		{
			uiTooltipData = data;
			isBound = true;
			TooltipResponder.OnTooltipModified();
		}

		public void Bind(object target, object context = null)
		{
			Bind(TooltipClass, target, context);
		}

		public void Bind(UITooltipClassDefinition tooltipClass, object target, object context = null)
		{
			Bind(tooltipClass, string.Empty, target, context);
		}

		public void Bind(UITooltipClassDefinition tooltipClass, string message, object target, object context = null)
		{
			Bind(new UITooltipData(tooltipClass, message, target, context));
		}

		public void Unbind(bool preserveTooltipClass = true)
		{
			isBound = false;
			UITooltipData empty = UITooltipData.Empty;
			if (preserveTooltipClass)
			{
				empty.Class = TooltipClass;
			}
			uiTooltipData = empty;
			TooltipResponder.OnTooltipModified();
		}

		public void OnContentChanged()
		{
			TooltipResponder.OnTooltipModified();
		}

		protected override IUIResponder InstantiateResponder()
		{
			return new UITooltipResponder(this);
		}

		protected override void Load()
		{
			base.Load();
			if (!string.IsNullOrEmpty(Message))
			{
				isBound = true;
			}
		}

		protected override void Unload()
		{
			isBound = false;
			TooltipResponder.OnTooltipModified();
			base.Unload();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (!isBound && !string.IsNullOrEmpty(Message))
			{
				isBound = true;
			}
			else if (isBound && Data == default(UITooltipData))
			{
				isBound = false;
			}
		}
	}
}
