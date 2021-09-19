using UnityEngine;

namespace Amplitude.UI.Animations
{
	[ExecuteInEditMode]
	public class UIAnimationComponent : UIComponent
	{
		[SerializeField]
		private UIAnimationMultiController animationController = new UIAnimationMultiController();

		public IUIAnimationController Controller => animationController;

		internal UIAnimationMultiController MultiController => animationController;

		protected override void Load()
		{
			base.Load();
			UITransform.VisibleGloballyChange += OnVisibleGlobalyChanged;
			OnVisibleGlobalyChanged(UITransform.VisibleGlobally);
		}

		protected override void Unload()
		{
			OnVisibleGlobalyChanged(visible: false);
			UITransform.VisibleGloballyChange -= OnVisibleGlobalyChanged;
			base.Unload();
		}

		private void OnVisibleGlobalyChanged(bool visible)
		{
			if (visible)
			{
				animationController.StartAnimations(forward: true, autoTriggerOnly: true);
			}
			else
			{
				animationController.StopAnimations();
			}
		}
	}
}
