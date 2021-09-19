using System;
using Amplitude.Extensions;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationMultiController : UIAnimationBaseController
	{
		[SerializeField]
		private UIComponent[] targets;

		public override UIComponent FindTarget(int index)
		{
			if (targets != null && index >= 0 && index < targets.Length)
			{
				return targets[index];
			}
			return null;
		}

		public override bool IsLoaded()
		{
			if (targets != null)
			{
				UIComponent[] array = targets;
				foreach (UIComponent uIComponent in array)
				{
					if (uIComponent != null && !uIComponent.Loaded)
					{
						return false;
					}
				}
			}
			return true;
		}

		public override IUIAnimationItem CreateItem(Type itemType, UIComponent target)
		{
			targets = targets.Append(target);
			return base.CreateItem(itemType, target);
		}

		public override void RemoveItem(int index)
		{
			if (index >= 0 && index < targets.Length)
			{
				targets = targets.RemoveAt(index);
			}
			base.RemoveItem(index);
		}

		public override void SwapItems(int index1, int index2)
		{
			int num = Mathf.Max(index1, index2);
			if (num < targets.Length)
			{
				targets = targets.Ensure(num + 1);
			}
			UIComponent uIComponent = targets[index1];
			targets[index1] = targets[index2];
			targets[index2] = uIComponent;
			base.SwapItems(index1, index2);
		}

		internal void LoadTargetsIfNecessary()
		{
			if (targets == null)
			{
				return;
			}
			UIComponent[] array = targets;
			foreach (UIComponent uIComponent in array)
			{
				if (uIComponent != null)
				{
					uIComponent.LoadIfNecessary();
				}
			}
		}

		internal void SetTarget(IUIAnimationItem item, UIComponent target)
		{
			int length = items.Length;
			for (int i = 0; i < length; i++)
			{
				if (items[i] == item)
				{
					if (targets == null)
					{
						targets = new UIComponent[i + 1];
					}
					else if (i >= targets.Length)
					{
						targets = targets.Ensure(i + 1);
					}
					targets[i] = target;
					item.Initialize(this, target);
					break;
				}
			}
		}
	}
}
