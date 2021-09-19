namespace Amplitude.UI.Traits
{
	public interface IUITraitMaterialModifier : IUITrait<float>
	{
		float GetFloat(StaticString id);

		void SetValue(StaticString id, float value);
	}
}
