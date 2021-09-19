using UnityEngine;

namespace Amplitude.UI.Animations.Data
{
	public abstract class UIAnimationItemAsset : ScriptableObject
	{
		[SerializeField]
		public float Duration;

		[SerializeField]
		public float StartTime;

		[SerializeField]
		public AnimationCurve Curve;

		private StaticString cachedName = StaticString.Empty;

		public StaticString Name
		{
			get
			{
				return cachedName;
			}
			internal set
			{
				cachedName = value;
				base.name = value.ToString();
			}
		}

		internal void Load()
		{
			if (StaticString.IsNullOrEmpty(cachedName))
			{
				cachedName = new StaticString(base.name);
			}
		}

		internal void Unload()
		{
		}
	}
}
