using System;
using Amplitude.Framework.Localization;
using Amplitude.Graphics.Text;
using Amplitude.UI.Animations;
using Amplitude.UI.Styles;
using Amplitude.UI.Styles.Scene;
using Amplitude.UI.Text;
using Amplitude.UI.Traits;
using UnityEngine;
using UnityEngine.Serialization;

namespace Amplitude.UI.Renderers
{
	[RequireComponent(typeof(UITransform))]
	[ExecuteInEditMode]
	public class UILabel : UIRenderer, IUIResolutionDependent, IUILocalizationDependent, IUIMaterialPropertyOverridesProvider, IUITraitColor, IUITrait<Color>, IUITraitFont, IUITrait<FontFamily>, IUITrait<FontFace>, IUITrait<int>, IUITrait<bool>, IUITrait<FontRenderingMode>, IUITraitRectMargins, IUITrait<RectMargins>, IUITraitHorizontalAlignment, IUITrait<Alignment>, IUITraitVerticalAlignment, IUITraitTextPositionning
	{
		public static class Mutators
		{
			public static readonly MutatorSet<UILabel, FontFamily> FontFamily = new MutatorSet<UILabel, FontFamily>(ItemIdentifiers.FontFamily, (UILabel t) => t.fontFamily, delegate(UILabel t, FontFamily value)
			{
				t.FontFamily = value;
			});

			public static readonly MutatorSet<UILabel, FontFace> FontFace = new MutatorSet<UILabel, FontFace>(ItemIdentifiers.FontFace, (UILabel t) => t.fontFace, delegate(UILabel t, FontFace value)
			{
				t.FontFace = value;
			});

			public static readonly MutatorSet<UILabel, uint> FontSize = new MutatorSet<UILabel, uint>(ItemIdentifiers.FontSize, (UILabel t) => t.fontSize, delegate(UILabel t, uint value)
			{
				t.FontSize = value;
			});

			public static readonly MutatorSet<UILabel, Color> Color = new MutatorSet<UILabel, Color>(ItemIdentifiers.Color, (UILabel t) => t.Color, delegate(UILabel t, Color value)
			{
				t.Color = value;
			});

			public static readonly MutatorSet<UILabel, FontRenderingMode> RenderingMode = new MutatorSet<UILabel, FontRenderingMode>(ItemIdentifiers.RenderingMode, (UILabel t) => t.RenderingMode, delegate(UILabel t, FontRenderingMode value)
			{
				t.RenderingMode = value;
			});

			public static readonly MutatorSet<UILabel, HorizontalAlignment> HorizontalAlignment = new MutatorSet<UILabel, HorizontalAlignment>(ItemIdentifiers.HorizontalAlignment, (UILabel t) => t.alignment.Horizontal, delegate(UILabel t, HorizontalAlignment value)
			{
				t.SetHorizontalAlignment(value);
			});

			public static readonly MutatorSet<UILabel, VerticalAlignment> VerticalAlignment = new MutatorSet<UILabel, VerticalAlignment>(ItemIdentifiers.VerticalAlignment, (UILabel t) => t.alignment.Vertical, delegate(UILabel t, VerticalAlignment value)
			{
				t.SetVerticalAlignment(value);
			});

			public static readonly MutatorSet<UILabel, bool> UseAscent = new MutatorSet<UILabel, bool>(ItemIdentifiers.UseAscent, (UILabel t) => t.useAscent, delegate(UILabel t, bool value)
			{
				t.UseAscent = value;
			});

			public static readonly MutatorSet<UILabel, bool> Justify = new MutatorSet<UILabel, bool>(ItemIdentifiers.Justify, (UILabel t) => t.justify, delegate(UILabel t, bool value)
			{
				t.Justify = value;
			});

			public static readonly MutatorSet<UILabel, bool> ForceCaps = new MutatorSet<UILabel, bool>(ItemIdentifiers.ForceCaps, (UILabel t) => t.forceCaps, delegate(UILabel t, bool value)
			{
				t.ForceCaps = value;
			});

			public static readonly MutatorSet<UILabel, uint> AutoAdjustFontSizeMin = new MutatorSet<UILabel, uint>(ItemIdentifiers.AutoAdjustFontSizeMin, (UILabel t) => t.AutoAdjustFontSizeMin, delegate(UILabel t, uint value)
			{
				t.AutoAdjustFontSizeMin = value;
			});

			public static readonly MutatorSet<UILabel, bool> AutoTruncate = new MutatorSet<UILabel, bool>(ItemIdentifiers.AutoTruncate, (UILabel t) => t.AutoTruncate, delegate(UILabel t, bool value)
			{
				t.AutoTruncate = value;
			});

			public static readonly MutatorSet<UILabel, bool> WordWrap = new MutatorSet<UILabel, bool>(ItemIdentifiers.WordWrap, (UILabel t) => t.WordWrap, delegate(UILabel t, bool value)
			{
				t.WordWrap = value;
			});

			public static readonly MutatorSet<UILabel, bool> AdjustWidth = new MutatorSet<UILabel, bool>(ItemIdentifiers.AdjustWidth, (UILabel t) => t.AutoAdjustWidth, delegate(UILabel t, bool value)
			{
				t.AutoAdjustWidth = value;
			});

			public static readonly MutatorSet<UILabel, float> AdjustWidthMin = new MutatorSet<UILabel, float>(ItemIdentifiers.AdjustWidthMin, (UILabel t) => t.AutoAdjustWidthMin, delegate(UILabel t, float value)
			{
				t.AutoAdjustWidthMin = value;
			});

			public static readonly MutatorSet<UILabel, float> AdjustWidthMax = new MutatorSet<UILabel, float>(ItemIdentifiers.AdjustWidthMax, (UILabel t) => t.AutoAdjustWidthMax, delegate(UILabel t, float value)
			{
				t.AutoAdjustWidthMax = value;
			});

			public static readonly MutatorSet<UILabel, bool> AdjustHeight = new MutatorSet<UILabel, bool>(ItemIdentifiers.AdjustHeight, (UILabel t) => t.AutoAdjustHeight, delegate(UILabel t, bool value)
			{
				t.AutoAdjustHeight = value;
			});

			public static readonly MutatorSet<UILabel, int> InterLetterAdditionalSpacing = new MutatorSet<UILabel, int>(ItemIdentifiers.InterLetterAdditionalSpacing, (UILabel t) => t.InterLetterAdditionalSpacing, delegate(UILabel t, int value)
			{
				t.interLetterAdditionalSpacing = value;
			});

			public static readonly MutatorSet<UILabel, int> InterLineAdditionalSpacing = new MutatorSet<UILabel, int>(ItemIdentifiers.InterLineAdditionalSpacing, (UILabel t) => t.InterLineAdditionalSpacing, delegate(UILabel t, int value)
			{
				t.interLineAdditionalSpacing = value;
			});

			public static readonly MutatorSet<UILabel, int> InterParagraphAdditionalSpacing = new MutatorSet<UILabel, int>(ItemIdentifiers.InterParagraphAdditionalSpacing, (UILabel t) => t.InterParagraphAdditionalSpacing, delegate(UILabel t, int value)
			{
				t.interParagraphAdditionalSpacing = value;
			});

			public static readonly MutatorSet<UILabel, TextStyle> TextStyle = new MutatorSet<UILabel, TextStyle>(ItemIdentifiers.TextStyle, (UILabel t) => t.TextStyle, delegate(UILabel t, TextStyle value)
			{
				t.textStyle = value;
			});

			public static readonly MutatorSet<UILabel, bool> InterpreteRichText = new MutatorSet<UILabel, bool>(ItemIdentifiers.InterpreteRichText, (UILabel t) => t.InterpreteRichText, delegate(UILabel t, bool value)
			{
				t.InterpreteRichText = value;
			});

			public static readonly MutatorSet<UILabel, bool> InterpreteSymbols = new MutatorSet<UILabel, bool>(ItemIdentifiers.InterpreteSymbols, (UILabel t) => t.InterpreteSymbols, delegate(UILabel t, bool value)
			{
				t.InterpreteSymbols = value;
			});

			public static readonly MutatorSet<UILabel, float> TopMargin = new MutatorSet<UILabel, float>(ItemIdentifiers.TopMargin, (UILabel t) => t.margins.Top, delegate(UILabel t, float value)
			{
				t.Margins = new RectMargins(t.margins.Left, t.margins.Right, value, t.margins.Bottom);
			});

			public static readonly MutatorSet<UILabel, float> BottomMargin = new MutatorSet<UILabel, float>(ItemIdentifiers.BottomMargin, (UILabel t) => t.margins.Bottom, delegate(UILabel t, float value)
			{
				t.Margins = new RectMargins(t.margins.Left, t.margins.Right, t.margins.Top, value);
			});

			public static readonly MutatorSet<UILabel, float> LeftMargin = new MutatorSet<UILabel, float>(ItemIdentifiers.LeftMargin, (UILabel t) => t.margins.Left, delegate(UILabel t, float value)
			{
				t.Margins = new RectMargins(value, t.margins.Right, t.margins.Top, t.margins.Bottom);
			});

			public static readonly MutatorSet<UILabel, float> RightMargin = new MutatorSet<UILabel, float>(ItemIdentifiers.RightMargin, (UILabel t) => t.margins.Right, delegate(UILabel t, float value)
			{
				t.Margins = new RectMargins(t.margins.Left, value, t.margins.Top, t.margins.Bottom);
			});
		}

		public const int FontSizeIfZero = 20;

		public static Color DefaultHighlightColor = new Color(1f, 1f, 1f, 0.2f);

		private const string DefaultMaterialName = "Default";

		private const float Epsilon = 0.01f;

		[SerializeField]
		private string text = string.Empty;

		[SerializeField]
		[UIMaterialId(UIPrimitiveType.Glyph)]
		private UIMaterialId material;

		[SerializeField]
		private UIMaterialPropertyOverrides materialPropertyOverrides;

		[SerializeField]
		private FontFamily fontFamily;

		[SerializeField]
		private FontFace fontFace;

		[SerializeField]
		private uint fontSize = 20u;

		[SerializeField]
		private Color color = Color.white;

		[SerializeField]
		private FontRenderingMode renderingMode;

		[SerializeField]
		private Alignment alignment = Alignment.CenterCenter;

		[SerializeField]
		private bool useAscent;

		[SerializeField]
		private bool justify;

		[SerializeField]
		private bool forceCaps;

		[SerializeField]
		private bool autoTruncate;

		[SerializeField]
		private bool wordWrap;

		[SerializeField]
		private bool autoAdjustWidth;

		[SerializeField]
		private float autoAdjustWidthMin;

		[SerializeField]
		[FormerlySerializedAs("autoAdjustMaxWidth")]
		private float autoAdjustWidthMax;

		[SerializeField]
		private bool autoAdjustHeight;

		[SerializeField]
		private uint autoAdjustFontSizeMin;

		[SerializeField]
		private RectMargins margins = RectMargins.None;

		[SerializeField]
		private int interLetterAdditionalSpacing;

		[SerializeField]
		private int interLineAdditionalSpacing;

		[SerializeField]
		private int interParagraphAdditionalSpacing;

		[SerializeField]
		private int maximumLines;

		[SerializeField]
		private TextStyle textStyle;

		[SerializeField]
		private bool interpreteRichText = true;

		[SerializeField]
		private bool interpreteSymbols = true;

		[SerializeField]
		private Color highlightColor = DefaultHighlightColor;

		[SerializeField]
		private UIBlendType blendType;

		[NonSerialized]
		private ProcessedText processedText;

		[NonSerialized]
		private Material resolvedMaterial;

		[NonSerialized]
		private FontRenderingMode usedRenderingMode;

		[NonSerialized]
		private PerformanceList<RectColored> selectionRects;

		private int nestedCallsToAdjustSizes;

		public UIBlendType BlendType
		{
			get
			{
				return blendType;
			}
			set
			{
				blendType = value;
			}
		}

		ref UIMaterialPropertyOverrides IUIMaterialPropertyOverridesProvider.MaterialPropertyOverrides => ref materialPropertyOverrides;

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				string text = ((value != null) ? value : string.Empty);
				if (this.text != text)
				{
					this.text = text;
					if (base.Loaded)
					{
						processedText.RawText = LocalizeIfNecessary(this.text);
						AdjustSizesIfNecessary();
					}
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
				if (!(fontFamily != value))
				{
					return;
				}
				fontFamily = value;
				if (base.Loaded)
				{
					if (fontFamily == null)
					{
						Diagnostics.LogWarning("Trying to assign a null font family to '{0}'.", this);
					}
					processedText.FontFamily = fontFamily;
					AdjustSizesIfNecessary();
				}
			}
		}

		public FontFace FontFace
		{
			get
			{
				return fontFace;
			}
			set
			{
				if (fontFace != value)
				{
					fontFace = value;
					if (base.Loaded)
					{
						processedText.DefaultFontFace = fontFace;
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public uint FontSize
		{
			get
			{
				return fontSize;
			}
			set
			{
				if (fontSize != value)
				{
					fontSize = value;
					if (autoAdjustFontSizeMin != 0 && autoAdjustFontSizeMin > fontSize)
					{
						Diagnostics.LogWarning("The min value {0} for auto-adjusting font size is higher than the base value {1}", autoAdjustFontSizeMin, fontSize);
					}
					if (base.Loaded)
					{
						processedText.UnscaledFontSize = fontSize;
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public Color Color
		{
			get
			{
				return color;
			}
			set
			{
				if (color != value)
				{
					color = value;
					if (base.Loaded)
					{
						processedText.DefaultColor = color;
					}
				}
			}
		}

		public FontRenderingMode RenderingMode
		{
			get
			{
				return renderingMode;
			}
			set
			{
				if (renderingMode != value)
				{
					renderingMode = value;
					if (base.Loaded)
					{
						processedText.WantedRenderingMode = renderingMode;
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public RectMargins Margins
		{
			get
			{
				return margins;
			}
			set
			{
				if (margins != value)
				{
					margins = value;
					if (base.Loaded)
					{
						processedText.UnscaledLocalRect = GetLocalRectWithMargins();
						AdjustSizesIfNecessary();
					}
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
					if (base.Loaded)
					{
						processedText.Alignment = alignment;
					}
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
					if (base.Loaded)
					{
						processedText.UseAscent = useAscent;
					}
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
					if (!wordWrap)
					{
						Diagnostics.LogWarning("Cannot justify a text without word wrap.");
					}
					if (base.Loaded)
					{
						processedText.Justify = justify;
					}
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
					if (base.Loaded)
					{
						processedText.ForceCaps = forceCaps;
						AdjustSizesIfNecessary();
					}
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
				if (autoTruncate == value)
				{
					return;
				}
				autoTruncate = value;
				if (autoTruncate)
				{
					wordWrap = false;
					if (autoAdjustWidth && autoAdjustWidthMax < float.Epsilon)
					{
						autoAdjustWidth = false;
					}
				}
				if (base.Loaded)
				{
					processedText.WordWrap = wordWrap;
					processedText.AutoTruncate = autoTruncate;
					AdjustSizesIfNecessary();
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
					if (wordWrap)
					{
						autoAdjustFontSizeMin = 0u;
						autoTruncate = false;
					}
					if (base.Loaded)
					{
						processedText.AutoAdjustUnscaledFontSizeMin = autoAdjustFontSizeMin;
						processedText.AutoTruncate = autoTruncate;
						processedText.WordWrap = wordWrap;
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public bool AutoAdjustWidth
		{
			get
			{
				return autoAdjustWidth;
			}
			set
			{
				if (autoAdjustWidth != value)
				{
					autoAdjustWidth = value;
					if (autoAdjustWidth && autoAdjustWidthMax < float.Epsilon)
					{
						wordWrap = false;
						autoTruncate = false;
					}
					if (base.Loaded)
					{
						processedText.AutoTruncate = autoTruncate;
						processedText.WordWrap = wordWrap;
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public float AutoAdjustWidthMin
		{
			get
			{
				return autoAdjustWidthMin;
			}
			set
			{
				if (autoAdjustWidthMin != value)
				{
					autoAdjustWidthMin = value;
					if (base.Loaded)
					{
						processedText.UnscaledLocalRect = GetLocalRectWithMargins();
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public float AutoAdjustWidthMax
		{
			get
			{
				return autoAdjustWidthMax;
			}
			set
			{
				if (autoAdjustWidthMax != value)
				{
					autoAdjustWidthMax = value;
					if (autoAdjustWidth && autoAdjustWidthMax < float.Epsilon)
					{
						wordWrap = false;
						autoTruncate = false;
					}
					if (base.Loaded)
					{
						processedText.UnscaledLocalRect = GetLocalRectWithMargins();
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public bool AutoAdjustHeight
		{
			get
			{
				return autoAdjustHeight;
			}
			set
			{
				if (autoAdjustHeight != value)
				{
					autoAdjustHeight = value;
					if (base.Loaded)
					{
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public uint AutoAdjustFontSizeMin
		{
			get
			{
				return autoAdjustFontSizeMin;
			}
			set
			{
				if (autoAdjustFontSizeMin != value)
				{
					autoAdjustFontSizeMin = value;
					if (autoAdjustFontSizeMin > fontSize)
					{
						Diagnostics.LogWarning("The min value {0} for auto-adjusting font size is higher than the base value {1}", autoAdjustFontSizeMin, fontSize);
					}
					if (base.Loaded)
					{
						processedText.AutoAdjustUnscaledFontSizeMin = autoAdjustFontSizeMin;
						processedText.WordWrap = wordWrap;
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public int InterLetterAdditionalSpacing
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
					if (base.Loaded)
					{
						processedText.InterLetterAdditionalSpacing = interLetterAdditionalSpacing;
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public int InterLineAdditionalSpacing
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
					if (base.Loaded)
					{
						processedText.InterLineAdditionalSpacing = interLineAdditionalSpacing;
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public int InterParagraphAdditionalSpacing
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
					if (base.Loaded)
					{
						processedText.InterParagraphAdditionalSpacing = interParagraphAdditionalSpacing;
						AdjustSizesIfNecessary();
					}
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
					if (base.Loaded)
					{
						processedText.MaximumLines = maximumLines;
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public TextStyle TextStyle
		{
			get
			{
				return textStyle;
			}
			set
			{
				if (textStyle != value)
				{
					textStyle = value;
					if (base.Loaded)
					{
						processedText.DefaultTextStyle = textStyle;
					}
				}
			}
		}

		public Color HighlightColor
		{
			get
			{
				return highlightColor;
			}
			set
			{
				if (highlightColor != value)
				{
					highlightColor = value;
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
					if (base.Loaded)
					{
						processedText.InterpreteRichText = interpreteRichText;
						AdjustSizesIfNecessary();
					}
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
					if (base.Loaded)
					{
						processedText.InterpreteSymbols = interpreteSymbols;
						AdjustSizesIfNecessary();
					}
				}
			}
		}

		public float TextWidthWithMargins
		{
			get
			{
				Rect unscaledLocalRect = processedText.UnscaledLocalRect;
				processedText.UnscaledLocalRect = GetLocalRectWithMargins();
				float result = processedText.ComputeWidth() + Margins.Left + Margins.Right;
				processedText.UnscaledLocalRect = unscaledLocalRect;
				return result;
			}
		}

		public float TextHeightWithMargins
		{
			get
			{
				Rect unscaledLocalRect = processedText.UnscaledLocalRect;
				processedText.UnscaledLocalRect = GetLocalRectWithMargins();
				float result = processedText.ComputeHeight() + Margins.Top + Margins.Bottom;
				processedText.UnscaledLocalRect = unscaledLocalRect;
				return result;
			}
		}

		public bool IsTruncated => processedText.IsTruncated;

		public bool IsMultilineTruncated => processedText.IsMultilineTruncated;

		public uint GetUsedFontSize()
		{
			return processedText.GetUsedUnscaledFontSize();
		}

		public void OnResolutionChanged(float oldWidth, float oldHeight, float newWidth, float newHeight)
		{
			if (base.Loaded)
			{
				processedText.OnPositioningContextChanged();
				AdjustSizesIfNecessary();
			}
		}

		public void OnLocalizationChanged()
		{
			if (base.Loaded)
			{
				processedText.RawText = LocalizeIfNecessary(text);
				AdjustSizesIfNecessary();
			}
		}

		public Vector2 GetCharPosition(int charIndex)
		{
			return processedText.GetCharPosition(charIndex);
		}

		public int GetCharAtPosition(Vector2 position, int lineOffset = 0)
		{
			return processedText.GetCharAtPosition(position, lineOffset);
		}

		public void SelectText(int selectionStartIndex, int selectionLength)
		{
			if (selectionStartIndex >= 0 && selectionLength > 0)
			{
				selectionRects.Clear();
				processedText.SelectText(selectionStartIndex, selectionLength, ref selectionRects);
			}
			else
			{
				selectionRects.ClearReleaseMemory();
			}
		}

		public override void CreateAnimationItems(IUIAnimationItemsCollection animationItemsCollection)
		{
			base.CreateAnimationItems(animationItemsCollection);
			animationItemsCollection.Add(Mutators.Color);
			animationItemsCollection.Add(Mutators.AdjustWidthMax);
			if (materialPropertyOverrides.Empty)
			{
				return;
			}
			int num = materialPropertyOverrides.Items.Length;
			for (int i = 0; i < num; i++)
			{
				UIMaterialPropertyOverride uIMaterialPropertyOverride = materialPropertyOverrides.Items[i];
				StaticString staticString = new StaticString(uIMaterialPropertyOverride.Name);
				int elementIndex = i;
				switch (uIMaterialPropertyOverride.Type)
				{
				case UIMaterialPropertyOverride.PropertyType.Float:
					IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, staticString, (UILabel t) => t.materialPropertyOverrides.GetFloatValue(elementIndex), delegate(UILabel t, float x)
					{
						t.materialPropertyOverrides.SetFloatValue(elementIndex, x);
					});
					break;
				case UIMaterialPropertyOverride.PropertyType.Vector:
					IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, staticString, (UILabel t) => t.materialPropertyOverrides.GetVectorValue(elementIndex), delegate(UILabel t, Vector3 x)
					{
						t.materialPropertyOverrides.SetVectorValue(elementIndex, x);
					});
					break;
				case UIMaterialPropertyOverride.PropertyType.Color:
					IUIAnimationItemsCollectionHelper.Add(animationItemsCollection, staticString, (UILabel t) => t.materialPropertyOverrides.GetColorValue(elementIndex), delegate(UILabel t, Color x)
					{
						t.materialPropertyOverrides.SetColorValue(elementIndex, x);
					});
					break;
				}
			}
		}

		protected override void OnTransformGlobalPositionChanged()
		{
			if (base.Loaded)
			{
				processedText.UpdateLocalToGlobalMatrix(UITransform.LocalToGlobalMatrix);
			}
		}

		protected override void OnTransformLocalRectChanged()
		{
			if (base.Loaded)
			{
				processedText.UnscaledLocalRect = GetLocalRectWithMargins();
				AdjustSizesIfNecessary();
			}
		}

		protected override void OnTransformVisibleGloballyChanged(bool previouslyVisible, bool currentlyVisible)
		{
			base.OnTransformVisibleGloballyChanged(previouslyVisible, currentlyVisible);
			if (base.Loaded && currentlyVisible)
			{
				AdjustSizesIfNecessary();
			}
		}

		protected override void Render(UIPrimitiveDrawer drawer)
		{
			if (!string.IsNullOrEmpty(text))
			{
				if (usedRenderingMode != processedText.UsedRenderingMode)
				{
					usedRenderingMode = processedText.UsedRenderingMode;
					ResolveMaterial();
				}
				if (Color.a != 0f)
				{
					drawer.Label(resolvedMaterial, processedText, blendType);
				}
				PerformanceList<RectColored> styleRects = processedText.GetStyleRects();
				for (int i = 0; i < styleRects.Count; i++)
				{
					Color color = styleRects.Data[i].Color;
					drawer.Rect(ref UITransform.MatrixAtomId, ref styleRects.Data[i].Rect, UITexture.White, ref color, blendType);
				}
				for (int j = 0; j < selectionRects.Count; j++)
				{
					Color color2 = selectionRects.Data[j].Color;
					drawer.Rect(ref UITransform.MatrixAtomId, ref selectionRects.Data[j].Rect, UITexture.White, ref color2, blendType);
				}
			}
		}

		protected override void Load()
		{
			base.Load();
			UIServiceAccessManager.Instance?.LoadIfNecessary();
			materialPropertyOverrides.OnLoad();
			UIHierarchyManager.Instance?.MainFullscreenView.LoadIfNecessary();
			processedText = CreateNewProcessedText();
			AdjustSizesIfNecessary();
			ResolveMaterial();
		}

		protected override void Unload()
		{
			processedText.Dispose();
			processedText = null;
			resolvedMaterial = null;
			materialPropertyOverrides.OnUnload();
			base.Unload();
		}

		protected override void OnValidate()
		{
			base.OnValidate();
			if (base.Loaded)
			{
				materialPropertyOverrides.OnValidate();
				if (fontFamily == null)
				{
					Diagnostics.LogWarning("Trying to assign a null font family to '{0}'.", this);
				}
				if (processedText == null)
				{
					processedText = CreateNewProcessedText();
				}
				else
				{
					processedText.UnscaledLocalRect = GetLocalRectWithMargins();
					processedText.RawText = LocalizeIfNecessary(text);
					processedText.FontFamily = fontFamily;
					processedText.DefaultFontFace = fontFace;
					processedText.UnscaledFontSize = fontSize;
					processedText.DefaultColor = color;
					processedText.WantedRenderingMode = renderingMode;
					processedText.Alignment = alignment;
					processedText.UseAscent = useAscent;
					processedText.Justify = justify;
					processedText.ForceCaps = forceCaps;
					processedText.AutoAdjustUnscaledFontSizeMin = autoAdjustFontSizeMin;
					processedText.AutoTruncate = autoTruncate;
					processedText.WordWrap = wordWrap;
					processedText.InterLetterAdditionalSpacing = interLetterAdditionalSpacing;
					processedText.InterLineAdditionalSpacing = interLineAdditionalSpacing;
					processedText.InterParagraphAdditionalSpacing = interParagraphAdditionalSpacing;
					processedText.MaximumLines = maximumLines;
					processedText.DefaultTextStyle = textStyle;
					processedText.InterpreteRichText = interpreteRichText;
					processedText.InterpreteSymbols = interpreteSymbols;
				}
				AdjustSizesIfNecessary();
				ResolveMaterial();
			}
		}

		[MutatorsProvider]
		private static void LoadStyles(MutatorsReceiver receiver)
		{
			receiver.Add(Mutators.FontFamily);
			receiver.Add(Mutators.FontFace);
			receiver.Add(Mutators.FontSize);
			receiver.Add(Mutators.Color);
			receiver.Add(Mutators.RenderingMode);
			receiver.Add(Mutators.HorizontalAlignment);
			receiver.Add(Mutators.VerticalAlignment);
			receiver.Add(Mutators.UseAscent);
			receiver.Add(Mutators.Justify);
			receiver.Add(Mutators.ForceCaps);
			receiver.Add(Mutators.AutoAdjustFontSizeMin);
			receiver.Add(Mutators.AutoTruncate);
			receiver.Add(Mutators.WordWrap);
			receiver.Add(Mutators.AdjustWidth);
			receiver.Add(Mutators.AdjustWidthMin);
			receiver.Add(Mutators.AdjustWidthMax);
			receiver.Add(Mutators.AdjustHeight);
			receiver.Add(Mutators.InterLetterAdditionalSpacing);
			receiver.Add(Mutators.InterLineAdditionalSpacing);
			receiver.Add(Mutators.InterParagraphAdditionalSpacing);
			receiver.Add(Mutators.TextStyle);
			receiver.Add(Mutators.InterpreteRichText);
			receiver.Add(Mutators.InterpreteSymbols);
			receiver.Add(Mutators.LeftMargin);
			receiver.Add(Mutators.RightMargin);
			receiver.Add(Mutators.TopMargin);
			receiver.Add(Mutators.BottomMargin);
		}

		private string LocalizeIfNecessary(string key)
		{
			if (string.IsNullOrEmpty(key) || key[0] != '%')
			{
				return key;
			}
			ILocalizationService localizationService = UIServiceAccessManager.LocalizationService;
			if (localizationService == null)
			{
				return key;
			}
			return localizationService.Localize(key, key, null, noWarning: false);
		}

		private ProcessedText CreateNewProcessedText()
		{
			return new ProcessedText(UIHierarchyManager.Instance?.MainFullscreenView, UITransform.LocalToGlobalMatrix, GetLocalRectWithMargins(), LocalizeIfNecessary(text), fontFamily, fontFace, fontSize, color, renderingMode, alignment, useAscent, justify, forceCaps, autoAdjustFontSizeMin, autoTruncate, wordWrap, interLetterAdditionalSpacing, interLineAdditionalSpacing, interParagraphAdditionalSpacing, maximumLines, textStyle, interpreteRichText, interpreteSymbols, this);
		}

		private void AdjustSizesIfNecessary()
		{
			if (UIHierarchyManager.Instance == null || UIHierarchyManager.Instance.MainFullscreenView == null || !base.VisibleGlobally || (!autoAdjustWidth && !autoAdjustHeight && autoAdjustFontSizeMin == 0))
			{
				return;
			}
			nestedCallsToAdjustSizes++;
			if (nestedCallsToAdjustSizes <= 3)
			{
				processedText.UnscaledLocalRect = GetLocalRectWithMargins();
				if (autoAdjustWidth)
				{
					float num = processedText.ComputeWidth() + margins.Left + margins.Right;
					if (autoAdjustWidthMax > 0f && num > autoAdjustWidthMax)
					{
						num = autoAdjustWidthMax;
					}
					else if (autoAdjustWidthMin > 0f && num < autoAdjustWidthMin)
					{
						num = autoAdjustWidthMin;
					}
					if (Mathf.Abs(UITransform.Width - num) > 0.01f)
					{
						UITransform.Width = num;
					}
				}
				if (autoAdjustHeight)
				{
					float num2 = processedText.ComputeHeight() + margins.Top + margins.Bottom;
					if (Mathf.Abs(UITransform.Height - num2) > 0.01f)
					{
						UITransform.Height = num2;
					}
				}
				processedText.UnscaledLocalRect = GetLocalRectWithMargins(useAutoAdjustWidthMax: false);
				processedText.OnPositioningContextChanged();
			}
			else
			{
				Diagnostics.LogWarning("Detected infinite loop while calling AdjustSizesIfNecessary on '{0}'", this);
			}
			nestedCallsToAdjustSizes--;
		}

		private Rect GetLocalRectWithMargins(bool useAutoAdjustWidthMax = true)
		{
			Rect localRect = UITransform.LocalRect;
			if (autoAdjustWidth && autoAdjustWidthMax > 0f && useAutoAdjustWidthMax)
			{
				localRect.width = autoAdjustWidthMax;
			}
			return new Rect(localRect.x + margins.Left, localRect.y + margins.Top, localRect.width - margins.Left - margins.Right, localRect.height - margins.Top - margins.Bottom);
		}

		private void SetHorizontalAlignment(HorizontalAlignment horizontalAlignment)
		{
			Alignment alignment = Alignment;
			alignment.Horizontal = horizontalAlignment;
			Alignment = alignment;
		}

		private void SetVerticalAlignment(VerticalAlignment verticalAlignment)
		{
			Alignment alignment = Alignment;
			alignment.Vertical = verticalAlignment;
			Alignment = alignment;
		}

		private void OnMaterialChanged()
		{
			ResolveMaterial();
		}

		private void ResolveMaterial()
		{
			StaticString variant = ((processedText.UsedRenderingMode == FontRenderingMode.DistanceField) ? UIPrimitiveDrawer.DistanceFieldVariantName : StaticString.Empty);
			resolvedMaterial = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Glyph, blendType, material.Id, variant, out var materialPropertiesInfo);
			if (resolvedMaterial == null)
			{
				resolvedMaterial = UIMaterialCollection.ResolveMaterial(UIPrimitiveType.Glyph, blendType, UIMaterialCollection.DefaultMaterialNameId, variant, out materialPropertiesInfo);
			}
			processedText.MaterialPropertyOverridesAtomId = materialPropertyOverrides.GetAtomId(materialPropertiesInfo);
		}
	}
}
