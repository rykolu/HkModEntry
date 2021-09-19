using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public interface IUIInteractable
	{
		bool IsVisible { get; }

		bool IsInteractive { get; }

		bool IsMask { get; }

		bool Hovered { get; }

		bool Loaded { get; }

		bool Contains(Vector2 standardizedPosition);

		bool TryCatchEvent(ref InputEvent inputEvent);

		bool TryCatchEvent(ref InputEvent inputEvent, Vector2 mouseScreenPosition);

		SortedResponder GetSortedResponder(UIView view, int viewGroupCullingMask, IUIResponder responder);

		UITransform GetUITransform();
	}
}
