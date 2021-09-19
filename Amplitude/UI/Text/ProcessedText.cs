using System;
using System.Collections.Generic;
using System.Text;
using Amplitude.Framework.Extensions;
using Amplitude.Graphics.Text;
using Amplitude.UI.Renderers;
using UnityEngine;

namespace Amplitude.UI.Text
{
	public class ProcessedText : IDisposable
	{
		private class ColorContext
		{
			public Color32 Current;

			private List<Color32> colorsList = new List<Color32>();

			public void Push(Color32 color)
			{
				colorsList.Add(color);
				Current = color;
			}

			public bool Pop()
			{
				if (colorsList.Count > 1)
				{
					colorsList.RemoveAt(colorsList.Count - 1);
					Current = colorsList[colorsList.Count - 1];
					return true;
				}
				return false;
			}

			public void Copy(ColorContext other)
			{
				colorsList.AddRange(other.colorsList);
				Current = other.Current;
			}

			public void Reset()
			{
				colorsList.Clear();
				Current = Color32Utils.White;
			}
		}

		internal struct Data
		{
			public bool IsImageSymbol;

			public Vector4 Position;

			public Color Color;

			public int SymbolMapperId;

			public void FillSymbol(Vector4 position, Color color, int symbolMapperId)
			{
				IsImageSymbol = true;
				Position = position;
				Color = color;
				SymbolMapperId = symbolMapperId;
			}

			public void FillGlyph()
			{
				IsImageSymbol = false;
				Position = Vector4.zero;
				Color = Color.magenta;
				SymbolMapperId = -1;
			}
		}

		private class FontFaceContext
		{
			public FontFace Current;

			private List<FontFace> fontFacesList = new List<FontFace>();

			public void Add(FontFace fontFace)
			{
				fontFacesList.Add(fontFace);
				Current ^= fontFace;
			}

			public bool Remove(FontFace fontFace)
			{
				for (int num = fontFacesList.Count - 1; num >= 0; num--)
				{
					if (fontFacesList[num] == fontFace)
					{
						fontFacesList.RemoveAt(num);
						RecomputeCurrentFontFace();
						return true;
					}
				}
				return false;
			}

			public void Copy(FontFaceContext other)
			{
				fontFacesList.AddRange(other.fontFacesList);
				Current = other.Current;
			}

			public void Reset()
			{
				fontFacesList.Clear();
				Current = FontFace.Normal;
			}

			private void RecomputeCurrentFontFace()
			{
				Current = FontFace.Normal;
				int count = fontFacesList.Count;
				for (int i = 0; i < count; i++)
				{
					Current ^= fontFacesList[i];
				}
			}
		}

		private struct FormattedChar
		{
			public char Char;

			public FontFace Face;

			public TextStyle Style;

			public Color32 Color;

			public FontAsset Font;

			public int UserDefined;

			public float Width;

			public Vector2 Position;

			public bool IsFormatChar
			{
				get
				{
					if (Char >= '\uf000')
					{
						return Char < '\uf0f0';
					}
					return false;
				}
			}

			public bool IsNewLineChar
			{
				get
				{
					if (Char != '\n')
					{
						return Char == '\uf001';
					}
					return true;
				}
			}

			public FormattedChar(char @char)
			{
				Char = @char;
				Face = FontFace.Normal;
				Style = TextStyle.None;
				Color = Color32Utils.White;
				Font = null;
				UserDefined = 0;
				Width = 0f;
				Position = Vector2.zero;
			}

			public FormattedChar(char @char, int userDefined)
			{
				Char = @char;
				Face = FontFace.Normal;
				Style = TextStyle.None;
				Color = Color32Utils.White;
				Font = null;
				UserDefined = userDefined;
				Width = 0f;
				Position = Vector2.zero;
			}

			public void CopyFormattingFrom(FormattedChar other)
			{
				Face = other.Face;
				Style = other.Style;
				Color = other.Color;
			}

			public void ResolveFont(FontFamily fontFamily)
			{
				FontAsset fontAsset = fontFamily.GetFontAsset(Face);
				if (Char == '\n' || Char == '\t' || IsFormatChar || fontAsset.IsGlyphAvailable(Char))
				{
					Font = fontAsset;
					return;
				}
				FontAsset fontAsset2 = UIRenderingManager.Instance.FallbackFontFamily.GetFontAsset(Face);
				if (fontAsset2.IsGlyphAvailable(Char))
				{
					Font = fontAsset2;
					return;
				}
				Char = '?';
				Font = fontAsset;
			}

			public override string ToString()
			{
				return $"{Char} ({Face}, {Style}, {Color}, {Width})";
			}
		}

		private class TextStyleContext
		{
			public TextStyle Current;

			private List<TextStyle> textStylesList = new List<TextStyle>();

			public void Add(TextStyle textStyle)
			{
				textStylesList.Add(textStyle);
				Current ^= textStyle;
			}

			public bool Remove(TextStyle textStyle)
			{
				for (int num = textStylesList.Count - 1; num >= 0; num--)
				{
					if (textStylesList[num] == textStyle)
					{
						textStylesList.RemoveAt(num);
						RecomputeCurrentTextStyle();
						return true;
					}
				}
				return false;
			}

			public void Copy(TextStyleContext other)
			{
				textStylesList.AddRange(other.textStylesList);
				Current = other.Current;
			}

			public void Reset()
			{
				textStylesList.Clear();
				Current = TextStyle.None;
			}

			private void RecomputeCurrentTextStyle()
			{
				Current = TextStyle.None;
				int count = textStylesList.Count;
				for (int i = 0; i < count; i++)
				{
					Current ^= textStylesList[i];
				}
			}
		}

		public bool IsTruncated;

		public bool IsMultilineTruncated;

		private const char FormatCharEndOfText = '\0';

		private const char FormatCharInvalid = '\uf000';

		private const char FormatCharWordWrap = '\uf001';

		private const char FormatCharImageSymbol = '\uf002';

		private const char FormatCharMaxValue = '\uf0f0';

		private const char CharIfGlyphNotFound = '?';

		private const int TabulationLength = 4;

		private const float Epsilon = 0.01f;

		private static FontFaceContext fontFaceContext = new FontFaceContext();

		private static TextStyleContext textStyleContext = new TextStyleContext();

		private static ColorContext colorContext = new ColorContext();

		private static StringBuilder stringBuilder = new StringBuilder();

		private static PerformanceList<int> adjustableSpacesIndexes = default(PerformanceList<int>);

		private static PerformanceList<FormattedChar> editableText = default(PerformanceList<FormattedChar>);

		private static PerformanceList<int> editableLinesBeginnings = default(PerformanceList<int>);

		private PerformanceList<FormattedChar> formattedText;

		private Vector2 endOfTextPosition = Vector2.zero;

		private Rect unscaledLocalRect = new Rect(0f, 0f, float.MaxValue, float.MaxValue);

		private string rawText = string.Empty;

		private FontFamily fontFamily;

		private FontFace defaultFontFace;

		private uint unscaledFontSize;

		private Color defaultColor;

		private FontRenderingMode wantedRenderingMode;

		private Alignment alignment = Alignment.CenterCenter;

		private bool useAscent;

		private bool justify;

		private bool forceCaps;

		private uint autoAdjustUnscaledFontSizeMin;

		private bool autoTruncate;

		private bool wordWrap;

		private float interLetterAdditionalSpacing;

		private float interLineAdditionalSpacing;

		private float interParagraphAdditionalSpacing;

		private int maximumLines;

		private TextStyle defaultTextStyle;

		private bool interpreteRichText = true;

		private bool interpreteSymbols = true;

		private UIView view;

		private Matrix4x4 localToGlobalMatrix;

		private float widthScalingRatio = 1f;

		private float heightScalingRatio = 1f;

		private FontRenderingMode usedRenderingMode;

		private Rect localRect = new Rect(0f, 0f, float.MaxValue, float.MaxValue);

		private uint usedUnscaledFontSize;

		private uint fontSize;

		private float ascender;

		private float descender;

		private float capHeight;

		private float accentedCapHeight;

		private float corpusSize;

		private float lineHeight;

		private float underlinePosition;

		private float underlineThickness;

		private bool formattingDirty = true;

		private bool glyphsCacheDirty = true;

		private bool styleRectsCacheDirty = true;

		private int glyphCount;

		private int symbolCount;

		private Data[] glyphsCache;

		private PerformanceList<RectColored> styleRectsCache;

		private int fontAtlasRendererRevisionIndex;

		private UIAtomId glyphsAtomId = UIAtomId.Invalid;

		private UIAtomId transformAtomId = UIAtomId.Invalid;

		private UIAtomId materialPropertyOverridesAtomId = UIAtomId.Invalid;

		private float computedWidthCache = -1f;

		private float computedHeightCache = -1f;

		public Matrix4x4 LocalToGlobalMatrix => localToGlobalMatrix;

		public UIView View => view;

		public Rect UnscaledLocalRect
		{
			get
			{
				return unscaledLocalRect;
			}
			set
			{
				if (unscaledLocalRect != value)
				{
					unscaledLocalRect = value;
					OnPositioningContextChanged();
				}
			}
		}

		public string RawText
		{
			get
			{
				return rawText;
			}
			set
			{
				if (rawText != value)
				{
					rawText = value;
					FormattingDirty = true;
				}
			}
		}

		public FontFamily FontFamily
		{
			get
			{
				return fontFamily;
			}
			set
			{
				if (fontFamily != value)
				{
					if (value != null)
					{
						fontFamily = value;
					}
					else
					{
						fontFamily = UIRenderingManager.Instance.DefaultFontFamily;
					}
					FormattingDirty = true;
				}
			}
		}

		public FontFace DefaultFontFace
		{
			get
			{
				return defaultFontFace;
			}
			set
			{
				if (defaultFontFace != value)
				{
					defaultFontFace = value;
					FormattingDirty = true;
				}
			}
		}

		public uint UnscaledFontSize
		{
			get
			{
				return unscaledFontSize;
			}
			set
			{
				if (unscaledFontSize != value)
				{
					unscaledFontSize = value;
					OnPositioningContextChanged();
				}
			}
		}

		public Color DefaultColor
		{
			get
			{
				return defaultColor;
			}
			set
			{
				if (defaultColor != value)
				{
					defaultColor = value;
					FormattingDirty = true;
					styleRectsCacheDirty = true;
				}
			}
		}

		public FontRenderingMode WantedRenderingMode
		{
			get
			{
				return wantedRenderingMode;
			}
			set
			{
				if (wantedRenderingMode != value)
				{
					wantedRenderingMode = value;
					FormattingDirty = true;
					OnPositioningContextChanged();
				}
			}
		}

		public Alignment Alignment
		{
			get
			{
				return alignment;
			}
			set
			{
				if (alignment != value)
				{
					alignment = value;
					FormattingDirty = true;
				}
			}
		}

		public bool UseAscent
		{
			get
			{
				return useAscent;
			}
			set
			{
				if (useAscent != value)
				{
					useAscent = value;
					FormattingDirty = true;
				}
			}
		}

		public bool Justify
		{
			get
			{
				return justify;
			}
			set
			{
				if (justify != value)
				{
					justify = value;
					FormattingDirty = true;
				}
			}
		}

		public bool ForceCaps
		{
			get
			{
				return forceCaps;
			}
			set
			{
				if (forceCaps != value)
				{
					forceCaps = value;
					FormattingDirty = true;
				}
			}
		}

		public uint AutoAdjustUnscaledFontSizeMin
		{
			get
			{
				return autoAdjustUnscaledFontSizeMin;
			}
			set
			{
				if (autoAdjustUnscaledFontSizeMin != value)
				{
					autoAdjustUnscaledFontSizeMin = value;
					if (autoAdjustUnscaledFontSizeMin == 0)
					{
						fontSize = ((usedRenderingMode == FontRenderingMode.DistanceField) ? unscaledFontSize : ComputeScaledFontSize(unscaledFontSize));
					}
					FormattingDirty = true;
				}
			}
		}

		public bool AutoTruncate
		{
			get
			{
				return autoTruncate;
			}
			set
			{
				if (autoTruncate != value)
				{
					autoTruncate = value;
					FormattingDirty = true;
				}
			}
		}

		public bool WordWrap
		{
			get
			{
				return wordWrap;
			}
			set
			{
				if (wordWrap != value)
				{
					wordWrap = value;
					FormattingDirty = true;
				}
			}
		}

		public float InterLetterAdditionalSpacing
		{
			get
			{
				return interLetterAdditionalSpacing;
			}
			set
			{
				if (interLetterAdditionalSpacing != value)
				{
					interLetterAdditionalSpacing = value;
					FormattingDirty = true;
				}
			}
		}

		public float InterLineAdditionalSpacing
		{
			get
			{
				return interLineAdditionalSpacing;
			}
			set
			{
				if (interLineAdditionalSpacing != value)
				{
					interLineAdditionalSpacing = value;
					FormattingDirty = true;
				}
			}
		}

		public float InterParagraphAdditionalSpacing
		{
			get
			{
				return interParagraphAdditionalSpacing;
			}
			set
			{
				if (interParagraphAdditionalSpacing != value)
				{
					interParagraphAdditionalSpacing = value;
					FormattingDirty = true;
				}
			}
		}

		public int MaximumLines
		{
			get
			{
				return maximumLines;
			}
			set
			{
				if (maximumLines != value)
				{
					maximumLines = value;
					FormattingDirty = true;
				}
			}
		}

		public TextStyle DefaultTextStyle
		{
			get
			{
				return defaultTextStyle;
			}
			set
			{
				if (defaultTextStyle != value)
				{
					defaultTextStyle = value;
					FormattingDirty = true;
				}
			}
		}

		public bool InterpreteRichText
		{
			get
			{
				return interpreteRichText;
			}
			set
			{
				if (interpreteRichText != value)
				{
					interpreteRichText = value;
					FormattingDirty = true;
				}
			}
		}

		public bool InterpreteSymbols
		{
			get
			{
				return interpreteSymbols;
			}
			set
			{
				if (interpreteSymbols != value)
				{
					interpreteSymbols = value;
					FormattingDirty = true;
				}
			}
		}

		public FontRenderingMode UsedRenderingMode => usedRenderingMode;

		public UIAtomId MaterialPropertyOverridesAtomId
		{
			get
			{
				return materialPropertyOverridesAtomId;
			}
			set
			{
				if (!materialPropertyOverridesAtomId.Equals(value))
				{
					materialPropertyOverridesAtomId = value;
					glyphsCacheDirty = true;
					styleRectsCacheDirty = true;
				}
			}
		}

		private bool FormattingDirty
		{
			get
			{
				return formattingDirty;
			}
			set
			{
				if (formattingDirty != value)
				{
					formattingDirty = value;
					if (formattingDirty)
					{
						glyphsCacheDirty = true;
						styleRectsCacheDirty = true;
						usedUnscaledFontSize = 0u;
						computedWidthCache = -1f;
						computedHeightCache = -1f;
						IsTruncated = false;
						IsMultilineTruncated = false;
					}
				}
			}
		}

		private float ImageSymbolSquareSize => capHeight;

		private float DistanceBetweenBaselines => capHeight - descender;

		private float ScaledInterLetterAdditionalSpacing => interLetterAdditionalSpacing * heightScalingRatio;

		private float ScaledInterLineAdditionalSpacing => interLineAdditionalSpacing * heightScalingRatio;

		private float ScaledInterParagraphAdditionalSpacing => interParagraphAdditionalSpacing * heightScalingRatio;

		private float LineFeedVerticalAdvance
		{
			get
			{
				float num = DistanceBetweenBaselines + ScaledInterLineAdditionalSpacing + ScaledInterParagraphAdditionalSpacing;
				if (usedRenderingMode == FontRenderingMode.Raster)
				{
					num = Mathf.Round(num);
				}
				return num;
			}
		}

		private float WordWrapVerticalAdvance
		{
			get
			{
				float num = DistanceBetweenBaselines + ScaledInterLineAdditionalSpacing;
				if (usedRenderingMode == FontRenderingMode.Raster)
				{
					num = Mathf.Round(num);
				}
				return num;
			}
		}

		public ProcessedText()
		{
		}

		public ProcessedText(UIView view, Matrix4x4 localToGlobalMatrix, Rect rect, string text, FontFamily fontFamily, FontFace defaultFontFace, uint fontSize, Color defaultColor = default(Color), FontRenderingMode wantedRenderingMode = FontRenderingMode.Default, Alignment alignment = default(Alignment), bool useAscent = false, bool justify = false, bool forceCaps = false, uint autoAdjustFontSizeMin = 0u, bool autoTruncate = false, bool wordWrap = false, float interLetterAdditionalSpacing = 0f, float interLineAdditionalSpacing = 0f, float interParagraphAdditionalSpacing = 0f, int maximumLines = -1, TextStyle defaultTextStyle = TextStyle.None, bool interpreteRichText = true, bool interpreteSymbols = true, UIComponent contextComponent = null)
		{
			this.view = view;
			this.localToGlobalMatrix = localToGlobalMatrix;
			unscaledLocalRect = rect;
			rawText = text;
			this.fontFamily = fontFamily;
			this.defaultFontFace = defaultFontFace;
			unscaledFontSize = fontSize;
			this.fontSize = fontSize;
			this.defaultColor = defaultColor;
			this.wantedRenderingMode = wantedRenderingMode;
			this.alignment = alignment;
			this.useAscent = useAscent;
			this.justify = justify;
			this.forceCaps = forceCaps;
			autoAdjustUnscaledFontSizeMin = autoAdjustFontSizeMin;
			this.autoTruncate = autoTruncate;
			this.wordWrap = wordWrap;
			this.interLetterAdditionalSpacing = interLetterAdditionalSpacing;
			this.interLineAdditionalSpacing = interLineAdditionalSpacing;
			this.interParagraphAdditionalSpacing = interParagraphAdditionalSpacing;
			this.maximumLines = maximumLines;
			this.defaultTextStyle = defaultTextStyle;
			this.interpreteRichText = interpreteRichText;
			this.interpreteSymbols = interpreteSymbols;
			if (this.fontFamily == null)
			{
				this.fontFamily = UIRenderingManager.Instance.DefaultFontFamily;
			}
			Matrix4x4 value = Matrix4x4.identity;
			transformAtomId = UIAtomContainer<Matrix4x4>.Allocate(ref value);
			materialPropertyOverridesAtomId = UIAtomId.Invalid;
			OnPositioningContextChanged();
			FormattingDirty = true;
		}

		public void Dispose()
		{
			if (glyphsAtomId.IsValid)
			{
				UIAtomContainer<UIPrimitiveGlyphData>.Deallocate(ref glyphsAtomId);
			}
			if (transformAtomId.IsValid)
			{
				UIAtomContainer<Matrix4x4>.Deallocate(ref transformAtomId);
			}
		}

		public void Clear()
		{
			view = null;
			localRect = new Rect(0f, 0f, float.MaxValue, float.MaxValue);
			rawText = string.Empty;
			fontFamily = UIRenderingManager.Instance.DefaultFontFamily;
			unscaledFontSize = 0u;
			fontSize = 0u;
			defaultColor = default(Color);
			alignment = Alignment.CenterCenter;
			wantedRenderingMode = FontRenderingMode.Default;
			forceCaps = false;
			interLetterAdditionalSpacing = 0f;
			interLineAdditionalSpacing = 0f;
			interParagraphAdditionalSpacing = 0f;
			autoTruncate = false;
			wordWrap = false;
			maximumLines = 0;
			defaultTextStyle = TextStyle.None;
			interpreteRichText = true;
			interpreteSymbols = true;
			formattedText.Clear();
			ascender = 0f;
			descender = 0f;
			capHeight = 0f;
			accentedCapHeight = 0f;
			corpusSize = 0f;
			lineHeight = 0f;
			underlinePosition = 0f;
			underlineThickness = 0f;
			formattingDirty = true;
			glyphsCacheDirty = true;
			computedWidthCache = -1f;
			computedHeightCache = -1f;
			Array.Clear(glyphsCache, 0, glyphsCache.Length);
			if (glyphsAtomId.IsValid)
			{
				UIAtomContainer<UIPrimitiveGlyphData>.Deallocate(ref glyphsAtomId);
			}
			if (transformAtomId.IsValid)
			{
				UIAtomContainer<Matrix4x4>.Deallocate(ref transformAtomId);
			}
		}

		public override string ToString()
		{
			return rawText;
		}

		public float ComputeWidth()
		{
			if (computedWidthCache >= 0f)
			{
				return computedWidthCache;
			}
			if (formattingDirty)
			{
				ReprocessFormatting();
			}
			if (formattedText.Count == 0)
			{
				computedWidthCache = 0f;
				return computedWidthCache;
			}
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < formattedText.Count; i++)
			{
				FormattedChar formattedChar = formattedText.Data[i];
				if (formattedChar.IsNewLineChar)
				{
					if (num2 > num)
					{
						num = num2;
					}
					num2 = 0f;
				}
				else
				{
					num2 += formattedChar.Width;
				}
			}
			if (num2 > num)
			{
				num = num2;
			}
			computedWidthCache = Mathf.Ceil(num / widthScalingRatio);
			return computedWidthCache;
		}

		public float ComputeHeight()
		{
			if (computedHeightCache >= 0f)
			{
				return computedHeightCache;
			}
			if (formattingDirty)
			{
				ReprocessFormatting();
			}
			if (formattedText.Count == 0)
			{
				computedHeightCache = 0f;
			}
			else
			{
				float num = accentedCapHeight - descender;
				for (int i = 0; i < formattedText.Count; i++)
				{
					FormattedChar formattedChar = formattedText.Data[i];
					if (formattedChar.Char == '\uf001')
					{
						num += WordWrapVerticalAdvance;
					}
					else if (formattedChar.Char == '\n')
					{
						num += LineFeedVerticalAdvance;
					}
				}
				computedHeightCache = Mathf.Ceil(num / heightScalingRatio);
			}
			return computedHeightCache;
		}

		public int GetLinesCount()
		{
			if (formattingDirty)
			{
				ReprocessFormatting();
			}
			int num = 1;
			for (int i = 0; i < formattedText.Count; i++)
			{
				if (formattedText.Data[i].IsNewLineChar)
				{
					num++;
				}
			}
			return num;
		}

		public int GetInterParagraphsCount()
		{
			if (formattingDirty)
			{
				ReprocessFormatting();
			}
			int num = 0;
			for (int i = 0; i < formattedText.Count; i++)
			{
				if (formattedText.Data[i].Char == '\n')
				{
					num++;
				}
			}
			return num;
		}

		public uint GetUsedUnscaledFontSize()
		{
			if (formattingDirty)
			{
				ReprocessFormatting();
			}
			if (usedUnscaledFontSize == 0)
			{
				return unscaledFontSize;
			}
			return usedUnscaledFontSize;
		}

		public Vector2 GetCharPosition(int charIndex)
		{
			if (formattingDirty)
			{
				ReprocessFormatting();
			}
			RebuildEditableText();
			Vector2 position = editableText.Data[charIndex].Position;
			return MakeInternalPositionLocal(position);
		}

		public int GetCharAtPosition(Vector2 localPosition, int lineOffset = 0)
		{
			if (formattingDirty)
			{
				ReprocessFormatting();
			}
			if (formattedText.Count == 0)
			{
				return 0;
			}
			RebuildEditableText();
			RebuildLinesStartIndexes();
			Vector2 vector = MakeLocalPositionInternal(localPosition);
			int i;
			for (i = 0; i < editableLinesBeginnings.Count && !(vector.y < editableText.Data[editableLinesBeginnings.Data[i]].Position.y - descender); i++)
			{
			}
			i = Mathf.Clamp(i + lineOffset, 0, editableLinesBeginnings.Count - 1);
			int num = editableLinesBeginnings.Data[i];
			int num2 = ((i < editableLinesBeginnings.Count - 1) ? (editableLinesBeginnings.Data[i + 1] - 1) : (editableText.Count - 1));
			for (int j = num; j <= num2; j++)
			{
				float x = editableText.Data[j].Position.x;
				float width = editableText.Data[j].Width;
				if (vector.x < x + width / 2f)
				{
					return j;
				}
			}
			return num2;
		}

		public void SelectText(int selectionStartIndex, int selectionLength, ref PerformanceList<RectColored> selectionRectsColored)
		{
			if (selectionLength <= 0)
			{
				return;
			}
			float height = capHeight - descender;
			RebuildEditableText();
			int num = selectionStartIndex;
			int num2 = num + selectionLength;
			for (int i = selectionStartIndex; i < num2; i++)
			{
				if (i > selectionStartIndex && editableText.Data[i].Position.y > editableText.Data[i - 1].Position.y)
				{
					AddRectColored(editableText, ref selectionRectsColored, num, i - 1, 0f - capHeight - 1f, height, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 64));
					num = i;
				}
				else if (i == num2 - 1)
				{
					AddRectColored(editableText, ref selectionRectsColored, num, i, 0f - capHeight - 1f, height, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 64));
				}
			}
		}

		public void UpdateView(UIView view)
		{
			if (this.view != view)
			{
				this.view = view;
				OnPositioningContextChanged();
			}
		}

		public void UpdateLocalToGlobalMatrix(Matrix4x4 transformMatrix)
		{
			if (localToGlobalMatrix != transformMatrix)
			{
				localToGlobalMatrix = transformMatrix;
				OnPositioningContextChanged();
			}
		}

		public void OnPositioningContextChanged()
		{
			float num = widthScalingRatio;
			float num2 = heightScalingRatio;
			uint num3 = fontSize;
			Rect rect = localRect;
			FontRenderingMode fontRenderingMode = usedRenderingMode;
			bool flag = localToGlobalMatrix.GetColumn(0) == VectorExtension.X && localToGlobalMatrix.GetColumn(1) == VectorExtension.Y && localToGlobalMatrix.GetColumn(2) == VectorExtension.Z;
			bool flag2 = view.IsDistortedProjection || !flag;
			usedRenderingMode = (flag2 ? FontRenderingMode.DistanceField : UIRenderingManager.Instance.ResolveFontRenderingMode(wantedRenderingMode, fontFamily));
			if (usedRenderingMode == FontRenderingMode.DistanceField)
			{
				widthScalingRatio = 1f;
				heightScalingRatio = 1f;
				fontSize = unscaledFontSize;
				localRect = unscaledLocalRect;
			}
			else
			{
				if (!view.Loaded)
				{
					return;
				}
				widthScalingRatio = (float)view.OutputWidth / view.Viewport.width;
				heightScalingRatio = (float)view.OutputHeight / view.Viewport.height;
				fontSize = ComputeScaledFontSize(unscaledFontSize);
				localRect = ComputeScaledLocalRect();
			}
			if (usedRenderingMode == FontRenderingMode.DistanceField)
			{
				UIAtomContainer<Matrix4x4>.SetData(ref transformAtomId, ref localToGlobalMatrix);
			}
			else
			{
				UIView uIView = view;
				Matrix4x4 data = GL.GetGPUProjectionMatrix(uIView.ProjectionMatrix, renderIntoTexture: false).inverse * uIView.ScreenToClipMatrix;
				UIAtomContainer<Matrix4x4>.SetData(ref transformAtomId, ref data);
			}
			if (widthScalingRatio != num || heightScalingRatio != num2 || fontSize != num3 || localRect != rect || usedRenderingMode != fontRenderingMode)
			{
				FormattingDirty = true;
			}
		}

		internal Data[] GetGlyphDataCache(FontAtlasRenderer fontAtlasRenderer, out UIAtomId glyphsAtomId, out int glyphCount, out int symbolCount)
		{
			int revisionIndex = fontAtlasRenderer.RevisionIndex;
			if (fontAtlasRendererRevisionIndex != revisionIndex)
			{
				fontAtlasRendererRevisionIndex = revisionIndex;
				OnPositioningContextChanged();
				glyphsCacheDirty = true;
			}
			if (formattingDirty)
			{
				ReprocessFormatting();
			}
			if (glyphsCacheDirty)
			{
				RebuildGlyphsCache(fontAtlasRenderer);
				RebuildStyleRectsCache();
			}
			glyphCount = this.glyphCount;
			symbolCount = this.symbolCount;
			glyphsAtomId = this.glyphsAtomId;
			return glyphsCache;
		}

		internal PerformanceList<RectColored> GetStyleRects()
		{
			if (styleRectsCacheDirty)
			{
				RebuildStyleRectsCache();
			}
			return styleRectsCache;
		}

		private uint ComputeScaledFontSize(uint unscaledFontSize)
		{
			uint num = (uint)Mathf.RoundToInt((float)unscaledFontSize * heightScalingRatio);
			if (num > 255)
			{
				Diagnostics.LogError("Trying to set a font size of {0}, this is probably wrong.", num);
				num = 255u;
			}
			else if (num == 0)
			{
				num = 20u;
			}
			return num;
		}

		private Rect ComputeScaledLocalRect()
		{
			Vector4 vector = localToGlobalMatrix * new Vector4(unscaledLocalRect.position.x, unscaledLocalRect.position.y, 0f, 1f);
			Vector2 vector2 = view.StandardizedPositionToScreenPosition(vector.x, vector.y);
			Vector2 size = view.StandardizedPositionToScreenPosition(vector.x + unscaledLocalRect.width, vector.y + unscaledLocalRect.height) - vector2;
			return new Rect(vector2, size);
		}

		private void ReprocessFormatting(bool handleLongText = true)
		{
			FormattingDirty = false;
			fontFamily.NormalFont.GlobalMetrics(fontSize, out ascender, out descender, out corpusSize, out capHeight, out accentedCapHeight, out lineHeight, out underlinePosition, out underlineThickness);
			int length = rawText.Length;
			formattedText.Clear();
			formattedText.Reserve(length);
			for (int i = 0; i < length; i++)
			{
				char c = rawText[i];
				if (TextUtils.IsSurrogate(c))
				{
					c = '?';
					i++;
				}
				formattedText.Add(new FormattedChar(c));
			}
			if (interpreteSymbols)
			{
				ParseSymbols();
			}
			if (forceCaps)
			{
				RaiseCaps();
			}
			if (interpreteRichText)
			{
				ApplyRichText();
			}
			ParseNewLines();
			ResolveCharsFonts();
			ComputeCharsWidths();
			if (handleLongText)
			{
				HandleLongText();
			}
			if (wordWrap && justify)
			{
				AdjustSpaces();
			}
			ComputeCharsPositions();
		}

		private void ParseSymbols()
		{
			if (UIRenderingManager.Instance.SymbolMapperController == null)
			{
				return;
			}
			for (int i = 0; i < formattedText.Count; i++)
			{
				if (formattedText.Data[i].Char != '[')
				{
					continue;
				}
				int j = i + 1;
				stringBuilder.Length = 0;
				stringBuilder.Append('[');
				for (; j < formattedText.Count && formattedText.Data[j].Char != ']'; j++)
				{
					stringBuilder.Append(char.ToUpperInvariant(formattedText.Data[j].Char));
				}
				stringBuilder.Append(']');
				if (j >= formattedText.Count || formattedText.Data[j].Char != ']')
				{
					continue;
				}
				int length = j - i + 1;
				StaticString tag = new StaticString(stringBuilder.ToString());
				if (!UIRenderingManager.Instance.SymbolMapperController.TryFindSymbolMapper(tag, out var mapper, out var identifier))
				{
					continue;
				}
				SymbolToTextMapper symbolToTextMapper = mapper as SymbolToTextMapper;
				if (symbolToTextMapper != null)
				{
					formattedText.RemoveRange(i, length);
					string text = symbolToTextMapper.Text;
					for (int k = 0; k < text.Length; k++)
					{
						formattedText.Insert(i + k, new FormattedChar(text[k]));
					}
					i--;
				}
				if (mapper as SymbolToImageMapper != null)
				{
					formattedText.RemoveRange(i, length);
					formattedText.Insert(i, new FormattedChar('\uf002', identifier));
				}
			}
		}

		private void RaiseCaps()
		{
			for (int i = 0; i < formattedText.Count; i++)
			{
				formattedText.Data[i].Char = char.ToUpperInvariant(formattedText.Data[i].Char);
			}
		}

		private void ApplyRichText()
		{
			fontFaceContext.Reset();
			fontFaceContext.Add(defaultFontFace);
			textStyleContext.Reset();
			textStyleContext.Add(defaultTextStyle);
			colorContext.Reset();
			colorContext.Push(defaultColor);
			for (int i = 0; i < formattedText.Count; i++)
			{
				if (formattedText.Data[i].Char == '<')
				{
					int num = ApplyRichText_FindNextFormatEndChar(i);
					if (num != -1)
					{
						int formatLength = num - i + 1;
						if (formattedText.Data[i + 1].Char == '/')
						{
							if (ApplyRichText_TryProcessFormatRevert(i, formatLength))
							{
								i--;
								continue;
							}
						}
						else if (ApplyRichText_TryProcessFormatAddition(i, formatLength))
						{
							i--;
							continue;
						}
					}
				}
				formattedText.Data[i].Face = fontFaceContext.Current;
				formattedText.Data[i].Style = textStyleContext.Current;
				formattedText.Data[i].Color = colorContext.Current;
			}
			fontFaceContext.Remove(defaultFontFace);
			textStyleContext.Remove(defaultTextStyle);
			colorContext.Pop();
		}

		private int ApplyRichText_FindNextFormatEndChar(int formatBeginCharIndex)
		{
			for (int i = formatBeginCharIndex + 2; i < formattedText.Count; i++)
			{
				if (formattedText.Data[i].Char == '>')
				{
					return i;
				}
			}
			return -1;
		}

		private bool ApplyRichText_TryProcessFormatAddition(int beginIndex, int formatLength)
		{
			char @char = formattedText.Data[beginIndex + 1].Char;
			bool flag = false;
			if (formatLength == 3 && (@char == 'i' || @char == 'I'))
			{
				fontFaceContext.Add(FontFace.Italic);
				flag = true;
			}
			if (formatLength == 3 && (@char == 'b' || @char == 'B'))
			{
				fontFaceContext.Add(FontFace.Bold);
				flag = true;
			}
			if (formatLength == 3 && (@char == 'u' || @char == 'U'))
			{
				textStyleContext.Add(TextStyle.Underline);
				flag = true;
			}
			if (formatLength == 3 && (@char == 's' || @char == 'S'))
			{
				textStyleContext.Add(TextStyle.StrikeThrough);
				flag = true;
			}
			if (formatLength == 3 && (@char == 'm' || @char == 'M'))
			{
				textStyleContext.Add(TextStyle.Highlight);
				flag = true;
			}
			if ((formatLength == 10 || formatLength == 12) && (@char == 'c' || @char == 'C') && formattedText.Data[beginIndex + 2].Char == '=')
			{
				int num = beginIndex + 3;
				int num2 = formatLength - 3 - 1;
				colorContext.Push(new Color32((byte)(TextUtils.ComputeHexa(formattedText.Data[num].Char) * 16 + TextUtils.ComputeHexa(formattedText.Data[num + 1].Char)), (byte)(TextUtils.ComputeHexa(formattedText.Data[num + 2].Char) * 16 + TextUtils.ComputeHexa(formattedText.Data[num + 3].Char)), (byte)(TextUtils.ComputeHexa(formattedText.Data[num + 4].Char) * 16 + TextUtils.ComputeHexa(formattedText.Data[num + 5].Char)), (num2 == 8) ? ((byte)(TextUtils.ComputeHexa(formattedText.Data[num + 6].Char) * 16 + TextUtils.ComputeHexa(formattedText.Data[num + 7].Char))) : byte.MaxValue));
				flag = true;
			}
			if (flag)
			{
				formattedText.RemoveRange(beginIndex, formatLength);
				return true;
			}
			return false;
		}

		private bool ApplyRichText_TryProcessFormatRevert(int beginIndex, int formatLength)
		{
			if (formatLength != 4)
			{
				return false;
			}
			char @char = formattedText.Data[beginIndex + 2].Char;
			bool flag = false;
			switch (@char)
			{
			case 'I':
			case 'i':
				fontFaceContext.Remove(FontFace.Italic);
				flag = true;
				break;
			case 'B':
			case 'b':
				fontFaceContext.Remove(FontFace.Bold);
				flag = true;
				break;
			case 'U':
			case 'u':
				textStyleContext.Remove(TextStyle.Underline);
				flag = true;
				break;
			case 'S':
			case 's':
				textStyleContext.Remove(TextStyle.StrikeThrough);
				flag = true;
				break;
			case 'M':
			case 'm':
				textStyleContext.Remove(TextStyle.Highlight);
				flag = true;
				break;
			case 'C':
			case 'c':
				colorContext.Pop();
				flag = true;
				break;
			}
			if (flag)
			{
				formattedText.RemoveRange(beginIndex, formatLength);
				return true;
			}
			return false;
		}

		private void ParseNewLines()
		{
			for (int i = 0; i < formattedText.Count; i++)
			{
				if (formattedText.Data[i].Char == '\r')
				{
					formattedText.Data[i].Char = '\n';
					if (i + 1 < formattedText.Count && formattedText.Data[i + 1].Char == '\n')
					{
						formattedText.RemoveAt(i + 1);
					}
				}
			}
		}

		private void ResolveCharsFonts()
		{
			for (int i = 0; i < formattedText.Count; i++)
			{
				formattedText.Data[i].ResolveFont(fontFamily);
			}
		}

		private void ComputeCharsWidths()
		{
			for (int i = 0; i < formattedText.Count; i++)
			{
				char nextChar = ((i < formattedText.Count - 1) ? formattedText.Data[i + 1].Char : '\0');
				formattedText.Data[i].Width = GetCharAdvance(formattedText.Data[i], nextChar);
			}
		}

		private void HandleLongText()
		{
			if (autoAdjustUnscaledFontSizeMin != 0)
			{
				AdjustFontSize();
			}
			if (autoTruncate)
			{
				Truncate();
			}
			else if (wordWrap)
			{
				Wrap();
			}
			if (maximumLines > 0)
			{
				ApplyMaxLines();
			}
		}

		private void AdjustFontSize()
		{
			uint num = usedUnscaledFontSize;
			if (usedUnscaledFontSize != unscaledFontSize)
			{
				usedUnscaledFontSize = unscaledFontSize;
				fontSize = ((usedRenderingMode == FontRenderingMode.DistanceField) ? usedUnscaledFontSize : ComputeScaledFontSize(usedUnscaledFontSize));
				ComputeCharsWidths();
				computedWidthCache = -1f;
			}
			float num2 = unscaledLocalRect.width + 0.01f;
			while (ComputeWidth() > num2 && usedUnscaledFontSize - 1 >= autoAdjustUnscaledFontSizeMin)
			{
				usedUnscaledFontSize--;
				fontSize = ((usedRenderingMode == FontRenderingMode.DistanceField) ? usedUnscaledFontSize : ComputeScaledFontSize(usedUnscaledFontSize));
				ComputeCharsWidths();
				computedWidthCache = -1f;
			}
			if (usedUnscaledFontSize != num)
			{
				fontFamily.NormalFont.GlobalMetrics(fontSize, out ascender, out descender, out corpusSize, out capHeight, out accentedCapHeight, out lineHeight, out underlinePosition, out underlineThickness);
			}
		}

		private void Truncate()
		{
			float num = 0f;
			float num2 = localRect.width + 0.01f;
			for (int i = 0; i < formattedText.Count; i++)
			{
				FormattedChar formattedChar = formattedText.Data[i];
				if (formattedChar.Char == '\n')
				{
					break;
				}
				if (num > 0f && num + formattedChar.Width > num2)
				{
					InsertTruncationChar('.', i, num);
					IsTruncated = true;
				}
				else
				{
					num += formattedChar.Width;
				}
			}
		}

		private void Wrap()
		{
			float num = 0f;
			int num2 = -1;
			for (int i = 0; i < formattedText.Count; i++)
			{
				FormattedChar other = formattedText.Data[i];
				if (other.Char == '\n')
				{
					num = 0f;
					num2 = -1;
					continue;
				}
				bool num3 = TextUtils.IsBreakableWhitespace(other.Char);
				if (num3)
				{
					num2 = i;
				}
				if (!num3 && num > 0f && num + other.Width > localRect.width + 0.01f)
				{
					FormattedChar item = new FormattedChar('\uf001');
					item.CopyFormattingFrom(other);
					item.ResolveFont(fontFamily);
					if (num2 > 0)
					{
						formattedText.Insert(num2 + 1, item);
						i = num2 + 1;
					}
					else
					{
						formattedText.Insert(i, item);
					}
					num = 0f;
					num2 = -1;
				}
				else
				{
					num += other.Width;
				}
			}
		}

		private float ComputeLineWidth(int lineStartIndex, bool trimLeft, bool trimRight)
		{
			float num = 0f;
			int i = lineStartIndex;
			if (trimLeft)
			{
				for (i = lineStartIndex; i < formattedText.Count; i++)
				{
					if (TextUtils.GetCharacterType(formattedText.Data[lineStartIndex].Char) != TextUtils.CharacterType.Space)
					{
						break;
					}
				}
			}
			int j;
			for (j = i; j < formattedText.Count; j++)
			{
				FormattedChar formattedChar = formattedText.Data[j];
				if (formattedChar.IsNewLineChar)
				{
					break;
				}
				num += formattedChar.Width;
			}
			if (trimRight)
			{
				int num2 = j - 1;
				while (num2 > i && TextUtils.GetCharacterType(formattedText.Data[num2].Char) == TextUtils.CharacterType.Space)
				{
					num -= formattedText.Data[num2].Width;
					num2--;
				}
			}
			return num;
		}

		private void AdjustSpaces()
		{
			int searchStartIndex = 0;
			int lineStartIndex;
			int lineEndIndex;
			while (AdjustSpaces_FindNextWrappedLine(searchStartIndex, out lineStartIndex, out lineEndIndex))
			{
				float num = localRect.width - ComputeLineWidth(lineStartIndex, trimLeft: false, trimRight: true);
				if (num >= 1f)
				{
					AdjustSpaces_FindAdjustableSpaces(lineStartIndex, lineEndIndex);
					if (adjustableSpacesIndexes.Count > 0)
					{
						float num2 = num / (float)adjustableSpacesIndexes.Count;
						if (usedRenderingMode == FontRenderingMode.Raster)
						{
							num2 = Mathf.FloorToInt(num2);
						}
						for (int i = 0; i < adjustableSpacesIndexes.Count; i++)
						{
							formattedText.Data[adjustableSpacesIndexes.Data[i]].Width += num2;
						}
						if (usedRenderingMode == FontRenderingMode.Raster)
						{
							num -= num2 * (float)adjustableSpacesIndexes.Count;
							for (int j = 0; j < adjustableSpacesIndexes.Count; j++)
							{
								if (!(num >= 1f))
								{
									break;
								}
								formattedText.Data[adjustableSpacesIndexes.Data[j]].Width += 1f;
								num -= 1f;
							}
						}
					}
				}
				searchStartIndex = lineEndIndex + 1;
			}
		}

		private bool AdjustSpaces_FindNextWrappedLine(int searchStartIndex, out int lineStartIndex, out int lineEndIndex)
		{
			lineStartIndex = searchStartIndex;
			lineEndIndex = -1;
			float num = 0f;
			for (int i = searchStartIndex; i < formattedText.Count; i++)
			{
				FormattedChar formattedChar = formattedText.Data[i];
				if (formattedChar.Char == '\n')
				{
					lineStartIndex = i + 1;
					lineEndIndex = -1;
					num = 0f;
					continue;
				}
				if (formattedChar.Char == '\uf001')
				{
					lineEndIndex = i;
					return true;
				}
				num += formattedChar.Width;
			}
			return false;
		}

		private void AdjustSpaces_FindAdjustableSpaces(int lineStartIndex, int lineEndIndex)
		{
			adjustableSpacesIndexes.Clear();
			lineEndIndex--;
			TrimSpacesLeft(ref lineStartIndex, lineEndIndex);
			TrimSpacesRight(lineStartIndex, ref lineEndIndex);
			for (int i = lineStartIndex + 1; i <= lineEndIndex; i++)
			{
				if (TextUtils.GetCharacterType(formattedText.Data[i].Char) == TextUtils.CharacterType.Space)
				{
					adjustableSpacesIndexes.Add(i);
				}
			}
		}

		private void ApplyMaxLines()
		{
			int num = 1;
			float num2 = 0f;
			for (int i = 0; i < formattedText.Count; i++)
			{
				FormattedChar formattedChar = formattedText.Data[i];
				if (formattedChar.IsNewLineChar)
				{
					if (num >= maximumLines)
					{
						InsertTruncationChar('…', i, num2);
						IsMultilineTruncated = true;
						break;
					}
					num++;
					num2 = 0f;
				}
				else
				{
					num2 += formattedChar.Width;
				}
			}
		}

		private void InsertChar(int insertionIndex, char newChar)
		{
			formattedText.Insert(insertionIndex, formattedText.Data[insertionIndex - 1]);
			formattedText.Data[insertionIndex].Char = newChar;
		}

		private void InsertTruncationChar(char truncationChar, int overflowIndex, float currentWidth)
		{
			float num = localRect.width + 0.01f;
			for (int num2 = overflowIndex; num2 > 0; num2--)
			{
				currentWidth -= formattedText.Data[num2 - 1].Width;
				if (num2 < overflowIndex && formattedText.Data[num2].IsNewLineChar)
				{
					break;
				}
				if (TextUtils.GetCharacterType(formattedText.Data[num2 - 1].Char) != TextUtils.CharacterType.Space)
				{
					formattedText.Data[num2] = new FormattedChar(truncationChar);
					formattedText.Data[num2].CopyFormattingFrom(formattedText.Data[num2 - 1]);
					formattedText.Data[num2].ResolveFont(fontFamily);
					formattedText.Data[num2].Width = GetCharAdvance(formattedText.Data[num2], '\0');
					formattedText.Data[num2 - 1].Width = GetCharAdvance(formattedText.Data[num2 - 1], truncationChar);
					if (currentWidth + formattedText.Data[num2 - 1].Width + formattedText.Data[num2].Width <= num)
					{
						formattedText.Count = num2 + 1;
						return;
					}
				}
			}
			Diagnostics.LogWarning("Failed to find enough space in '{0}' to insert the truncation char '{1}'.", rawText, truncationChar);
		}

		private void ComputeCharsPositions()
		{
			_ = capHeight;
			_ = descender;
			Vector2 penPosition = Vector2.zero;
			PlacePenAtTextBeginningVertically(ref penPosition);
			PlacePenAtLineBeginningHorizontally(0, ref penPosition);
			for (int i = 0; i < formattedText.Count; i++)
			{
				_ = usedRenderingMode;
				_ = 1;
				formattedText.Data[i].Position = penPosition;
				if (formattedText.Data[i].Char == '\n')
				{
					PlacePenAtLineBeginningHorizontally(i + 1, ref penPosition);
					penPosition.y += LineFeedVerticalAdvance;
				}
				else if (formattedText.Data[i].Char == '\uf001')
				{
					PlacePenAtLineBeginningHorizontally(i + 1, ref penPosition);
					penPosition.y += WordWrapVerticalAdvance;
				}
				else
				{
					penPosition.x += formattedText.Data[i].Width;
				}
			}
			endOfTextPosition = penPosition;
		}

		private void RebuildGlyphsCache(FontAtlasRenderer fontAtlasRenderer)
		{
			glyphsCacheDirty = false;
			ResizeOutputArrays();
			int num = 0;
			for (int i = 0; i < formattedText.Count; i++)
			{
				FormattedChar formattedChar = formattedText.Data[i];
				if (!formattedChar.IsNewLineChar)
				{
					if (formattedChar.Char == '\uf002')
					{
						AddImageToCache(formattedChar, num);
						num++;
					}
					else
					{
						char nextChar = ((i < formattedText.Count - 1) ? formattedText.Data[i + 1].Char : '\0');
						AddGlyphToCache(ref formattedChar, nextChar, num, fontAtlasRenderer);
						num++;
					}
				}
			}
		}

		private void ResizeOutputArrays()
		{
			int printableGlyphsCount = GetPrintableGlyphsCount();
			if (glyphsCache == null || glyphsCache.Length != printableGlyphsCount)
			{
				glyphsCache = new Data[printableGlyphsCount];
			}
			if (glyphsAtomId.IsValid)
			{
				UIAtomContainer<UIPrimitiveGlyphData>.Deallocate(ref glyphsAtomId);
			}
			if (printableGlyphsCount > 0)
			{
				glyphsAtomId = UIAtomContainer<UIPrimitiveGlyphData>.Allocate(printableGlyphsCount);
			}
			glyphCount = 0;
			symbolCount = 0;
		}

		private int GetPrintableGlyphsCount()
		{
			int num = 0;
			if (formattingDirty)
			{
				ReprocessFormatting();
			}
			for (int i = 0; i < formattedText.Count; i++)
			{
				if (!formattedText.Data[i].IsNewLineChar)
				{
					num++;
				}
			}
			return num;
		}

		private float ComputeLineWidthForPenPositionning(int lineBeginningIndex)
		{
			float num = 0f;
			if (formattingDirty)
			{
				ReprocessFormatting();
			}
			if (formattedText.Count == 0)
			{
				return 0f;
			}
			for (int i = lineBeginningIndex; i < formattedText.Count; i++)
			{
				FormattedChar formattedChar = formattedText.Data[i];
				if (formattedChar.IsNewLineChar)
				{
					if (justify || formattedChar.Char == '\uf001')
					{
						int num2 = i - 1;
						while (num2 > lineBeginningIndex && TextUtils.GetCharacterType(formattedText.Data[num2].Char) == TextUtils.CharacterType.Space)
						{
							num -= formattedText.Data[num2].Width;
							num2--;
						}
					}
					break;
				}
				num += formattedChar.Width;
			}
			return num;
		}

		private void PlacePenAtLineBeginningHorizontally(int lineBeginningIndex, ref Vector2 penPosition)
		{
			penPosition.x = localRect.xMin;
			if (Alignment.Horizontal == HorizontalAlignment.Right)
			{
				penPosition.x += localRect.width - ComputeLineWidthForPenPositionning(lineBeginningIndex);
			}
			else if (Alignment.Horizontal == HorizontalAlignment.Center)
			{
				penPosition.x += (localRect.width - ComputeLineWidthForPenPositionning(lineBeginningIndex)) * 0.5f;
			}
			if (usedRenderingMode == FontRenderingMode.Raster)
			{
				penPosition.x = Mathf.Round(penPosition.x);
			}
		}

		private void PlacePenAtTextBeginningVertically(ref Vector2 penPosition)
		{
			float num = GetLinesCount();
			float num2 = GetInterParagraphsCount();
			float num3 = (num - 1f) * ScaledInterLineAdditionalSpacing;
			float num4 = num2 * ScaledInterParagraphAdditionalSpacing;
			float num5 = (num - 1f) * DistanceBetweenBaselines + num3 + num4;
			float num6 = (UseAscent ? (-1f) : descender);
			penPosition.y = localRect.yMin;
			if (Alignment.Vertical == VerticalAlignment.Top)
			{
				penPosition.y += accentedCapHeight;
				if (usedRenderingMode == FontRenderingMode.Raster)
				{
					penPosition.y = Mathf.Ceil(penPosition.y);
				}
			}
			else if (Alignment.Vertical == VerticalAlignment.Bottom)
			{
				penPosition.y += localRect.height - num5 + num6;
				if (usedRenderingMode == FontRenderingMode.Raster)
				{
					penPosition.y = Mathf.Floor(penPosition.y);
				}
			}
			else if (Alignment.Vertical == VerticalAlignment.Center)
			{
				penPosition.y += (localRect.height - num5 + capHeight + num6) * 0.5f;
				if (usedRenderingMode == FontRenderingMode.Raster)
				{
					penPosition.y = Mathf.Ceil(penPosition.y);
				}
			}
		}

		private void AddGlyphToCache(ref FormattedChar formattedChar, char nextChar, int glyphIndex, FontAtlasRenderer fontAtlasRenderer)
		{
			if (char.IsWhiteSpace(formattedChar.Char))
			{
				UIPrimitiveGlyphData data = default(UIPrimitiveGlyphData);
				data.Position = new Vector4(0f, 0f, 0f, 0f);
				data.Color = formattedChar.Color;
				data.Texcoords = Vector4.zero;
				data.TransformId = transformAtomId.Index;
				data.MaterialPropertyOverridesId = materialPropertyOverridesAtomId.Index;
				UIAtomContainer<UIPrimitiveGlyphData>.SetData(ref glyphsAtomId, glyphIndex, ref data);
				glyphCount++;
				glyphsCache[glyphIndex].FillGlyph();
				float width = formattedChar.Width;
			}
			else
			{
				Rect orCreateInAtlas = fontAtlasRenderer.GetOrCreateInAtlas(formattedChar.Char, formattedChar.Font, fontSize, usedRenderingMode);
				fontAtlasRenderer.GetGlyphPositioning(formattedChar.Char, formattedChar.Font, fontSize, usedRenderingMode, out var bearing, out var widthHeight, out var width);
				Vector2 vector = formattedChar.Position + bearing;
				UIPrimitiveGlyphData data2 = default(UIPrimitiveGlyphData);
				data2.Position = new Vector4(vector.x, vector.y, widthHeight.x, widthHeight.y);
				data2.Color = formattedChar.Color;
				data2.Texcoords = new Vector4(orCreateInAtlas.xMin, orCreateInAtlas.yMin, orCreateInAtlas.width, orCreateInAtlas.height);
				data2.TransformId = transformAtomId.Index;
				data2.MaterialPropertyOverridesId = materialPropertyOverridesAtomId.Index;
				UIAtomContainer<UIPrimitiveGlyphData>.SetData(ref glyphsAtomId, glyphIndex, ref data2);
				glyphCount++;
				glyphsCache[glyphIndex].FillGlyph();
				width = formattedChar.Width;
			}
		}

		private void AddImageToCache(FormattedChar formattedChar, int glyphIndex)
		{
			int userDefined = formattedChar.UserDefined;
			SymbolToImageMapper symbolToImageMapper = UIRenderingManager.Instance.SymbolMapperController.GetSymbolMapper(userDefined) as SymbolToImageMapper;
			Vector4 position = new Vector4(formattedChar.Position.x, formattedChar.Position.y - ImageSymbolSquareSize, ImageSymbolSquareSize, ImageSymbolSquareSize);
			formattedChar.Position.x += symbolToImageMapper.GetAdvance(ImageSymbolSquareSize);
			glyphsCache[glyphIndex].FillSymbol(position, formattedChar.Color, userDefined);
			symbolCount++;
		}

		private float GetCharAdvance(FormattedChar formattedChar, char nextChar)
		{
			if (formattedChar.Char == '\n')
			{
				return 0f;
			}
			float num;
			if (formattedChar.Char == '\t')
			{
				num = formattedChar.Font.GetCharAdvance(' ', fontSize, usedRenderingMode) * 4f;
			}
			else
			{
				if (formattedChar.Char == '\uf002')
				{
					return (UIRenderingManager.Instance.SymbolMapperController.GetSymbolMapper(formattedChar.UserDefined) as SymbolToImageMapper).GetAdvance(ImageSymbolSquareSize);
				}
				num = formattedChar.Font.GetCharAdvance(formattedChar.Char, fontSize, usedRenderingMode);
			}
			if (nextChar != 0)
			{
				num += formattedChar.Font.GetKerningOffset(formattedChar.Char, nextChar, fontSize, usedRenderingMode);
				num += interLetterAdditionalSpacing;
			}
			if (usedRenderingMode == FontRenderingMode.Raster)
			{
				num = Mathf.Round(num);
			}
			return num;
		}

		private void RebuildStyleRectsCache()
		{
			styleRectsCacheDirty = false;
			styleRectsCache.Clear();
			if (interpreteRichText && formattedText.Count != 0)
			{
				float thickness = capHeight - descender;
				int styleStartIndex = -1;
				int styleStartIndex2 = -1;
				int styleStartIndex3 = -1;
				Color32 right = default(Color32);
				for (int i = 0; i < formattedText.Count; i++)
				{
					bool hasChangedColor = !Color32Utils.Equals(formattedText.Data[i].Color, right);
					bool isNewLineChar = formattedText.Data[i].IsNewLineChar;
					ParseStyle(TextStyle.Underline, ref styleStartIndex, i, hasChangedColor, isNewLineChar, 0f - underlinePosition, underlineThickness);
					ParseStyle(TextStyle.StrikeThrough, ref styleStartIndex2, i, hasChangedColor, isNewLineChar, underlinePosition * 2f, underlineThickness);
					ParseColoredStyle(TextStyle.Highlight, ref styleStartIndex3, i, isNewLineChar, 0f - capHeight - 1f, thickness);
					right = formattedText.Data[i].Color;
				}
			}
		}

		private void ParseStyle(TextStyle style, ref int styleStartIndex, int currentIndex, bool hasChangedColor, bool isNewLine, float offsetFromBaseline, float thickness)
		{
			bool flag = (formattedText.Data[currentIndex].Style & style) != 0;
			if (styleStartIndex >= 0)
			{
				if (!flag || hasChangedColor || isNewLine)
				{
					AddRectColored(formattedText, ref styleRectsCache, styleStartIndex, currentIndex - 1, offsetFromBaseline, thickness, formattedText.Data[styleStartIndex].Color);
					styleStartIndex = ((flag && !isNewLine) ? currentIndex : (-1));
				}
				else if (currentIndex == formattedText.Count - 1)
				{
					AddRectColored(formattedText, ref styleRectsCache, styleStartIndex, currentIndex, offsetFromBaseline, thickness, formattedText.Data[styleStartIndex].Color);
				}
			}
			else if (flag)
			{
				styleStartIndex = currentIndex;
			}
		}

		private void ParseColoredStyle(TextStyle style, ref int styleStartIndex, int currentIndex, bool isNewLine, float offsetFromBaseline, float thickness)
		{
			bool flag = (formattedText.Data[currentIndex].Style & style) != 0;
			if (styleStartIndex >= 0)
			{
				if (!flag || isNewLine)
				{
					AddRectColored(formattedText, ref styleRectsCache, styleStartIndex, currentIndex - 1, offsetFromBaseline, thickness, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 64));
					styleStartIndex = ((flag && !isNewLine) ? currentIndex : (-1));
				}
				else if (currentIndex == formattedText.Count - 1)
				{
					AddRectColored(formattedText, ref styleRectsCache, styleStartIndex, currentIndex, offsetFromBaseline, thickness, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 64));
				}
			}
			else if (flag)
			{
				styleStartIndex = currentIndex;
			}
		}

		private void AddRectColored(PerformanceList<FormattedChar> formattedChars, ref PerformanceList<RectColored> rectsColored, int styleFirstIndex, int styleLastIndex, float offsetFromBaseline, float height, Color32 color)
		{
			if (styleLastIndex == formattedChars.Count - 1 || formattedChars.Data[styleLastIndex + 1].IsNewLineChar)
			{
				TrimSpacesRight(styleFirstIndex, ref styleLastIndex);
			}
			Vector2 position = formattedChars.Data[styleFirstIndex].Position;
			Vector2 position2 = formattedChars.Data[styleLastIndex].Position;
			float width = formattedChars.Data[styleLastIndex].Width;
			Rect internalRect = new Rect(position.x, position.y + offsetFromBaseline, position2.x + width - position.x, height);
			Rect rect = MakeInternalRectLocal(internalRect);
			if (usedRenderingMode == FontRenderingMode.Raster)
			{
				rect.x = Mathf.Round(rect.x);
				rect.y = Mathf.Round(rect.y);
				rect.width = Mathf.Round(rect.width);
				rect.height = Mathf.Round(rect.height);
			}
			rectsColored.Add(new RectColored(rect, color));
		}

		private void RebuildEditableText()
		{
			editableText.Clear();
			for (int i = 0; i < formattedText.Count; i++)
			{
				FormattedChar formattedChar = formattedText.Data[i];
				if (formattedChar.Char != '\uf001')
				{
					editableText.Add(formattedChar);
					if (formattedChar.Char == '\n')
					{
						FormattedChar formattedChar2 = new FormattedChar(' ');
						formattedChar2.CopyFormattingFrom(formattedChar);
						formattedChar2.ResolveFont(fontFamily);
						editableText.Data[editableText.Count - 1].Width = GetCharAdvance(formattedChar2, '\0');
					}
				}
			}
			FormattedChar item = new FormattedChar('\0');
			item.Position = endOfTextPosition;
			editableText.Add(item);
		}

		private void RebuildLinesStartIndexes()
		{
			editableLinesBeginnings.Clear();
			editableLinesBeginnings.Add(0);
			for (int i = 0; i < editableText.Count; i++)
			{
				if (i > 0 && editableText.Data[i].Position.y > editableText.Data[i - 1].Position.y)
				{
					editableLinesBeginnings.Add(i);
				}
			}
		}

		private Vector2 MakeInternalPositionLocal(Vector2 internalPosition)
		{
			if (usedRenderingMode == FontRenderingMode.Raster)
			{
				UIView uIView = View;
				Vector4 vector = GL.GetGPUProjectionMatrix(uIView.ProjectionMatrix, renderIntoTexture: false).inverse * uIView.ScreenToClipMatrix * internalPosition.xy01();
				return (localToGlobalMatrix.inverse * vector).xy();
			}
			return new Vector2(Mathf.Ceil(internalPosition.x), Mathf.Ceil(internalPosition.y));
		}

		private Rect MakeInternalRectLocal(Rect internalRect)
		{
			Vector2 internalPosition = new Vector2(internalRect.x, internalRect.y);
			Vector2 internalPosition2 = new Vector2(internalRect.xMax, internalRect.yMax);
			Vector2 vector = MakeInternalPositionLocal(internalPosition);
			Vector2 vector2 = MakeInternalPositionLocal(internalPosition2);
			return new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
		}

		private Vector2 MakeLocalPositionInternal(Vector2 localPosition)
		{
			if (usedRenderingMode == FontRenderingMode.Raster)
			{
				Vector4 vector = localToGlobalMatrix * localPosition.xy01();
				return (View.GlobalToScreenMatrix * vector).xy();
			}
			return localPosition;
		}

		private void TrimSpacesLeft(ref int firstIndex, int lastIndex)
		{
			while (firstIndex < lastIndex && TextUtils.GetCharacterType(formattedText.Data[firstIndex].Char) == TextUtils.CharacterType.Space)
			{
				firstIndex++;
			}
		}

		private void TrimSpacesRight(int firstIndex, ref int lastIndex)
		{
			while (lastIndex >= firstIndex && TextUtils.GetCharacterType(formattedText.Data[lastIndex].Char) == TextUtils.CharacterType.Space)
			{
				lastIndex--;
			}
		}
	}
}
