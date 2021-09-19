namespace Amplitude.UI.Audio
{
	public interface IAudioControllerOwner
	{
		AudioController AudioController { get; }

		void InitializeAudioProfile(AudioProfile audioProfile);
	}
}
