using System.Collections.Generic;
using Amplitude.UI.Audio;
using UnityEngine;

namespace Amplitude.UI.Animations.Data
{
	public class UIAnimationAsset : ScriptableObject, IAudioControllerOwner
	{
		[SerializeField]
		public UIAnimationItemAsset[] Items;

		[SerializeField]
		public UIAnimationLoopMode LoopMode;

		[SerializeField]
		public bool AutoTrigger;

		[SerializeField]
		public float DurationFactor = 1f;

		[SerializeField]
		public UIAnimationEvent[] Events;

		[SerializeField]
		public AudioController AudioController;

		AudioController IAudioControllerOwner.AudioController => AudioController;

		void IAudioControllerOwner.InitializeAudioProfile(AudioProfile audioProfile)
		{
			List<StaticString> list = new List<StaticString>();
			int num = ((Events != null) ? Events.Length : 0);
			for (int i = 0; i < num; i++)
			{
				UIAnimationEvent uIAnimationEvent = Events[i];
				if (!list.Contains(uIAnimationEvent.Name))
				{
					list.Add(uIAnimationEvent.Name);
				}
			}
			int count = list.Count;
			audioProfile.Pairs = new AudioProfile.Pair[count];
			for (int j = 0; j < count; j++)
			{
				audioProfile.Pairs[j] = new AudioProfile.Pair(list[j]);
			}
		}

		internal void Load()
		{
			UIAnimationItemAsset[] items = Items;
			int num = ((items != null) ? items.Length : 0);
			for (int i = 0; i < num; i++)
			{
				Items[i]?.Load();
			}
			UIAnimationEvent[] events = Events;
			int num2 = ((events != null) ? events.Length : 0);
			for (int j = 0; j < num2; j++)
			{
				Events[j].Load();
			}
		}

		internal void Unload()
		{
			int num = ((Items != null) ? Items.Length : 0);
			for (int i = 0; i < num; i++)
			{
				Items[i]?.Unload();
			}
			int num2 = ((Events != null) ? Events.Length : 0);
			for (int j = 0; j < num2; j++)
			{
				Events[j].Unload();
			}
		}

		private void OnEnable()
		{
			Load();
		}

		private void OnDisable()
		{
			Unload();
		}

		private void OnValidate()
		{
			Load();
		}
	}
}
