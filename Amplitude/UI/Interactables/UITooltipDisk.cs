using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public class UITooltipDisk : UITooltip
	{
		public override bool Contains(Vector2 pointPosition)
		{
			return UIInteractable.IsContainedWithinInscribedEllipse(UITransform, pointPosition);
		}
	}
}
