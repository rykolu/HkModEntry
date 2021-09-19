namespace Amplitude.UI.Interactables
{
	public class UIThreeStatesToggleResponder : UIToggleResponder
	{
		private IUIThreeStatesToggle ThreeStatesToggle => base.Interactable as IUIThreeStatesToggle;

		public UIThreeStatesToggleResponder(IUIThreeStatesToggle threeStateToggle)
			: base(threeStateToggle)
		{
		}

		protected override void OnMouseDown(ref InputEvent mouseEvent, bool isMouseInside)
		{
			if (mouseEvent.Button != 0)
			{
				return;
			}
			if (ThreeStatesToggle.Unspecified)
			{
				if (!ThreeStatesToggle.HandleOnMouseUp)
				{
					ThreeStatesToggle.Unspecified = false;
					OnStateSwitch();
					UpdateReactivity();
				}
			}
			else
			{
				base.OnMouseDown(ref mouseEvent, isMouseInside);
			}
		}

		protected override void OnMouseUp(ref InputEvent mouseEvent, bool isMouseInside)
		{
			if (mouseEvent.Button != 0)
			{
				return;
			}
			if (ThreeStatesToggle.Unspecified)
			{
				if (ThreeStatesToggle.HandleOnMouseUp)
				{
					ThreeStatesToggle.Unspecified = false;
					OnStateSwitch();
					UpdateReactivity();
				}
			}
			else
			{
				base.OnMouseUp(ref mouseEvent, isMouseInside);
			}
		}

		protected override void DoUpdateReactivity(ref UIReactivityState reactivityState)
		{
			if (!base.IsInteractive)
			{
				reactivityState.Add(UIReactivityState.Key.Disabled);
			}
			if (ThreeStatesToggle.Unspecified)
			{
				reactivityState.Add(UIReactivityState.Key.Unspecified);
			}
			else if (ThreeStatesToggle.State)
			{
				reactivityState.Add(UIReactivityState.Key.On);
			}
			if (hovered)
			{
				reactivityState.Add(UIReactivityState.Key.Hover);
			}
		}
	}
}
