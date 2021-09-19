namespace Amplitude.UI
{
	public interface IInterpolator<TValue>
	{
		void Interpolate(TValue origin, TValue target, float ratio, ref TValue result);
	}
}
