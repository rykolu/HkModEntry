namespace Amplitude.UI.Animations
{
	public interface IUIAnimationManagedController
	{
		bool Active { get; }

		void SetActive(bool value);

		bool UpdateAnimation();

		void UpdateTemplate(UIAnimationTemplate template);
	}
}
