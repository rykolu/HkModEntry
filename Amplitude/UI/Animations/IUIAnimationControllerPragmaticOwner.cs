namespace Amplitude.UI.Animations
{
	public interface IUIAnimationControllerPragmaticOwner : IUIAnimationControllerOwner
	{
		bool HasAnimationController { get; }

		void CreateAnimationController();

		void ClearAnimationController(bool onlyIfNecessary);
	}
}
