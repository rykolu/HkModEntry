using System;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[Serializable]
	public struct UIAnimationItemParams
	{
		[SerializeField]
		public AnimationCurve Curve;

		[SerializeField]
		public float Duration;

		[SerializeField]
		public bool Repeat;

		[SerializeField]
		public float Delay;

		[SerializeField]
		public float ReverseDelay;

		[SerializeField]
		public bool AutoTrigger;

		[SerializeField]
		public UIAnimationEventsSet Events;

		[SerializeField]
		public string PropertyKey;
	}
}
