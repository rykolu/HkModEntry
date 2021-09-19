using System;
using Amplitude.UI.Renderers;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	public interface IUITextField : IUIControl, IUIInteractable
	{
		UILabel Label { get; }

		string Text { get; }

		int MaximumChars { get; }

		bool Multiline { get; }

		int SelectionStartIndex { get; }

		int CaretIndex { get; }

		UITextFieldFocusAction ActionOnFocus { get; }

		UITextFieldKeyAction ActionOnReturn { get; }

		UITextFieldKeyAction ActionOnEscape { get; }

		UITextFieldKeyAction ActionOnTab { get; }

		bool KeepFocus { get; }

		bool ExtendSelection { get; set; }

		bool HasSelection { get; }

		event Action<IUITextField, string> TextChange;

		event Action<IUITextField, string> TextValidation;

		void OnEditionStarted();

		void OnEditionFinished();

		void SelectText(int beginIndex, int endIndex, bool force = false);

		void SelectWholeText(bool force = false);

		void MoveCaretAtPosition(Vector2 position);

		void MoveCaretToCharIndex(int position, bool force = false);

		void MoveCaretToTextBeginning(bool force = false);

		void MoveCaretToTextEnd(bool force = false);

		void MoveCaretToLineBeginning(bool force = false);

		void MoveCaretToLineEnd(bool force = false);

		void AdvanceCaretCharwise(UITextFieldDirection direction);

		void AdvanceCaretWordwise(UITextFieldDirection direction);

		void MoveCaretOneLineUp(bool force = false);

		void MoveCaretOneLineDown(bool force = false);

		bool TryInsertChar(char newChar);

		bool TryInsertString(string newString);

		bool TryRemoveChar(UITextFieldDirection direction);

		bool TryRemoveWord(UITextFieldDirection direction);

		bool TryRemoveSelection();

		string GetSelectedText();

		void ReplaceText(string newText);
	}
}
