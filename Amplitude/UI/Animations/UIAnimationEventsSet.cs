using System;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationEventsSet
	{
		[SerializeField]
		private UIAnimationEvent[] events = new UIAnimationEvent[0];

		private int lastTriggeredIndex = -1;

		public int Length
		{
			get
			{
				if (events == null)
				{
					return 0;
				}
				return events.Length;
			}
		}

		internal UIAnimationEvent this[int index] => events[index];

		public void TryTrigger(IUIAnimationEventsSetClient client, float ratio, bool forward)
		{
			if (events == null || events.Length == 0)
			{
				return;
			}
			if (forward)
			{
				for (int i = lastTriggeredIndex + 1; i < events.Length; i++)
				{
					UIAnimationEvent uIAnimationEvent = events[i];
					if (uIAnimationEvent.Time <= ratio)
					{
						client.TriggerEvent(uIAnimationEvent);
						lastTriggeredIndex++;
						continue;
					}
					break;
				}
				return;
			}
			if (lastTriggeredIndex < 0)
			{
				lastTriggeredIndex = events.Length;
			}
			int num = lastTriggeredIndex - 1;
			while (num >= 0)
			{
				UIAnimationEvent uIAnimationEvent2 = events[num];
				if (uIAnimationEvent2.Time >= ratio)
				{
					client.TriggerEvent(uIAnimationEvent2);
					lastTriggeredIndex--;
					num--;
					continue;
				}
				break;
			}
		}

		public void ResetTriggered()
		{
			lastTriggeredIndex = -1;
		}

		public void CheckCurve(AnimationCurve curve)
		{
		}

		public void SortEvents()
		{
			if (events != null)
			{
				Array.Sort(events);
			}
		}

		public void AddEvent()
		{
			int length = Length;
			UIAnimationEvent[] array = new UIAnimationEvent[length + 1];
			if (events != null)
			{
				Array.Copy(events, array, length);
			}
			array[length] = new UIAnimationEvent();
			events = array;
		}

		public void RemoveEvent(int removedIndex)
		{
			int length = Length;
			UIAnimationEvent[] array = new UIAnimationEvent[length - 1];
			if (events != null)
			{
				int num = 0;
				for (int i = 0; i < length; i++)
				{
					if (i != removedIndex)
					{
						array[num++] = events[i];
					}
				}
			}
			events = array;
		}
	}
}
