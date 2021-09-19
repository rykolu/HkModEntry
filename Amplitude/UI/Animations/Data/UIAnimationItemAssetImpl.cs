using UnityEngine;

namespace Amplitude.UI.Animations.Data
{
	public class UIAnimationItemAssetImpl<TValue> : UIAnimationItemAsset
	{
		[SerializeField]
		public TValue Min;

		[SerializeField]
		public TValue Max;
	}
}
