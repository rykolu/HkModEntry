using System;

namespace Amplitude.UI.Animations
{
	public interface IUIAnimationControllerInspectable : IUIAnimationController
	{
		bool IsInitialized { get; }

		UIAnimationTemplate Template { get; }

		IUIAnimationItem CreateItem(Type type, UIComponent target = null);

		void SwapItems(int index1, int index2);

		void RemoveItem(int index);

		void ApplyTemplate(UIAnimationTemplate template);
	}
}
