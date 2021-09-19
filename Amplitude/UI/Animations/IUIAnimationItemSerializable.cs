namespace Amplitude.UI.Animations
{
	public interface IUIAnimationItemSerializable
	{
		UIAnimationItemParams Parameters { get; set; }

		IUIAnimationInterpolator Interpolator { get; }
	}
}
