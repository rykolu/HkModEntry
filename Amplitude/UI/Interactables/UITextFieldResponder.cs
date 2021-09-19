using System;
using Amplitude.Framework.Input;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public class UITextFieldResponder : UIControlResponder
	{
		private bool leftMousePressed;

		private Vector2 lastFrameMousePosition = Vector2.zero;

		private bool moveCaretWithLeftClickOnFocus = true;

		private IUITextField TextField => base.Interactable as IUITextField;

		public event Action<IUITextField, string> TextChange;

		public event Action<IUITextField, string> TextValidation;

		public event Action<IUITextField, string> TextCancellation;

		public UITextFieldResponder(IUITextField textField)
			: base(textField)
		{
		}

		public override void ResetState()
		{
			base.ResetState();
			leftMousePressed = false;
			TextField.ExtendSelection = false;
			if (UIInteractivityManager.Instance.FocusedResponder == this)
			{
				UIInteractivityManager.Instance.SetFocus();
			}
			TextField.OnEditionFinished();
		}

		internal override void OnFocusGain()
		{
			base.OnFocusGain();
			if (TextField.IsInteractive)
			{
				UIInteractivityManager.Instance.AcquireKeyboard(TextField);
				TextField.OnEditionStarted();
			}
			if (TextField.ActionOnFocus != 0)
			{
				moveCaretWithLeftClickOnFocus = false;
			}
			UpdateReactivity();
		}

		internal override void OnFocusLoss()
		{
			base.OnFocusLoss();
			UIInteractivityManager.Instance.ReleaseKeyboard();
			ResetState();
			UpdateReactivity();
		}

		protected override void OnTick(ref InputEvent tickEvent)
		{
			base.OnTick(ref tickEvent);
			if (leftMousePressed && tickEvent.MousePosition != lastFrameMousePosition)
			{
				TextField.ExtendSelection = true;
				TextField.MoveCaretAtPosition(tickEvent.MousePosition);
			}
			lastFrameMousePosition = tickEvent.MousePosition;
		}

		protected override void OnMouseDown(ref InputEvent mouseEvent, bool isMouseInside)
		{
			base.OnMouseDown(ref mouseEvent, isMouseInside);
			if (!base.IsInteractive || mouseEvent.Button != 0)
			{
				return;
			}
			if (!moveCaretWithLeftClickOnFocus)
			{
				moveCaretWithLeftClickOnFocus = true;
				return;
			}
			leftMousePressed = true;
			bool extendSelection = ((ushort)mouseEvent.CustomData & 8) != 0;
			TextField.ExtendSelection = extendSelection;
			if (mouseEvent.Button == MouseButton.Left)
			{
				TextField.MoveCaretAtPosition(mouseEvent.MousePosition);
			}
		}

		protected override void OnMouseUp(ref InputEvent mouseEvent, bool isMouseInside)
		{
			base.OnMouseUp(ref mouseEvent, isMouseInside);
			leftMousePressed = false;
		}

		protected override void OnKeyDown(ref InputEvent keyboardEvent)
		{
			base.OnKeyDown(ref keyboardEvent);
			KeyCode key = (KeyCode)keyboardEvent.Key;
			ushort num = (ushort)keyboardEvent.CustomData;
			bool flag = (num & 8) != 0;
			bool flag2 = (num & 4) != 0;
			TextField.ExtendSelection = flag;
			switch (key)
			{
			case KeyCode.Return:
			case KeyCode.KeypadEnter:
				OnReturnKeyDown(flag);
				return;
			case KeyCode.Escape:
				OnEscapeKeyDown();
				return;
			case KeyCode.Tab:
				OnTabKeyDown();
				return;
			case KeyCode.Home:
				OnHomeKeyDown(flag2);
				return;
			case KeyCode.End:
				OnEndKeyDown(flag2);
				return;
			case KeyCode.RightArrow:
				OnRightArrowKeyDown(flag, flag2);
				return;
			case KeyCode.LeftArrow:
				OnLeftArrowKeyDown(flag, flag2);
				return;
			case KeyCode.UpArrow:
				OnUpArrowKeyDown(flag2);
				return;
			case KeyCode.DownArrow:
				OnDownArrowKeyDown(flag2);
				return;
			case KeyCode.Backspace:
			case KeyCode.Delete:
				OnDeleteOrBackspaceKeyDown(key, flag2);
				return;
			}
			if (key == KeyCode.A && flag2)
			{
				OnSelectWholeText();
			}
			else if (key == KeyCode.C && flag2)
			{
				OnCopyText();
			}
			else if (key == KeyCode.X && flag2)
			{
				OnCutText();
			}
			else if (key == KeyCode.V && flag2)
			{
				OnPasteText();
			}
		}

		protected override void OnNewChar(ref InputEvent keyboardEvent)
		{
			base.OnNewChar(ref keyboardEvent);
			char c = (char)keyboardEvent.Key;
			if (c != 0 && c != '\r' && c != '\n' && c != '\b' && c != '\u007f')
			{
				_ = (ushort)keyboardEvent.CustomData;
				TextField.ExtendSelection = false;
				if (TextField.TryInsertChar(c))
				{
					this.TextChange?.Invoke(TextField, TextField.Text);
				}
			}
		}

		protected virtual void OnReturnKeyDown(bool hasShiftModifier)
		{
			if (TextField.ActionOnReturn == UITextFieldKeyAction.Nothing)
			{
				return;
			}
			if (TextField.ActionOnReturn == UITextFieldKeyAction.Default || (TextField.Multiline && hasShiftModifier))
			{
				if (TextField.TryInsertChar('\n'))
				{
					OnTextChange();
				}
				return;
			}
			if (TextField.ActionOnReturn == UITextFieldKeyAction.Validate)
			{
				OnTextValidation();
			}
			if (TextField.ActionOnReturn == UITextFieldKeyAction.Cancel)
			{
				OnTextCancellation();
			}
			if (TextField.KeepFocus)
			{
				leftMousePressed = false;
				TextField.ExtendSelection = false;
			}
			else
			{
				ResetState();
			}
		}

		protected virtual void OnEscapeKeyDown()
		{
			if (TextField.ActionOnEscape != UITextFieldKeyAction.Nothing && TextField.ActionOnEscape != 0)
			{
				if (TextField.ActionOnEscape == UITextFieldKeyAction.Validate)
				{
					OnTextValidation();
				}
				if (TextField.ActionOnEscape == UITextFieldKeyAction.Cancel)
				{
					OnTextCancellation();
				}
				ResetState();
			}
		}

		protected virtual void OnTabKeyDown()
		{
			if (TextField.ActionOnTab == UITextFieldKeyAction.Nothing)
			{
				return;
			}
			if (TextField.ActionOnTab == UITextFieldKeyAction.Default)
			{
				if (TextField.TryInsertChar('\t'))
				{
					OnTextChange();
				}
				return;
			}
			if (TextField.ActionOnTab == UITextFieldKeyAction.Validate)
			{
				OnTextValidation();
			}
			if (TextField.ActionOnTab == UITextFieldKeyAction.Cancel)
			{
				OnTextCancellation();
			}
			ResetState();
		}

		protected virtual void OnHomeKeyDown(bool hasControlModifier)
		{
			if (TextField.Multiline)
			{
				if (hasControlModifier)
				{
					TextField.MoveCaretToTextBeginning();
				}
				else
				{
					TextField.MoveCaretToLineBeginning();
				}
			}
			else
			{
				TextField.MoveCaretToTextBeginning();
			}
		}

		protected virtual void OnEndKeyDown(bool hasControlModifier)
		{
			if (TextField.Multiline)
			{
				if (hasControlModifier)
				{
					TextField.MoveCaretToTextEnd();
				}
				else
				{
					TextField.MoveCaretToLineEnd();
				}
			}
			else
			{
				TextField.MoveCaretToTextEnd();
			}
		}

		protected virtual void OnRightArrowKeyDown(bool hasShiftModifier, bool hasControlModifier)
		{
			if (hasControlModifier)
			{
				TextField.AdvanceCaretWordwise(UITextFieldDirection.Right);
			}
			else if (TextField.HasSelection && !hasShiftModifier)
			{
				int position = ((TextField.SelectionStartIndex > TextField.CaretIndex) ? TextField.SelectionStartIndex : TextField.CaretIndex);
				TextField.ExtendSelection = false;
				TextField.MoveCaretToCharIndex(position, force: true);
			}
			else
			{
				TextField.AdvanceCaretCharwise(UITextFieldDirection.Right);
			}
		}

		protected virtual void OnLeftArrowKeyDown(bool hasShiftModifier, bool hasControlModifier)
		{
			if (hasControlModifier)
			{
				TextField.AdvanceCaretWordwise(UITextFieldDirection.Left);
			}
			else if (TextField.HasSelection && !hasShiftModifier)
			{
				int position = ((TextField.SelectionStartIndex < TextField.CaretIndex) ? TextField.SelectionStartIndex : TextField.CaretIndex);
				TextField.ExtendSelection = false;
				TextField.MoveCaretToCharIndex(position, force: true);
			}
			else
			{
				TextField.AdvanceCaretCharwise(UITextFieldDirection.Left);
			}
		}

		protected virtual void OnUpArrowKeyDown(bool hasControlModifier)
		{
			TextField.MoveCaretOneLineUp();
		}

		protected virtual void OnDownArrowKeyDown(bool hasControlModifier)
		{
			TextField.MoveCaretOneLineDown();
		}

		protected virtual void OnDeleteOrBackspaceKeyDown(KeyCode keyCode, bool hasControlModifier)
		{
			TextField.ExtendSelection = false;
			bool flag = false;
			if (TextField.HasSelection)
			{
				flag = TextField.TryRemoveSelection();
			}
			else
			{
				UITextFieldDirection direction = ((keyCode != KeyCode.Backspace) ? UITextFieldDirection.Right : UITextFieldDirection.Left);
				flag = ((!hasControlModifier) ? TextField.TryRemoveChar(direction) : TextField.TryRemoveWord(direction));
			}
			if (flag)
			{
				OnTextChange();
			}
		}

		protected virtual void OnSelectWholeText()
		{
			TextField.SelectWholeText();
		}

		protected virtual void OnCopyText()
		{
			if (TextField.HasSelection)
			{
				GUIUtility.systemCopyBuffer = TextField.GetSelectedText();
			}
		}

		protected virtual void OnCutText()
		{
			if (TextField.HasSelection)
			{
				OnCopyText();
				if (TextField.TryRemoveSelection())
				{
					OnTextChange();
				}
			}
		}

		protected virtual void OnPasteText()
		{
			if (!string.IsNullOrEmpty(GUIUtility.systemCopyBuffer) && TextField.TryInsertString(GUIUtility.systemCopyBuffer))
			{
				OnTextChange();
			}
		}

		protected override void DoUpdateReactivity(ref UIReactivityState reactivityState)
		{
			if (!base.IsInteractive)
			{
				reactivityState.Add(UIReactivityState.Key.Disabled);
			}
			else if (hovered)
			{
				reactivityState.Add(UIReactivityState.Key.Hover);
			}
			if (base.Focused)
			{
				reactivityState.Add(UIReactivityState.Key.Focused);
			}
		}

		protected void OnTextChange()
		{
			this.TextChange?.Invoke(TextField, TextField.Text);
		}

		protected void OnTextValidation()
		{
			this.TextValidation?.Invoke(TextField, TextField.Text);
		}

		protected void OnTextCancellation()
		{
			this.TextCancellation?.Invoke(TextField, TextField.Text);
		}

		private void OnExternalTextChange(string newText)
		{
			TextField.ReplaceText(newText);
			OnTextChange();
			OnTextValidation();
			ResetState();
		}

		private void OnExternalTextCancel()
		{
			OnTextCancellation();
			ResetState();
		}
	}
}
