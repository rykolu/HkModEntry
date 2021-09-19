using System;
using Amplitude.Framework.Input;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public class UIDragAreaResponder : UIControlResponder
	{
		private bool isDragging;

		private bool leftMousePressed;

		private Vector2 armingPosition = Vector2.zero;

		private Vector2 lastFramePosition = Vector2.zero;

		public IUIDragArea DragArea => base.Interactable as IUIDragArea;

		public bool IsDragging => isDragging;

		public event Action<IUIDragArea, Vector2> DragStart;

		public event Action<IUIDragArea, Vector2, Vector2> DragMove;

		public event Action<IUIDragArea, Vector2> DragComplete;

		public event Action<IUIDragArea, Vector2> DragCancel;

		public UIDragAreaResponder(IUIDragArea dragArea)
			: base(dragArea)
		{
		}

		public override void ResetState()
		{
			base.ResetState();
			isDragging = false;
			leftMousePressed = false;
			armingPosition = Vector2.zero;
			lastFramePosition = Vector2.zero;
		}

		public void ForceDragStart(ref InputEvent inputEvent)
		{
			if (base.IsInteractive)
			{
				OnDragArmed(ref inputEvent);
				OnDragStart(ref inputEvent);
			}
		}

		public void ForceDragComplete(ref InputEvent inputEvent)
		{
			if (base.IsInteractive)
			{
				OnDragComplete(ref inputEvent);
			}
		}

		public void ForceDragCancel(ref InputEvent inputEvent)
		{
			if (base.IsInteractive)
			{
				OnDragCancel(ref inputEvent);
			}
		}

		protected override void OnTick(ref InputEvent tickEvent)
		{
			base.OnTick(ref tickEvent);
			Vector2 distanceSinceLastFrame = (isDragging ? (tickEvent.MousePosition - lastFramePosition) : Vector2.zero);
			if (leftMousePressed && !isDragging)
			{
				Vector2 vector = tickEvent.MousePosition - armingPosition;
				if (vector.magnitude > DragArea.DragThreshold)
				{
					OnDragStart(ref tickEvent);
					distanceSinceLastFrame = vector;
				}
			}
			if (isDragging && distanceSinceLastFrame.magnitude > 0f)
			{
				OnDragMove(ref tickEvent, distanceSinceLastFrame);
				lastFramePosition = tickEvent.MousePosition;
			}
		}

		protected override void OnMouseDown(ref InputEvent mouseEvent, bool isMouseInside)
		{
			base.OnMouseDown(ref mouseEvent, isMouseInside);
			if (mouseEvent.Button == MouseButton.Left)
			{
				OnDragArmed(ref mouseEvent);
			}
			else if (isDragging && DragArea.CancelOnOtherButtonClick)
			{
				OnDragCancel(ref mouseEvent);
			}
		}

		protected override void OnMouseUp(ref InputEvent mouseEvent, bool isMouseInside)
		{
			base.OnMouseUp(ref mouseEvent, isMouseInside);
			if (mouseEvent.Button == MouseButton.Left)
			{
				if (isDragging)
				{
					OnDragComplete(ref mouseEvent);
				}
				else if (leftMousePressed)
				{
					ResetState();
					UpdateReactivity();
				}
			}
		}

		protected override void OnKeyDown(ref InputEvent keyboardEvent)
		{
			base.OnKeyDown(ref keyboardEvent);
			KeyCode key = (KeyCode)keyboardEvent.Key;
			if (DragArea.CancelOnEscape && key == KeyCode.Escape)
			{
				OnDragCancel(ref keyboardEvent);
			}
		}

		protected virtual void OnDragArmed(ref InputEvent mouseEvent)
		{
			leftMousePressed = true;
			armingPosition = mouseEvent.MousePosition;
			lastFramePosition = mouseEvent.MousePosition;
			UpdateReactivity();
		}

		protected virtual void OnDragStart(ref InputEvent tickEvent)
		{
			if (leftMousePressed)
			{
				isDragging = true;
				this.DragStart?.Invoke(DragArea, tickEvent.MousePosition);
				UpdateReactivity();
			}
		}

		protected virtual void OnDragMove(ref InputEvent tickEvent, Vector2 distanceSinceLastFrame)
		{
			this.DragMove?.Invoke(DragArea, tickEvent.MousePosition, distanceSinceLastFrame);
			if (DragArea.AutoMove)
			{
				DragArea.GetUITransform().Rect = new Rect(DragArea.GetUITransform().Rect.x + distanceSinceLastFrame.x, DragArea.GetUITransform().Rect.y + distanceSinceLastFrame.y, DragArea.GetUITransform().Rect.width, DragArea.GetUITransform().Rect.height);
			}
		}

		protected virtual void OnDragComplete(ref InputEvent mouseEvent)
		{
			if (isDragging)
			{
				ResetState();
				this.DragComplete?.Invoke(DragArea, mouseEvent.MousePosition);
				UpdateReactivity();
			}
		}

		protected virtual void OnDragCancel(ref InputEvent mouseEvent)
		{
			if (isDragging && base.IsInteractive)
			{
				ResetState();
				this.DragCancel?.Invoke(DragArea, mouseEvent.MousePosition);
				UpdateReactivity();
			}
		}

		protected override void DoUpdateReactivity(ref UIReactivityState reactivityState)
		{
			if (!base.IsInteractive)
			{
				reactivityState.Add(UIReactivityState.Key.Disabled);
				return;
			}
			if (hovered)
			{
				reactivityState.Add(UIReactivityState.Key.Hover);
			}
			if (leftMousePressed)
			{
				reactivityState.Add(UIReactivityState.Key.Pressed);
			}
		}
	}
}
