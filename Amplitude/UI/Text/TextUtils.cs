using System;

namespace Amplitude.UI.Text
{
	public class TextUtils
	{
		[Flags]
		public enum CharacterType
		{
			None = 0x0,
			Letter = 0x1,
			Space = 0x2,
			Custom = 0x4,
			Other = 0x8
		}

		public const char NullChar = '\0';

		public const char ZeroWidthSpaceChar = '\u200b';

		public const char FormatBeginChar = '<';

		public const char FormatEndChar = '>';

		public const char FormatRevertChar = '/';

		public const char FormatColorChar = 'c';

		public const char FormatColorCharCaps = 'C';

		public const char FormatItalicChar = 'i';

		public const char FormatItalicCharCaps = 'I';

		public const char FormatBoldChar = 'b';

		public const char FormatBoldCharCaps = 'B';

		public const char FormatUnderlineChar = 'u';

		public const char FormatUnderlineCharCaps = 'U';

		public const char FormatStrikeThroughChar = 's';

		public const char FormatStrikeThroughCharCaps = 'S';

		public const char FormatHighlightChar = 'm';

		public const char FormatHighlightCharCaps = 'M';

		public const int FormatColorFirstHalfLength = 3;

		public const int FormatItalicLength = 3;

		public const int FormatBoldLength = 3;

		public const int FormatUnderlineLength = 3;

		public const int FormatStrikeThroughLength = 3;

		public const int FormatHighlightLength = 3;

		public const int FormatRevertLength = 4;

		public const char TruncationChar = '.';

		public const char MaxLinesTruncationChar = '…';

		public const char SymbolBeginChar = '[';

		public const char SymbolEndChar = ']';

		public const char LocalizationChar = '%';

		public const char LineFeedChar = '\n';

		public const char CarriageReturnChar = '\r';

		public const char TabulationChar = '\t';

		public static CharacterType GetCharacterType(char character)
		{
			if ((character >= '0' && character <= '9') || (character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z') || (character >= 'À' && character <= 'ἀ' && character != '×' && character != '÷') || (character >= 'Ⰰ' && character <= '\u2dff') || (character >= '⸀' && character <= '꓿'))
			{
				return CharacterType.Letter;
			}
			switch (character)
			{
			case ' ':
			case '\u00a0':
			case '\u2000':
			case '\u2001':
			case '\u2002':
			case '\u2003':
			case '\u2004':
			case '\u2005':
			case '\u2006':
			case '\u2008':
			case '\u2009':
			case '\u200a':
			case '\u200b':
			case '\u3000':
				return CharacterType.Space;
			default:
				if (character >= '\ue000' && character <= '\uf8ff')
				{
					return CharacterType.Custom;
				}
				return CharacterType.Other;
			}
		}

		public static bool IsSurrogate(char character)
		{
			if (character >= '\ud800')
			{
				return character <= '\udfff';
			}
			return false;
		}

		public static bool IsBreakableWhitespace(char spaceChar)
		{
			switch (spaceChar)
			{
			case '\u2000':
			case '\u2001':
			case '\u2002':
			case '\u2003':
			case '\u2004':
			case '\u2005':
			case '\u2006':
			case '\u2007':
			case '\u2008':
			case '\u2009':
			case '\u200a':
			case '\u200b':
				return spaceChar != '\u2007';
			default:
				return false;
			case '\t':
			case ' ':
			case '\u3000':
				return true;
			}
		}

		public static bool IsAdjustableSpace(char spaceChar)
		{
			if (spaceChar != ' ' && spaceChar != '\u00a0')
			{
				return spaceChar == '\u3000';
			}
			return true;
		}

		public static byte ComputeHexa(char digit)
		{
			if (digit >= '0' && digit <= '9')
			{
				return (byte)(digit - 48);
			}
			if (digit >= 'a' && digit <= 'f')
			{
				return (byte)(10 + digit - 97);
			}
			if (digit >= 'A' && digit <= 'F')
			{
				return (byte)(10 + digit - 65);
			}
			return byte.MaxValue;
		}
	}
}
