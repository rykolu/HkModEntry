using System;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	public abstract class UIAnimationBaseController : IUIAnimationController, IUIAnimationItemClient, IUIAnimationControllerInspectable, IUIAnimationManagedController, ISerializationCallbackReceiver
	{
		[SerializeField]
		protected UIAnimationItemsCollection items = new UIAnimationItemsCollection();

		[SerializeField]
		private UIAnimationTemplate template;

		[SerializeField]
		private float durationFactor = 1f;

		private bool active;

		public IUIAnimationItemsReadOnlyCollection Items => items;

		public float DurationFactor
		{
			get
			{
				return durationFactor;
			}
			set
			{
				durationFactor = Mathf.Max(float.Epsilon, value);
			}
		}

		public virtual bool IsInitialized => true;

		UIAnimationTemplate IUIAnimationControllerInspectable.Template => template;

		public bool Active => active;

		public event Action<bool> ActiveChanged;

		public abstract UIComponent FindTarget(int index);

		public abstract bool IsLoaded();

		public float ComputeFullDuration(bool forward = true)
		{
			float num = 0f;
			int length = items.Length;
			for (int i = 0; i < length; i++)
			{
				IUIAnimationItem iUIAnimationItem = items[i];
				if (iUIAnimationItem != null)
				{
					num = Mathf.Max(iUIAnimationItem.Duration + (forward ? iUIAnimationItem.Delay : iUIAnimationItem.ReverseDelay), num);
				}
			}
			return num;
		}

		public void ComputeAutoReverse()
		{
			float num = ComputeFullDuration();
			int length = items.Length;
			for (int i = 0; i < length; i++)
			{
				IUIAnimationItem iUIAnimationItem = items[i];
				iUIAnimationItem.ReverseDelay = num - (iUIAnimationItem.Delay + iUIAnimationItem.Duration);
			}
		}

		public bool IsAnyItemInProgress()
		{
			int length = items.Length;
			for (int i = 0; i < length; i++)
			{
				if (items[i] != null && items[i].InProgress)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsAnyItemPaused()
		{
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i].IsPaused)
				{
					return true;
				}
			}
			return false;
		}

		public ItemType FindItem<ItemType>(UIComponent target = null) where ItemType : class, IUIAnimationItem
		{
			int length = items.Length;
			for (int i = 0; i < length; i++)
			{
				ItemType val = items[i] as ItemType;
				if (val != null && (target == null || FindTarget(i) == target))
				{
					return val;
				}
			}
			return null;
		}

		public IUIAnimationItem FindItem(Type type, UIComponent target = null)
		{
			int length = items.Length;
			for (int i = 0; i < length; i++)
			{
				if (items[i] != null && type == items[i].GetType() && (target == null || FindTarget(i) == target))
				{
					return items[i];
				}
			}
			return null;
		}

		public void StartAnimations(bool forward = true, bool autoTriggerOnly = false)
		{
			UIAnimationManager instance = UIAnimationManager.Instance;
			if (instance != null)
			{
				instance.StartAnimation(this);
			}
			else
			{
				Diagnostics.LogWarning(" Animation could not be launched. UIAnimationManager is missing.");
			}
			int length = items.Length;
			for (int i = 0; i < length; i++)
			{
				IUIAnimationItem iUIAnimationItem = items[i];
				if (items[i] != null && (!autoTriggerOnly || iUIAnimationItem.AutoTrigger))
				{
					items[i].StartAnimation(forward);
				}
			}
		}

		public void PauseAnimations(bool paused)
		{
			int length = items.Length;
			for (int i = 0; i < length; i++)
			{
				if (items[i] != null)
				{
					items[i].IsPaused = paused;
				}
			}
		}

		public void StopAnimations()
		{
			UIAnimationManager instance = UIAnimationManager.Instance;
			if (instance != null)
			{
				instance.StopAnimation(this);
			}
			int length = items.Length;
			for (int i = 0; i < length; i++)
			{
				items[i]?.StopAnimation();
			}
		}

		public void ResetAnimations(bool toStart = true, bool applyValue = true)
		{
			int length = items.Length;
			for (int i = 0; i < length; i++)
			{
				items[i]?.ResetAnimation(toStart, applyValue);
			}
		}

		public void ReverseAnimations(bool allReversed)
		{
			bool flag = IsAnyItemInProgress();
			if (!flag)
			{
				StartAnimations(!allReversed);
				return;
			}
			int length = Items.Length;
			for (int i = 0; i < length; i++)
			{
				IUIAnimationItem iUIAnimationItem = Items[i];
				if (iUIAnimationItem == null)
				{
					continue;
				}
				if (iUIAnimationItem.InProgress)
				{
					if (iUIAnimationItem.IsReversed != allReversed)
					{
						iUIAnimationItem.ReverseAnimation();
					}
				}
				else
				{
					iUIAnimationItem.StartAnimation(!allReversed, flag);
				}
			}
		}

		void IUIAnimationItemClient.OnItemStart(IUIAnimationItem item)
		{
			if (!Active)
			{
				UIAnimationManager.Instance?.StartAnimation(this);
			}
		}

		void IUIAnimationItemClient.OnItemStop(IUIAnimationItem item)
		{
			if (!Active)
			{
				return;
			}
			int length = Items.Length;
			for (int i = 0; i < length; i++)
			{
				if (Items[i] != null && Items[i].InProgress)
				{
					return;
				}
			}
			UIAnimationManager.Instance?.StopAnimation(this);
		}

		public virtual IUIAnimationItem CreateItem(Type type, UIComponent target = null)
		{
			IUIAnimationItem iUIAnimationItem = Activator.CreateInstance(type) as IUIAnimationItem;
			iUIAnimationItem.Initialize(this, target, setValuesToDefault: true);
			items.Append(iUIAnimationItem);
			return iUIAnimationItem;
		}

		public virtual void SwapItems(int index1, int index2)
		{
			items.Swap(index1, index2);
		}

		public virtual void RemoveItem(int index)
		{
			items.Remove(index);
		}

		public void ApplyTemplate(UIAnimationTemplate template)
		{
			if (template != null)
			{
				items.Clear();
				items.Copy(template.Items);
				int length = items.Length;
				for (int i = 0; i < length; i++)
				{
					if (items[i] == null)
					{
						Diagnostics.LogWarning("Null AnimationItem found in template...");
					}
					else
					{
						items[i].Initialize(this, FindTarget(i));
					}
				}
			}
			this.template = template;
		}

		void IUIAnimationManagedController.SetActive(bool value)
		{
			if (active != value)
			{
				active = value;
				this.ActiveChanged?.Invoke(value);
			}
		}

		bool IUIAnimationManagedController.UpdateAnimation()
		{
			bool flag = false;
			int length = items.Length;
			for (int i = 0; i < length; i++)
			{
				if (items[i] != null)
				{
					flag |= items[i].UpdateAnimation();
				}
			}
			return flag;
		}

		void IUIAnimationManagedController.UpdateTemplate(UIAnimationTemplate template)
		{
			if (this.template == template)
			{
				ApplyTemplate(this.template);
			}
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (template != null)
			{
				if (template.Deserialized)
				{
					ApplyTemplate(template);
				}
				else
				{
					template.OnAfterDeserialization += HACK_ApplyTemplate;
				}
				return;
			}
			int length = items.Length;
			for (int i = 0; i < length; i++)
			{
				if (items[i] == null)
				{
					Diagnostics.LogWarning("Null AnimationItem found...");
				}
				else
				{
					items[i].Initialize(this, FindTarget(i));
				}
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		private void HACK_ApplyTemplate()
		{
			ApplyTemplate(template);
		}
	}
}
