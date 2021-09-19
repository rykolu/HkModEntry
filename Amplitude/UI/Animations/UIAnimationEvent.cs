using System;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public class UIAnimationEvent : IComparable
	{
		[SerializeField]
		private int keyIndex = -1;

		[SerializeField]
		private float triggerTime = -1f;

		[SerializeField]
		private bool isSound;

		[SerializeField]
		private string serializableData = string.Empty;

		[NonSerialized]
		private StaticString data = StaticString.Empty;

		public int KeyIndex => keyIndex;

		public float Time
		{
			get
			{
				return triggerTime;
			}
			set
			{
				if (!Mathf.Approximately(triggerTime, value))
				{
					triggerTime = value;
					keyIndex = -1;
				}
			}
		}

		public StaticString Data
		{
			get
			{
				if (StaticString.IsNullOrEmpty(data))
				{
					data = new StaticString(serializableData);
				}
				return data;
			}
		}

		public string SerializableData
		{
			get
			{
				return serializableData;
			}
			set
			{
				if (serializableData != value)
				{
					serializableData = value;
					data = StaticString.Empty;
				}
			}
		}

		public bool IsSound
		{
			get
			{
				return isSound;
			}
			set
			{
				isSound = value;
			}
		}

		public void SetTriggerTime(AnimationCurve curve, int keyIndex)
		{
			if (keyIndex >= 0 && keyIndex < curve.length)
			{
				this.keyIndex = keyIndex;
				triggerTime = curve[keyIndex].time;
			}
			else
			{
				this.keyIndex = -1;
			}
		}

		int IComparable.CompareTo(object obj)
		{
			UIAnimationEvent uIAnimationEvent = obj as UIAnimationEvent;
			if (uIAnimationEvent != null)
			{
				return triggerTime.CompareTo(uIAnimationEvent.triggerTime);
			}
			return 0;
		}
	}
}
