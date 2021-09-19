using System;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public interface IUIDragArea : IUIControl, IUIInteractable
	{
		bool AutoMove { get; set; }

		bool CancelOnEscape { get; set; }

		bool CancelOnOtherButtonClick { get; set; }

		float DragThreshold { get; set; }

		bool IsDragging { get; }

		event Action<IUIDragArea, Vector2> DragStart;

		event Action<IUIDragArea, Vector2, Vector2> DragMove;

		event Action<IUIDragArea, Vector2> DragComplete;

		event Action<IUIDragArea, Vector2> DragCancel;

		void ForceDragStart(ref InputEvent inputEvent);

		void ForceDragCancel(ref InputEvent inputEvent);
	}
}
