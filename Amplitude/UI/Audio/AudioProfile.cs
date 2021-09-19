using System;
using Amplitude.Wwise.Audio;
using UnityEngine;

namespace Amplitude.UI.Audio
{
	[Serializable]
	public class AudioProfile : ScriptableObject
	{
		[Serializable]
		public struct Pair
		{
			[NonSerialized]
			public StaticString Key;

			[SerializeField]
			public AudioEventHandleReference AudioHandleReference;

			[SerializeField]
			private string serializableKey;

			public Pair(StaticString key)
			{
				Key = key;
				AudioHandleReference = default(AudioEventHandleReference);
				serializableKey = key.ToString();
			}

			internal void OnEnable()
			{
				Key = new StaticString(serializableKey);
			}
		}

		[SerializeField]
		public Pair[] Pairs;

		public void Initialize(params StaticString[] events)
		{
			int num = ((events != null) ? events.Length : 0);
			Pairs = new Pair[num];
			for (int i = 0; i < num; i++)
			{
				Pairs[i] = new Pair(events[i]);
			}
		}

		internal void OnEnable()
		{
			if (Pairs != null)
			{
				for (int i = 0; i < Pairs.Length; i++)
				{
					Pairs[i].OnEnable();
				}
			}
		}
	}
}
