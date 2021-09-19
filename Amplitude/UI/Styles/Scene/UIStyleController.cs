using System;
using Amplitude.UI.Interactables;
using Amplitude.UI.Renderers;
using Amplitude.UI.Styles.Data;
using UnityEngine;

namespace Amplitude.UI.Styles.Scene
{
	[Serializable]
	public struct UIStyleController
	{
		private static UIReactivityState workingReactivityState = UIReactivityState.Normal;

		[SerializeField]
		private string[] styleNames;

		[NonSerialized]
		private bool isLoaded;

		[NonSerialized]
		private IUIStyleTarget target;

		[NonSerialized]
		private PerformanceList<UIRuntimeStyleItem.LocalState> localStates;

		[NonSerialized]
		private bool hasAnyReactivity;

		[NonSerialized]
		private UIReactivityState lastReactivityState;

		[NonSerialized]
		private UIReactivityState additionalTags;

		public string[] StyleNames => styleNames;

		public UIReactivityState AdditionalTags => additionalTags;

		internal IUIStyleTarget Target => target;

		internal int LocalStatesCount => localStates.Count;

		internal UIReactivityState LastReactivityState => lastReactivityState;

		public void Bind(IUIStyleTarget target)
		{
			this.target = target;
			if (styleNames == null || styleNames.Length == 0)
			{
				isLoaded = true;
				return;
			}
			UIStyleManager instance = UIStyleManager.Instance;
			instance.LoadIfNecessary();
			IUIMaterialPropertyOverridesProvider iUIMaterialPropertyOverridesProvider;
			if ((iUIMaterialPropertyOverridesProvider = this.target as IUIMaterialPropertyOverridesProvider) != null)
			{
				ref UIMaterialPropertyOverrides materialPropertyOverrides = ref iUIMaterialPropertyOverridesProvider.MaterialPropertyOverrides;
				if (!materialPropertyOverrides.Empty)
				{
					instance.LoadMaterialPropertyOverrides(ref materialPropertyOverrides);
				}
			}
			FillStyleItemsList();
			isLoaded = true;
			lastReactivityState.Clear();
			UIControl control = null;
			if (hasAnyReactivity && TryFindClosestControl(out control))
			{
				control.LoadIfNecessary();
				lastReactivityState.AddTagsFrom(ref control.ReactivityState);
			}
			ApplyReactivityState(instant: true);
		}

		public void Unbind()
		{
			isLoaded = false;
			lastReactivityState.Clear();
			localStates.ClearArray();
			target = null;
		}

		public bool ContainsStyle(string styleName)
		{
			return Array.IndexOf(styleNames, styleName) >= 0;
		}

		public bool ContainsValue(StaticString id)
		{
			return IndexOfDataItem(id) >= 0;
		}

		public void UpdateReactivity(ref UIReactivityState reactivityState, bool instant)
		{
			if (isLoaded && hasAnyReactivity && (instant || !reactivityState.IsSameAs(ref lastReactivityState)))
			{
				lastReactivityState.Clear();
				lastReactivityState.AddTagsFrom(ref reactivityState);
				ApplyReactivityState(instant);
			}
		}

		public void SetStyleNames(string[] newStyleNames)
		{
			if (isLoaded)
			{
				styleNames = newStyleNames;
				OnLocalChange();
			}
		}

		public void SetAdditionalTags(ref UIReactivityState additionalTags, bool instant = false)
		{
			if (!this.additionalTags.IsSameAs(ref additionalTags))
			{
				this.additionalTags.Clear();
				this.additionalTags.AddTagsFrom(ref additionalTags);
				ApplyReactivityState(instant);
			}
		}

		public void SetAdditionalTag(StaticString tag, bool instant = false)
		{
			if (!additionalTags.IsSameAs(tag))
			{
				additionalTags.Clear();
				additionalTags.Add(tag);
				ApplyReactivityState(instant);
			}
		}

		public void SetAdditionalTags(StaticString[] additionalTags, bool instant = false)
		{
			if (!this.additionalTags.IsSameAs(additionalTags))
			{
				this.additionalTags.Clear();
				this.additionalTags.Add(additionalTags);
				ApplyReactivityState(instant);
			}
		}

		public void ClearAdditionalTags(bool instant = false)
		{
			if (!additionalTags.IsNormal)
			{
				additionalTags.Clear();
				ApplyReactivityState(instant);
			}
		}

		internal ref UIRuntimeStyleItem.LocalState GetLocalState(int index)
		{
			return ref localStates.Data[index];
		}

		internal void OnNameReorder()
		{
			OnLocalChange();
		}

		internal void OnNameAdded()
		{
			OnLocalChange();
		}

		internal void OnNameRemoved()
		{
			OnLocalChange();
		}

		internal void OnNameChanged()
		{
			OnLocalChange();
		}

		internal void OnStyleEvent(UIStylesEventArg rawArgs)
		{
			if (!isLoaded)
			{
				return;
			}
			switch (rawArgs.EventType)
			{
			case UIStylesEventArg.Type.ItemChanged:
			{
				UIStylesItemChangedEventArgs uIStylesItemChangedEventArgs = (UIStylesItemChangedEventArgs)rawArgs;
				if (IndexOfDataItem(uIStylesItemChangedEventArgs.Item) >= 0)
				{
					ApplyReactivityState(instant: true);
				}
				break;
			}
			case UIStylesEventArg.Type.ItemCreated:
			case UIStylesEventArg.Type.ItemDeleted:
			{
				UIStyle uIStyle2 = null;
				UIStylesItemCreatedEventArgs uIStylesItemCreatedEventArgs;
				UIStylesItemDeletedEventArgs uIStylesItemDeletedEventArgs;
				if ((uIStylesItemCreatedEventArgs = rawArgs as UIStylesItemCreatedEventArgs) != null)
				{
					uIStyle2 = uIStylesItemCreatedEventArgs.Style;
				}
				else if ((uIStylesItemDeletedEventArgs = rawArgs as UIStylesItemDeletedEventArgs) != null)
				{
					uIStyle2 = uIStylesItemDeletedEventArgs.Style;
				}
				if (uIStyle2 != null && IndexOfStyle(uIStyle2.Name) >= 0)
				{
					FillStyleItemsList();
					ApplyReactivityState(instant: true);
				}
				break;
			}
			case UIStylesEventArg.Type.StyleCreated:
			case UIStylesEventArg.Type.StyleDeleted:
			{
				UIStyle uIStyle = null;
				UIStyleCreatedEventArgs uIStyleCreatedEventArgs;
				UIStyleDeletedEventArgs uIStyleDeletedEventArgs;
				if ((uIStyleCreatedEventArgs = rawArgs as UIStyleCreatedEventArgs) != null)
				{
					uIStyle = uIStyleCreatedEventArgs.Style;
				}
				else if ((uIStyleDeletedEventArgs = rawArgs as UIStyleDeletedEventArgs) != null)
				{
					uIStyle = uIStyleDeletedEventArgs.Style;
				}
				if (uIStyle != null && IndexOfStyle(uIStyle.Name) >= 0)
				{
					FillStyleItemsList();
					ApplyReactivityState(instant: true);
				}
				break;
			}
			}
		}

		internal UIStyleItem FindItem(StaticString identifier)
		{
			int count = localStates.Count;
			for (int i = 0; i < count; i++)
			{
				UIStyleItem styleItem = localStates.Data[i].StyleItem;
				if (styleItem.GetIdentifier() == identifier)
				{
					return styleItem;
				}
			}
			return null;
		}

		private bool TryFindClosestControl(out UIControl control)
		{
			control = null;
			UIComponent uIComponent;
			if ((object)(uIComponent = target as UIComponent) != null)
			{
				UITransform uITransform = uIComponent.UITransform;
				while (uITransform != null && control == null)
				{
					if (uIComponent != uITransform)
					{
						uITransform.LoadIfNecessary();
					}
					control = uITransform.FindLastSibling<UIControl>();
					uITransform = uITransform.Parent;
				}
			}
			return control != null;
		}

		private void ApplyReactivityState(bool instant)
		{
			workingReactivityState.AddTagsFrom(ref lastReactivityState);
			workingReactivityState.AddTagsFrom(ref additionalTags);
			UIStyleManager.Instance.Apply(target, ref workingReactivityState, instant);
			workingReactivityState.Clear();
		}

		private void FillStyleItemsList()
		{
			localStates.ClearArray();
			if (styleNames != null && styleNames.Length != 0)
			{
				UIStyleManager.Instance.FillStyleItemsList(target, styleNames, ref localStates, out hasAnyReactivity);
			}
		}

		private int IndexOfStyle(StaticString name)
		{
			string[] array = styleNames;
			int num = ((array != null) ? array.Length : 0);
			for (int i = 0; i < num; i++)
			{
				if (styleNames[i] == name.ToString())
				{
					return i;
				}
			}
			return -1;
		}

		private int IndexOfDataItem(StaticString identifier)
		{
			int count = localStates.Count;
			for (int i = 0; i < count; i++)
			{
				if (localStates.Data[i].StyleItem.GetIdentifier() == identifier)
				{
					return i;
				}
			}
			return -1;
		}

		private int IndexOfDataItem(UIStyleItem item)
		{
			int count = localStates.Count;
			for (int i = 0; i < count; i++)
			{
				if (localStates.Data[i].StyleItem == item)
				{
					return i;
				}
			}
			return -1;
		}

		private void OnLocalChange()
		{
			if (isLoaded)
			{
				FillStyleItemsList();
				ApplyReactivityState(instant: true);
			}
		}
	}
}
