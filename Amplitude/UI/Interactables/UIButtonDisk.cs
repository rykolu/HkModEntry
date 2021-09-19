using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public class UIButtonDisk : UIButton
	{
		public override bool Contains(Vector2 pointPosition)
		{
			return UIInteractable.IsContainedWithinInscribedEllipse(UITransform, pointPosition);
		}
	}
}
