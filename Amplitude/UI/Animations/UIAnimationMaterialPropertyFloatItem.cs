using Amplitude.UI.Traits;

namespace Amplitude.UI.Animations
{
	public class UIAnimationMaterialPropertyFloatItem : UIAnimationItem<float, UIAnimationFloatInterpolator, IUITraitMaterialModifier>
	{
		public override bool OncePerController => false;

		public string PropertyKey
		{
			get
			{
				return parameters.PropertyKey;
			}
			set
			{
				parameters.PropertyKey = value;
			}
		}

		public override string GetShortName()
		{
			return "Material Property";
		}

		protected override void InitValues()
		{
			if (!string.IsNullOrEmpty(parameters.PropertyKey))
			{
				interpolator.Min = target.GetFloat(new StaticString(parameters.PropertyKey));
				interpolator.Max = interpolator.Min;
			}
			else
			{
				interpolator.Min = 0f;
				interpolator.Max = 0f;
			}
		}

		protected override void Apply(float value)
		{
			target.SetValue(new StaticString(parameters.PropertyKey), value);
		}
	}
}
