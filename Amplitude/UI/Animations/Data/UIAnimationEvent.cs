using System;
using UnityEngine;

namespace Amplitude.UI.Animations.Data
{
	[Serializable]
	public struct UIAnimationEvent
	{
		public enum Direction
		{
			Both,
			ForwardOnly,
			BackwardOnly
		}

		[Range(0f, 1f)]
		public float Ratio;

		public Direction TriggerDirection;

		[SerializeField]
		private string serializableName;

		public StaticString Name { get; private set; }

		internal void Load()
		{
			Name = new StaticString(serializableName);
		}

		internal void Unload()
		{
		}
	}
}
