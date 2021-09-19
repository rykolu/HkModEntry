using System;
using Amplitude.Wwise.Audio;
using UnityEngine;

namespace Amplitude.UI.Audio
{
	[Serializable]
	public struct AudioController
	{
		[SerializeField]
		public AudioProfile Profile;

		public bool Verified => Profile != null;

		public bool TryTriggerEvent(StaticString interactivityEvent)
		{
			if (Profile != null)
			{
				IAudioService audioService = UIServiceAccessManager.AudioService;
				for (int i = 0; i < Profile.Pairs.Length; i++)
				{
					if (Profile.Pairs[i].Key == interactivityEvent)
					{
						if (Profile.Pairs[i].AudioHandleReference.Value == null)
						{
							Diagnostics.LogWarning($"AudioProfile '{Profile.name}': Event '{interactivityEvent}' has no AudioHandle associated.");
							return false;
						}
						audioService?.Post2DEvent(Profile.Pairs[i].AudioHandleReference.Value);
						return true;
					}
				}
			}
			return false;
		}
	}
}
