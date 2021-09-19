using System;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationController : UIAnimationBaseController, ISerializationCallbackReceiver
	{
		[SerializeField]
		private UIComponent target;

		public override bool IsInitialized => target != null;

		public UIAnimationController(UIComponent target)
		{
			this.target = target;
		}

		public IUIAnimationItem CreateItem(Type type)
		{
			return base.CreateItem(type, target);
		}

		public override UIComponent FindTarget(int index)
		{
			return target;
		}

		public override bool IsLoaded()
		{
			if (!(target == null))
			{
				return target.Loaded;
			}
			return true;
		}
	}
}
