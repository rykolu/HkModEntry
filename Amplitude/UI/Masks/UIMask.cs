using System;
using Amplitude.Framework;
using UnityEngine;

namespace Amplitude.UI.Masks
{
	public abstract class UIMask : UIIndexedComponent
	{
		[Flags]
		public enum MaskFilter
		{
			Renderers = 0x1,
			Interactables = 0x2
		}

		[SerializeField]
		[EnumBitMask(typeof(MaskFilter))]
		private MaskFilter filter = MaskFilter.Renderers | MaskFilter.Interactables;

		public MaskFilter Filter
		{
			get
			{
				return filter;
			}
			set
			{
				if (filter != value)
				{
					MaskFilter previousFilter = filter;
					filter = value;
					OnFilterChanged(previousFilter, filter);
				}
			}
		}

		public bool VisibleGlobally
		{
			get
			{
				if (base.enabled && base.IsUpToDate)
				{
					return UITransform.VisibleGlobally;
				}
				return false;
			}
		}

		public abstract bool Contains(Vector2 pointPosition);

		protected virtual void OnFilterChanged(MaskFilter previousFilter, MaskFilter filter)
		{
		}
	}
}
