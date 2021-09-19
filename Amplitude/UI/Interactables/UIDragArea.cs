using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UITransform))]
	public class UIDragArea : UIControl, IUIDragArea, IUIControl, IUIInteractable
	{
		[SerializeField]
		[Tooltip("Move the UITransform under the mouse when dragging.")]
		private bool autoMove;

		[SerializeField]
		[Tooltip("Cancel the drag if the Escape Key is pressed when dragging.")]
		private bool cancelOnEscape;

		[SerializeField]
		[FormerlySerializedAs("cancelOnRightClick")]
		[Tooltip("Cancel the drag if the Right or Middle Click Button is pressed when dragging.")]
		private bool cancelOnOtherButtonClick;

		[SerializeField]
		[Tooltip("The number of pixels beyond which the drag will start.")]
		private float dragThreshold = 3f;

		public bool AutoMove
		{
			get
			{
				return autoMove;
			}
			set
			{
				autoMove = value;
			}
		}

		public bool CancelOnEscape
		{
			get
			{
				return cancelOnEscape;
			}
			set
			{
				cancelOnEscape = value;
			}
		}

		public bool CancelOnOtherButtonClick
		{
			get
			{
				return cancelOnOtherButtonClick;
			}
			set
			{
				cancelOnOtherButtonClick = value;
			}
		}

		public float DragThreshold
		{
			get
			{
				return dragThreshold;
			}
			set
			{
				dragThreshold = value;
			}
		}

		public bool IsDragging => DragAreaResponder.IsDragging;

		private UIDragAreaResponder DragAreaResponder => (UIDragAreaResponder)base.Responder;

		public event Action<IUIDragArea, Vector2> DragStart
		{
			add
			{
				DragAreaResponder.DragStart += value;
			}
			remove
			{
				DragAreaResponder.DragStart -= value;
			}
		}

		public event Action<IUIDragArea, Vector2, Vector2> DragMove
		{
			add
			{
				DragAreaResponder.DragMove += value;
			}
			remove
			{
				DragAreaResponder.DragMove -= value;
			}
		}

		public event Action<IUIDragArea, Vector2> DragComplete
		{
			add
			{
				DragAreaResponder.DragComplete += value;
			}
			remove
			{
				DragAreaResponder.DragComplete -= value;
			}
		}

		public event Action<IUIDragArea, Vector2> DragCancel
		{
			add
			{
				DragAreaResponder.DragCancel += value;
			}
			remove
			{
				DragAreaResponder.DragCancel -= value;
			}
		}

		public void ForceDragStart(ref InputEvent inputEvent)
		{
			DragAreaResponder.ForceDragStart(ref inputEvent);
		}

		public void ForceDragCancel(ref InputEvent inputEvent)
		{
			DragAreaResponder.ForceDragCancel(ref inputEvent);
		}

		protected override IUIResponder InstantiateResponder()
		{
			return new UIDragAreaResponder(this);
		}
	}
}
