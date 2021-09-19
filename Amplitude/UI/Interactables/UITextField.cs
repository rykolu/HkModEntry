using System;
using System.Text;
using Amplitude.Framework.Extensions;
using Amplitude.UI.Animations;
using Amplitude.UI.Renderers;
using Amplitude.UI.Text;
using UnityEngine;

namespace Amplitude.UI.Interactables
{
	[RequireComponent(typeof(UITransform))]
	public class UITextField : UIControl, IUITextField, IUIControl, IUIInteractable
	{
		private const int DefaultMaximumChars = 4096;

		private const char CharIfGlyphUnsupported = '?';

		private static StringBuilder tmpStringBuilder = new StringBuilder();

		[SerializeField]
		private UILabel label;

		[SerializeField]
		[Tooltip("Optional")]
		private UITransform mask;

		[SerializeField]
		private UITransform caret;

		[SerializeField]
		[Tooltip("Optional")]
		private UIAnimationComponent caretAnimation;

		[SerializeField]
		private bool multiline;

		[SerializeField]
		private UITextFieldFocusAction actionOnFocus;

		[SerializeField]
		private UITextFieldKeyAction actionOnReturn = UITextFieldKeyAction.Validate;

		[SerializeField]
		private UITextFieldKeyAction actionOnEscape = UITextFieldKeyAction.Cancel;

		[SerializeField]
		private UITextFieldKeyAction actionOnTab = UITextFieldKeyAction.Validate;

		[SerializeField]
		private bool keepFocus;

		[SerializeField]
		private int maximumChars;

		[SerializeField]
		private string whiteList = string.Empty;

		[SerializeField]
		private string blackList = string.Empty;

		[SerializeField]
		private string instructionText = string.Empty;

		[SerializeField]
		private char passwordChar;

		[NonSerialized]
		private string text = string.Empty;

		[NonSerialized]
		private int selectionStartIndex = -1;

		[NonSerialized]
		private int caretIndex;

		public UILabel Label => label;

		public string Text => text;

		public bool Multiline => multiline;

		public int SelectionStartIndex => selectionStartIndex;

		public int CaretIndex => caretIndex;

		public UITextFieldFocusAction ActionOnFocus => actionOnFocus;

		public UITextFieldKeyAction ActionOnReturn => actionOnReturn;

		public UITextFieldKeyAction ActionOnEscape => actionOnEscape;

		public UITextFieldKeyAction ActionOnTab => actionOnTab;

		public bool KeepFocus => keepFocus;

		public int MaximumChars
		{
			get
			{
				if (maximumChars > 0)
				{
					return maximumChars;
				}
				return 4096;
			}
		}

		public string WhiteList
		{
			get
			{
				return whiteList;
			}
			set
			{
				if (whiteList != value)
				{
					whiteList = value;
					OnWhiteListChanged();
				}
			}
		}

		public string BlackList
		{
			get
			{
				return blackList;
			}
			set
			{
				if (blackList != value)
				{
					blackList = value;
					OnBlackListChanged();
				}
			}
		}

		public string InstructionText
		{
			get
			{
				return instructionText;
			}
			set
			{
				instructionText = value;
				OnInstructionTextChanged();
			}
		}

		public char PasswordChar
		{
			get
			{
				return passwordChar;
			}
			set
			{
				passwordChar = value;
				OnTextChanged();
			}
		}

		public bool ExtendSelection { get; set; }

		public bool HasSelection
		{
			get
			{
				if (selectionStartIndex >= 0)
				{
					return selectionStartIndex != caretIndex;
				}
				return false;
			}
		}

		private UITextFieldResponder TextFieldResponder => (UITextFieldResponder)base.Responder;

		public event Action<IUITextField, string> TextChange
		{
			add
			{
				TextFieldResponder.TextChange += value;
			}
			remove
			{
				TextFieldResponder.TextChange -= value;
			}
		}

		public event Action<IUITextField, string> TextValidation
		{
			add
			{
				TextFieldResponder.TextValidation += value;
			}
			remove
			{
				TextFieldResponder.TextValidation -= value;
			}
		}

		public event Action<IUITextField, string> TextCancellation
		{
			add
			{
				TextFieldResponder.TextCancellation += value;
			}
			remove
			{
				TextFieldResponder.TextCancellation -= value;
			}
		}

		public void OnEditionStarted()
		{
			Input.imeCompositionMode = IMECompositionMode.On;
			UpdateInstructionVisibility();
			switch (ActionOnFocus)
			{
			case UITextFieldFocusAction.PlaceCaretAtCursor:
				MoveCaretToTextEnd(force: true);
				break;
			case UITextFieldFocusAction.CaretAtEnd:
				MoveCaretToTextEnd(force: true);
				break;
			case UITextFieldFocusAction.SelectWholeText:
				SelectWholeText();
				break;
			case UITextFieldFocusAction.DeleteText:
				SelectWholeText();
				TryRemoveSelection();
				break;
			default:
				Diagnostics.LogError(ActionOnFocus.ToString() + " is not correctly handled.");
				break;
			}
			caret.VisibleSelf = true;
		}

		public void OnEditionFinished()
		{
			UpdateInstructionVisibility();
			label.UITransform.X = 0f;
			caret.VisibleSelf = false;
			ExtendSelection = false;
			selectionStartIndex = -1;
			label.SelectText(0, 0);
			Input.imeCompositionMode = IMECompositionMode.Off;
		}

		public void SelectText(int beginIndex, int endIndex, bool force = false)
		{
			MoveCaretToCharIndex(beginIndex, force);
			ExtendSelection = true;
			MoveCaretToCharIndex(endIndex, force);
		}

		public void SelectWholeText(bool force = false)
		{
			MoveCaretToTextBeginning(force);
			ExtendSelection = true;
			MoveCaretToTextEnd(force);
		}

		public void MoveCaretAtPosition(Vector2 position)
		{
			Vector2 position2 = label.UITransform.LocalToGlobalMatrix.inverse * position.xy01();
			int charAtPosition = label.GetCharAtPosition(position2);
			MoveCaretToCharIndex(charAtPosition);
		}

		public void MoveCaretToCharIndex(int position, bool force = false)
		{
			if (force || caretIndex != position)
			{
				int length = text.Length;
				caretIndex = Mathf.Clamp(position, 0, length);
				UpdateCaretAndLabelPosition();
			}
			if (!ExtendSelection)
			{
				selectionStartIndex = caretIndex;
			}
			OnSelectionChanged();
		}

		public void MoveCaretToTextBeginning(bool force = false)
		{
			MoveCaretToCharIndex(0, force);
		}

		public void MoveCaretToTextEnd(bool force = false)
		{
			MoveCaretToCharIndex(text.Length, force);
		}

		public void MoveCaretToLineBeginning(bool force = false)
		{
			for (int num = caretIndex; num > 0; num--)
			{
				if (text[num - 1] == '\n')
				{
					MoveCaretToCharIndex(num, force);
					return;
				}
			}
			MoveCaretToCharIndex(0, force);
		}

		public void MoveCaretToLineEnd(bool force = false)
		{
			int length = text.Length;
			for (int i = caretIndex + 1; i < length; i++)
			{
				if (text[i] == '\n')
				{
					MoveCaretToCharIndex(i, force);
					return;
				}
			}
			MoveCaretToCharIndex(length, force);
		}

		public void AdvanceCaretCharwise(UITextFieldDirection direction)
		{
			if ((direction != UITextFieldDirection.Right || caretIndex < text.Length) && (direction != UITextFieldDirection.Left || caretIndex > 0))
			{
				MoveCaretToCharIndex((int)(caretIndex + direction));
			}
		}

		public void AdvanceCaretWordwise(UITextFieldDirection direction)
		{
			if ((direction == UITextFieldDirection.Right && caretIndex >= text.Length) || (direction == UITextFieldDirection.Left && caretIndex <= 0))
			{
				return;
			}
			int num = caretIndex;
			if (direction == UITextFieldDirection.Right)
			{
				TextUtils.CharacterType characterType = TextUtils.GetCharacterType(text[num]);
				num++;
				AdvanceIndexPassCharsOfType(ref num, characterType, direction);
				if (characterType != TextUtils.CharacterType.Space)
				{
					AdvanceIndexPassCharsOfType(ref num, TextUtils.CharacterType.Space, direction);
				}
			}
			else
			{
				num--;
				AdvanceIndexPassCharsOfType(ref num, TextUtils.CharacterType.Space, direction);
				if (num > -1)
				{
					TextUtils.CharacterType characterType2 = TextUtils.GetCharacterType(text[num]);
					AdvanceIndexPassCharsOfType(ref num, characterType2, direction);
				}
				num++;
			}
			MoveCaretToCharIndex(num);
		}

		public void MoveCaretOneLineUp(bool force = false)
		{
			if (caretIndex != 0)
			{
				Vector2 charPosition = label.GetCharPosition(caretIndex);
				int charAtPosition = label.GetCharAtPosition(charPosition, -1);
				MoveCaretToCharIndex(charAtPosition, force);
			}
		}

		public void MoveCaretOneLineDown(bool force = false)
		{
			if (caretIndex != text.Length)
			{
				Vector2 charPosition = label.GetCharPosition(caretIndex);
				int charAtPosition = label.GetCharAtPosition(charPosition, 1);
				MoveCaretToCharIndex(charAtPosition, force);
			}
		}

		public bool TryInsertChar(char newChar)
		{
			bool result = false;
			if (HasSelection)
			{
				result = TryRemoveSelection();
			}
			if (text.Length >= MaximumChars || TextUtils.IsSurrogate(newChar) || !IsValid(newChar))
			{
				return result;
			}
			int length = text.Length;
			if (newChar == '\0')
			{
				return false;
			}
			text = text.Insert(caretIndex, newChar.ToString());
			OnTextChanged();
			MoveCaretToCharIndex(caretIndex + text.Length - length);
			return true;
		}

		public bool TryInsertString(string newString)
		{
			bool result = false;
			if (HasSelection)
			{
				result = TryRemoveSelection();
			}
			int num = MaximumChars - text.Length;
			if (num <= 0)
			{
				return result;
			}
			tmpStringBuilder.Length = 0;
			tmpStringBuilder.Append(newString);
			for (int num2 = tmpStringBuilder.Length - 1; num2 >= 0; num2--)
			{
				char c = tmpStringBuilder[num2];
				if (TextUtils.IsSurrogate(c))
				{
					tmpStringBuilder.Remove(num2, 1);
					tmpStringBuilder[num2 - 1] = '?';
				}
				else if (c == '\r' && num2 < tmpStringBuilder.Length - 1 && tmpStringBuilder[num2 + 1] == '\n')
				{
					tmpStringBuilder.Remove(num2, 1);
				}
				else if (!IsValid(c))
				{
					tmpStringBuilder.Remove(num2, 1);
				}
			}
			if (num < tmpStringBuilder.Length)
			{
				tmpStringBuilder.Remove(num, tmpStringBuilder.Length - num);
			}
			int length = text.Length;
			if (string.IsNullOrEmpty(newString))
			{
				return false;
			}
			text = text.Insert(caretIndex, tmpStringBuilder.ToString());
			OnTextChanged();
			MoveCaretToCharIndex(caretIndex + text.Length - length);
			return true;
		}

		public bool TryRemoveChar(UITextFieldDirection direction)
		{
			ExtendSelection = true;
			AdvanceCaretCharwise(direction);
			ExtendSelection = true;
			return TryRemoveSelection();
		}

		public bool TryRemoveWord(UITextFieldDirection direction)
		{
			ExtendSelection = true;
			AdvanceCaretWordwise(direction);
			ExtendSelection = true;
			return TryRemoveSelection();
		}

		public bool TryRemoveSelection()
		{
			if (!HasSelection)
			{
				return false;
			}
			int num = Mathf.Min(selectionStartIndex, caretIndex);
			int num2 = Mathf.Abs(selectionStartIndex - caretIndex);
			if (num2 == 0)
			{
				return false;
			}
			if (num < 0 || num + num2 > text.Length)
			{
				Diagnostics.LogError($"Invalid bounds when trying to remove selection between {selectionStartIndex} and {caretIndex} : removalStart={num} removalLength={num2}");
				return false;
			}
			text = text.Remove(num, num2);
			OnTextChanged();
			if (selectionStartIndex < caretIndex)
			{
				caretIndex = selectionStartIndex;
			}
			else
			{
				selectionStartIndex = caretIndex;
			}
			label.SelectText(0, 0);
			UpdateCaretAndLabelPosition();
			return true;
		}

		public void ReplaceText(string newText)
		{
			if (label == null)
			{
				return;
			}
			selectionStartIndex = -1;
			ExtendSelection = false;
			int num = Mathf.Min(newText.Length, maximumChars);
			tmpStringBuilder.Length = 0;
			tmpStringBuilder.Append(newText, 0, num);
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				if (!IsValid(tmpStringBuilder[num2]))
				{
					tmpStringBuilder.Remove(num2, 1);
				}
			}
			string text = tmpStringBuilder.ToString();
			if (!(this.text == text))
			{
				this.text = text;
				OnTextChanged();
				MoveCaretToCharIndex(this.text.Length);
			}
		}

		public string GetSelectedText()
		{
			if (!HasSelection)
			{
				return string.Empty;
			}
			int startIndex = Mathf.Min(selectionStartIndex, caretIndex);
			int length = Mathf.Abs(selectionStartIndex - caretIndex);
			return text.Substring(startIndex, length);
		}

		protected override void Load()
		{
			label?.LoadIfNecessary();
			caret?.LoadIfNecessary();
			base.Load();
			if (label != null)
			{
				if (label.Text == instructionText)
				{
					text = string.Empty;
				}
				else
				{
					text = label.Text;
				}
				SetUpLabel();
				OnTextChanged();
			}
			if (caret != null)
			{
				caret.VisibleSelf = false;
			}
		}

		protected override IUIResponder InstantiateResponder()
		{
			return new UITextFieldResponder(this);
		}

		protected override void OnIsInteractiveChanged()
		{
			base.OnIsInteractiveChanged();
			if (TextFieldResponder.Focused)
			{
				if (base.IsInteractive)
				{
					OnEditionStarted();
				}
				else
				{
					OnEditionFinished();
				}
			}
		}

		private bool IsValid(char newChar)
		{
			if (!string.IsNullOrEmpty(whiteList))
			{
				for (int i = 0; i < whiteList.Length; i++)
				{
					if (newChar == whiteList[i])
					{
						return true;
					}
				}
				return false;
			}
			if (!string.IsNullOrEmpty(blackList))
			{
				for (int j = 0; j < blackList.Length; j++)
				{
					if (newChar == blackList[j])
					{
						return false;
					}
				}
				return true;
			}
			return true;
		}

		private void SetUpLabel()
		{
			if (multiline)
			{
				label.UITransform.LeftAnchor = label.UITransform.LeftAnchor.SetAttach(b: true);
				label.UITransform.RightAnchor = label.UITransform.RightAnchor.SetAttach(b: true);
				label.UITransform.TopAnchor = label.UITransform.TopAnchor.SetAttach(b: false);
				label.UITransform.BottomAnchor = label.UITransform.BottomAnchor.SetAttach(b: false);
				label.WordWrap = true;
				label.AutoAdjustHeight = true;
				label.AutoAdjustWidth = false;
			}
			else
			{
				label.UITransform.LeftAnchor = label.UITransform.LeftAnchor.SetAttach(b: false);
				label.UITransform.RightAnchor = label.UITransform.RightAnchor.SetAttach(b: false);
				label.UITransform.TopAnchor = label.UITransform.TopAnchor.SetAttach(b: true);
				label.UITransform.BottomAnchor = label.UITransform.BottomAnchor.SetAttach(b: true);
				label.UITransform.X = 0f;
				label.WordWrap = false;
				label.AutoAdjustHeight = false;
				label.AutoAdjustWidth = true;
			}
			label.UITransform.VisibleSelf = true;
		}

		private void OnTextChanged()
		{
			if (passwordChar != 0)
			{
				label.Text = new string(passwordChar, text.Length);
			}
			else
			{
				label.Text = text;
			}
			UpdateInstructionVisibility();
		}

		private void OnSelectionChanged()
		{
			if (HasSelection)
			{
				int num = Math.Min(selectionStartIndex, caretIndex);
				int num2 = Math.Max(selectionStartIndex, caretIndex);
				label.SelectText(num, num2 - num);
			}
			else
			{
				label.SelectText(0, 0);
			}
		}

		private void OnWhiteListChanged()
		{
			ReplaceText(Text);
		}

		private void OnBlackListChanged()
		{
			ReplaceText(Text);
		}

		private void OnInstructionTextChanged()
		{
			UpdateInstructionVisibility();
		}

		private void OnPasswordCharChanged()
		{
			OnTextChanged();
		}

		private void UpdateInstructionVisibility()
		{
			bool flag = UIInteractivityManager.Instance.FocusedResponder == TextFieldResponder;
			if (flag && string.IsNullOrEmpty(text))
			{
				label.InterpreteRichText = false;
				label.InterpreteSymbols = false;
				label.Text = string.Empty;
			}
			else if (!flag && string.IsNullOrEmpty(text))
			{
				label.InterpreteRichText = true;
				label.InterpreteSymbols = true;
				label.Text = instructionText;
			}
		}

		private void UpdateCaretAndLabelPosition()
		{
			Vector2 charPosition = label.GetCharPosition(caretIndex);
			Vector4 vector = label.UITransform.LocalToGlobalMatrix * charPosition.xy01();
			caret.SetGlobalPosition(vector);
			if (!(mask == null))
			{
				Rect globalRect = caret.GlobalRect;
				Rect globalRect2 = mask.GlobalRect;
				float num = 0f;
				if (globalRect.xMin < globalRect2.xMin)
				{
					num = globalRect2.xMin - globalRect.xMin;
				}
				else if (globalRect.xMax > globalRect2.xMax)
				{
					num = globalRect2.xMax - globalRect.xMax;
				}
				float num2 = 0f;
				if (globalRect.yMin < globalRect2.yMin)
				{
					num2 = globalRect2.yMin - globalRect.yMin;
				}
				else if (globalRect.yMax > globalRect2.yMax)
				{
					num2 = globalRect2.yMax - globalRect.yMax;
				}
				if (num != 0f)
				{
					label.UITransform.X += num;
				}
				if (num2 != 0f)
				{
					label.UITransform.Y += num2;
				}
				if (num != 0f || num2 != 0f)
				{
					charPosition = label.GetCharPosition(caretIndex);
					vector = label.UITransform.LocalToGlobalMatrix * charPosition.xy01();
					caret.SetGlobalPosition(vector);
				}
				if (caretAnimation != null)
				{
					caretAnimation.MultiController.StartAnimations();
				}
			}
		}

		private void AdvanceIndexPassCharsOfType(ref int index, TextUtils.CharacterType charType, UITextFieldDirection direction)
		{
			_ = text.Length;
			while (index >= 0 && index < text.Length && (direction != UITextFieldDirection.Left || index != -1) && TextUtils.GetCharacterType(text[index]) == charType)
			{
				index += (int)direction;
			}
		}

		private void OnMultilineChanged(bool previousMultiline, bool newMultiline)
		{
			SetUpLabel();
			if (newMultiline)
			{
				actionOnReturn = UITextFieldKeyAction.Default;
				actionOnTab = UITextFieldKeyAction.Default;
			}
			else
			{
				actionOnReturn = UITextFieldKeyAction.Validate;
				actionOnTab = UITextFieldKeyAction.Validate;
			}
			UpdateCaretAndLabelPosition();
		}
	}
}
