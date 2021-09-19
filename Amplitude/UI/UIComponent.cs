using System;
using UnityEngine;

namespace Amplitude.UI
{
	[RequireComponent(typeof(UITransform))]
	public abstract class UIComponent : UIBehaviour
	{
		[NonSerialized]
		public UITransform UITransform;

		public UIStamp Stamp;

		public override string ToString()
		{
			if (UITransform != null)
			{
				return $"{UITransform}|{GetType().Name}";
			}
			return $"???|{GetType().Name}";
		}

		protected override void Load()
		{
			base.Load();
			UITransform = GetComponent<UITransform>();
			if (this != UITransform)
			{
				UITransform.LoadIfNecessary();
			}
			if (Application.isPlaying)
			{
				Stamp.LoadIfNecessary(this);
			}
		}

		protected override void Unload()
		{
			if (Application.isPlaying)
			{
				Stamp.Unload();
			}
			base.Unload();
		}
	}
}
