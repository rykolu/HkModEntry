using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UITransform))]
	public class UIThreeStatesToggle : UIToggle, IUIThreeStatesToggle, IUIToggle, IUIControl, IUIInteractable
	{
		[SerializeField]
		[Tooltip("Neither on or off. Once specified, the control will behave like a regular Toggle.\nIts first state will be the one defined when unspecified is set to false.\nSwitch will be triggered at that moment.")]
		private bool unspecified = true;

		public bool Unspecified
		{
			get
			{
				return unspecified;
			}
			set
			{
				if (unspecified != value)
				{
					unspecified = value;
					ThreeStatesToggleResponder.UpdateReactivity();
				}
			}
		}

		public override bool State
		{
			get
			{
				return base.State;
			}
			set
			{
				unspecified = false;
				base.State = value;
			}
		}

		private UIThreeStatesToggleResponder ThreeStatesToggleResponder => (UIThreeStatesToggleResponder)base.Responder;

		protected override IUIResponder InstantiateResponder()
		{
			return new UIThreeStatesToggleResponder(this);
		}
	}
}
