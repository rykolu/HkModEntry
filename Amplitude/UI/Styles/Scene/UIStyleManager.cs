using System;
using System.Collections.Generic;
using System.Reflection;
using Amplitude.UI.Interactables;
using Amplitude.UI.Renderers;
using Amplitude.UI.Styles.Data;
using UnityEngine;

namespace Amplitude.UI.Styles.Scene
{
	[RequireComponent(typeof(UIHierarchyManager))]
	[ExecuteInEditMode]
	public class UIStyleManager : UIBehaviour
	{
		private struct MaterialPropertyOverridesItemFactory
		{
			public static readonly Type[] ItemsCtorParamsTypes = new Type[1] { typeof(StaticString) };

			public static readonly object[] ItemsCtorParams = new object[1];

			public ConstructorInfo[] ItemCtorPerPropertyType;

			public void RegisterMaterialPropertyOverridesItemCtor(Type type)
			{
				if (ItemCtorPerPropertyType == null)
				{
					InstantiateItemCtorPerPropertyType();
				}
				type.GetConstructor(ItemsCtorParamsTypes);
				ConstructorInfo constructor = type.GetConstructor(ItemsCtorParamsTypes);
				if (!(constructor == null))
				{
					object[] customAttributes = type.GetCustomAttributes(UIRuntimeStyleItemMaterialPropertyOverridesAttributeType, inherit: false);
					int num = ((customAttributes != null) ? customAttributes.Length : 0);
					for (int i = 0; i < num; i++)
					{
						UIMaterialPropertyOverride.PropertyType propertyType = ((UIRuntimeStyleItemMaterialPropertyOverridesAttribute)customAttributes[i]).PropertyType;
						ItemCtorPerPropertyType[(int)propertyType] = constructor;
					}
				}
			}

			public void ClearMaterialPropertyOverridesItemCtors()
			{
				ItemCtorPerPropertyType = null;
			}

			public bool TryCreateItem(UIMaterialPropertyOverride.PropertyType propertyType, StaticString identifier, out UIRuntimeStyleItem item)
			{
				ConstructorInfo constructorInfo = ItemCtorPerPropertyType[(int)propertyType];
				if (constructorInfo == null)
				{
					item = null;
					return false;
				}
				ItemsCtorParams[0] = identifier;
				item = (UIRuntimeStyleItem)constructorInfo.Invoke(ItemsCtorParams);
				return item != null;
			}

			private void InstantiateItemCtorPerPropertyType()
			{
				UIMaterialPropertyOverride.PropertyType[] array = (UIMaterialPropertyOverride.PropertyType[])Enum.GetValues(typeof(UIMaterialPropertyOverride.PropertyType));
				int num = ((array != null) ? array.Length : 0);
				int num2 = -1;
				for (int i = 0; i < num; i++)
				{
					num2 = Mathf.Max((int)array[i]);
				}
				ItemCtorPerPropertyType = new ConstructorInfo[num2 + 1];
			}
		}

		private struct ItemsSet
		{
			public readonly Type TargetType;

			public PerformanceList<UIRuntimeStyleItem> Items;

			public int ParentIndex;

			public ItemsSet(Type targetType)
			{
				TargetType = targetType;
				Items = default(PerformanceList<UIRuntimeStyleItem>);
				ParentIndex = -1;
			}

			public int IndexOf(StaticString itemIdentifier)
			{
				int count = Items.Count;
				for (int i = 0; i < count; i++)
				{
					if (Items.Data[i].Identifier == itemIdentifier)
					{
						return i;
					}
				}
				return -1;
			}
		}

		private static readonly Type UIRuntimeStyleItemMaterialPropertyOverridesAttributeType = typeof(UIRuntimeStyleItemMaterialPropertyOverridesAttribute);

		private MaterialPropertyOverridesItemFactory materialPropertyOverridesItemsFactory;

		private int materialPropertyOverridesItemsSetIndex = -1;

		[SerializeField]
		[HideInInspector]
		private UIStylesheet[] stylesheets;

		private static readonly Type MutatorsProviderAttributeType = typeof(MutatorsProviderAttribute);

		private static readonly Type MutatorsReceiverType = typeof(MutatorsReceiver);

		private static MutatorsReceiver mutatorsReceiver = new MutatorsReceiver();

		private static object[] mutatorsProviderParams = new object[1] { mutatorsReceiver };

		private PerformanceList<ItemsSet> itemsSets;

		private static readonly Type UIComponentType = typeof(UIComponent);

		private static readonly Type IUIStyleTargetType = typeof(IUIStyleTarget);

		private static readonly Type UIRuntimeStyleItemType = typeof(UIRuntimeStyleItem);

		private static UIStyleManager instance = null;

		private static PerformanceList<UIRuntimeStyleItem.LocalState> localStatesCache = new PerformanceList<UIRuntimeStyleItem.LocalState>(16);

		private Dictionary<StaticString, UIStyle> stylesByName = new Dictionary<StaticString, UIStyle>();

		private List<IUIStyleTarget> animatedTargets = new List<IUIStyleTarget>();

		public IEnumerable<UIStyle> AllStyles
		{
			get
			{
				if (stylesByName == null)
				{
					yield break;
				}
				foreach (KeyValuePair<StaticString, UIStyle> item in stylesByName)
				{
					yield return item.Value;
				}
			}
		}

		internal static UIStyleManager Instance => instance;

		public void LoadMaterialPropertyOverrides(ref UIMaterialPropertyOverrides materialPropertyOverrides)
		{
			if (materialPropertyOverridesItemsSetIndex < 0)
			{
				materialPropertyOverridesItemsSetIndex = itemsSets.Count;
				itemsSets.Add(new ItemsSet(typeof(UIMaterialPropertyOverrides)));
			}
			ref ItemsSet reference = ref itemsSets.Data[materialPropertyOverridesItemsSetIndex];
			int num = materialPropertyOverrides.Items.Length;
			for (int i = 0; i < num; i++)
			{
				UIMaterialPropertyOverride uIMaterialPropertyOverride = materialPropertyOverrides.Items[i];
				StaticString staticString = new StaticString(uIMaterialPropertyOverride.Name);
				if (reference.IndexOf(staticString) < 0)
				{
					UIRuntimeStyleItem item = null;
					if (!materialPropertyOverridesItemsFactory.TryCreateItem(uIMaterialPropertyOverride.Type, staticString, out item))
					{
						Diagnostics.LogWarning(55uL, $"No UIRuntimeStyleItem could not be created for '{staticString}' ('PropertyType.{uIMaterialPropertyOverride.Type}') .");
					}
					else
					{
						reference.Items.Add(item);
					}
				}
			}
		}

		internal float ComputeApplicationRating(Type target, UIStyle style)
		{
			int itemsLength = style.ItemsLength;
			if (itemsLength == 0)
			{
				return 0f;
			}
			int num = 0;
			int num2 = IndexOfItemsSet(target);
			while (num2 >= 0)
			{
				ref ItemsSet reference = ref itemsSets.Data[num2];
				for (int i = 0; i < itemsLength; i++)
				{
					UIStyleItem item = style.GetItem(i);
					if (item == null)
					{
						Diagnostics.LogWarning(55uL, "Style '" + style.name + "' has null item");
						continue;
					}
					int num3 = reference.IndexOf(item.GetIdentifier());
					if (num3 >= 0 && reference.Items.Data[num3].IsDataValid(item))
					{
						num++;
						if (num >= itemsLength)
						{
							return 1f;
						}
					}
				}
				num2 = reference.ParentIndex;
			}
			return (float)num / (float)itemsLength;
		}

		internal void FindAllRuntimeStyleItems(IUIStyleTarget target, List<UIRuntimeStyleItem> result)
		{
			int num = IndexOfItemsSet(target.GetType());
			while (num >= 0)
			{
				ref ItemsSet reference = ref itemsSets.Data[num];
				for (int i = 0; i < reference.Items.Count; i++)
				{
					result.Add(reference.Items.Data[i]);
				}
				num = reference.ParentIndex;
			}
			IUIMaterialPropertyOverridesProvider iUIMaterialPropertyOverridesProvider;
			if (materialPropertyOverridesItemsSetIndex < 0 || (iUIMaterialPropertyOverridesProvider = target as IUIMaterialPropertyOverridesProvider) == null)
			{
				return;
			}
			ref UIMaterialPropertyOverrides materialPropertyOverrides = ref iUIMaterialPropertyOverridesProvider.MaterialPropertyOverrides;
			if (materialPropertyOverrides.Empty)
			{
				return;
			}
			ref ItemsSet reference2 = ref itemsSets.Data[materialPropertyOverridesItemsSetIndex];
			for (int j = 0; j < materialPropertyOverrides.Items.Length; j++)
			{
				int num2 = reference2.IndexOf(new StaticString(materialPropertyOverrides.Items[j].Name));
				if (num2 >= 0)
				{
					result.Add(reference2.Items.Data[num2]);
				}
			}
		}

		internal void ReloadStylesheets()
		{
			UnloadStylesheets();
			LoadStylesheets();
		}

		internal void OnStylesheetAdded(UIStylesheet stylesheet)
		{
			ReloadStylesheets();
			if (stylesheet == null)
			{
				return;
			}
			UIStyle[] styles = stylesheet.Styles;
			int num = ((styles != null) ? styles.Length : 0);
			for (int i = 0; i < num; i++)
			{
				UIStyle uIStyle = stylesheet.Styles[i];
				if (uIStyle != null)
				{
					TriggerStyleCreatedEvent(uIStyle);
				}
			}
		}

		internal void OnStylesheetRemoved(UIStylesheet stylesheet)
		{
			ReloadStylesheets();
			UIStyle[] styles = stylesheet.Styles;
			int num = ((styles != null) ? styles.Length : 0);
			for (int i = 0; i < num; i++)
			{
				UIStyle uIStyle = stylesheet.Styles[i];
				if (uIStyle != null)
				{
					TriggerStyleDeletedEvent(uIStyle);
				}
			}
		}

		internal void OnStylesheetStyleReplaced(UIStylesheet stylesheet, UIStyle oldStyle, UIStyle newStyle)
		{
			if (stylesheets != null && Array.FindIndex(stylesheets, (UIStylesheet s) => s == stylesheet) >= 0)
			{
				ReloadStylesheets();
				if (oldStyle != null && !stylesByName.ContainsValue(oldStyle))
				{
					TriggerStyleDeletedEvent(oldStyle);
				}
				if (newStyle != null && stylesByName.ContainsValue(newStyle))
				{
					TriggerStyleCreatedEvent(newStyle);
				}
			}
		}

		private void LoadStylesheets()
		{
			stylesByName.Clear();
			UIStylesheet[] array = stylesheets;
			int num = ((array != null) ? array.Length : 0);
			for (int i = 0; i < num; i++)
			{
				UIStylesheet uIStylesheet = stylesheets[i];
				if (!(uIStylesheet != null))
				{
					continue;
				}
				uIStylesheet.Load();
				UIStyle[] styles = uIStylesheet.Styles;
				int num2 = ((styles != null) ? styles.Length : 0);
				for (int j = 0; j < num2; j++)
				{
					UIStyle uIStyle = uIStylesheet.Styles[j];
					if (uIStyle != null)
					{
						stylesByName[uIStyle.Name] = uIStyle;
					}
				}
			}
		}

		private void UnloadStylesheets()
		{
			stylesByName.Clear();
		}

		private void LoadRuntimeItems()
		{
			int count = itemsSets.Count;
			for (int i = 0; i < count; i++)
			{
				ref ItemsSet reference = ref itemsSets.Data[i];
				InstantiateRuntimeItems(reference.TargetType, ref reference.Items);
				InitializeParentChain(i);
			}
		}

		private void UnloadItems()
		{
			materialPropertyOverridesItemsSetIndex = -1;
			itemsSets.ClearArray();
		}

		private int IndexOfItemsSet(Type type)
		{
			int count = itemsSets.Count;
			for (int i = 0; i < count; i++)
			{
				if (itemsSets.Data[i].TargetType == type)
				{
					return i;
				}
			}
			return -1;
		}

		private void InitializeParentChain(int startIndex)
		{
			ref ItemsSet reference = ref itemsSets.Data[startIndex];
			if (reference.ParentIndex < 0)
			{
				Type baseType = reference.TargetType.BaseType;
				int num = IndexOfItemsSet(baseType);
				if (num >= 0)
				{
					reference.ParentIndex = num;
					InitializeParentChain(num);
				}
			}
		}

		private void InstantiateRuntimeItems(Type targetType, ref PerformanceList<UIRuntimeStyleItem> items)
		{
			MethodInfo[] methods = targetType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			int num = ((methods != null) ? methods.Length : 0);
			for (int i = 0; i < num; i++)
			{
				MethodInfo methodInfo = methods[i];
				object[] customAttributes = methodInfo.GetCustomAttributes(MutatorsProviderAttributeType, inherit: false);
				if (customAttributes == null || customAttributes.Length == 0)
				{
					continue;
				}
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters == null || parameters.Length != 1)
				{
					Diagnostics.LogError(55uL, $"[{MutatorsProviderAttributeType.Name}] '{targetType.FullName}.{methodInfo.Name}' has '{((parameters != null) ? parameters.Length : 0)}' parameters. Expected 1.");
					continue;
				}
				if (parameters[0].ParameterType != MutatorsReceiverType)
				{
					Diagnostics.LogError(55uL, $"[{MutatorsProviderAttributeType.Name}] '{targetType.FullName}.{methodInfo.Name}''s parameter[0] is invalid. Expected {MutatorsReceiverType}.");
					continue;
				}
				methodInfo.Invoke(null, mutatorsProviderParams);
				mutatorsReceiver.Flush(ref items);
				break;
			}
		}

		protected UIStyleManager()
		{
			instance = this;
		}

		public bool Contains(StaticString name)
		{
			LoadIfNecessary();
			return stylesByName.ContainsKey(name);
		}

		public bool TryFindStyle(StaticString name, out UIStyle style)
		{
			LoadIfNecessary();
			return stylesByName.TryGetValue(name, out style);
		}

		internal void FillStyleItemsList(IUIStyleTarget target, string[] styleNames, ref PerformanceList<UIRuntimeStyleItem.LocalState> localStates, out bool hasAnyReactivity)
		{
			hasAnyReactivity = false;
			int initItemsSetIndex = IndexOfItemsSet(target.GetType());
			for (int num = styleNames.Length - 1; num >= 0; num--)
			{
				if (!string.IsNullOrEmpty(styleNames[num]))
				{
					StaticString staticString = new StaticString(styleNames[num]);
					UIStyle style = null;
					if (TryFindStyle(staticString, out style))
					{
						int itemsLength = style.ItemsLength;
						for (int i = 0; i < itemsLength; i++)
						{
							UIStyleItem item = style.GetItem(i);
							if (!(item == null) && !LocalStatesCacheContains(item.GetIdentifier()))
							{
								int resultItemSetIndex = -1;
								int resultRuntimeItemIndex = -1;
								if (TryFindRuntimeItemIndices(target, initItemsSetIndex, item, out resultItemSetIndex, out resultRuntimeItemIndex))
								{
									localStatesCache.Add(new UIRuntimeStyleItem.LocalState(item, resultItemSetIndex, resultRuntimeItemIndex));
									hasAnyReactivity |= item.ReactivityKeysCount > 0;
								}
							}
						}
					}
				}
			}
			int count = localStatesCache.Count;
			if (count > 0)
			{
				localStates.Reserve(count);
				Array.Copy(localStatesCache.Data, localStates.Data, count);
				localStates.Count = count;
				localStatesCache.ClearArray();
			}
		}

		internal void Apply(IUIStyleTarget target, ref UIReactivityState reactivityState, bool instant)
		{
			bool flag = false;
			bool flag2 = false;
			IndexOfItemsSet(target.GetType());
			ref UIStyleController styleController = ref target.StyleController;
			int localStatesCount = styleController.LocalStatesCount;
			for (int i = 0; i < localStatesCount; i++)
			{
				ref UIRuntimeStyleItem.LocalState localState = ref styleController.GetLocalState(i);
				UIRuntimeStyleItem obj = itemsSets.Data[localState.ItemsSetIndex].Items.Data[localState.RuntimeItemIndex];
				flag |= localState.IsAnimationInProgress;
				obj.ApplyReactivityState(target, ref localState, ref reactivityState, instant);
				flag2 |= localState.IsAnimationInProgress;
			}
			if (flag != flag2)
			{
				if (flag2)
				{
					animatedTargets.Add(target);
				}
				else
				{
					animatedTargets.Remove(target);
				}
			}
		}

		internal void SpecificUpdate()
		{
			float deltaTime = Time.deltaTime;
			for (int num = animatedTargets.Count - 1; num >= 0; num--)
			{
				if (!UpdateAnimations(animatedTargets[num], deltaTime))
				{
					animatedTargets.RemoveAt(num);
				}
			}
		}

		protected override void Load()
		{
			base.Load();
			SearchForTypes();
			LoadRuntimeItems();
			LoadStylesheets();
		}

		protected override void Unload()
		{
			UnloadStylesheets();
			UnloadItems();
			materialPropertyOverridesItemsFactory.ClearMaterialPropertyOverridesItemCtors();
			base.Unload();
		}

		protected override void Destruct()
		{
			instance = null;
			base.Destruct();
		}

		private static bool LocalStatesCacheContains(StaticString identifier)
		{
			for (int i = 0; i < localStatesCache.Count; i++)
			{
				if (localStatesCache.Data[i].StyleItem.GetIdentifier() == identifier)
				{
					return true;
				}
			}
			return false;
		}

		private void SearchForTypes()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			int num = ((assemblies != null) ? assemblies.Length : 0);
			for (int i = 0; i < num; i++)
			{
				Type[] types = assemblies[i].GetTypes();
				int num2 = ((types != null) ? types.Length : 0);
				for (int j = 0; j < num2; j++)
				{
					Type type = types[j];
					if (type.IsClass)
					{
						if (IUIStyleTargetType.IsAssignableFrom(type))
						{
							itemsSets.Add(new ItemsSet(type));
						}
						else if (UIRuntimeStyleItemType.IsAssignableFrom(type))
						{
							materialPropertyOverridesItemsFactory.RegisterMaterialPropertyOverridesItemCtor(type);
						}
					}
				}
			}
		}

		private bool UpdateAnimations(IUIStyleTarget target, float deltaTime)
		{
			bool flag = false;
			IndexOfItemsSet(target.GetType());
			ref UIStyleController styleController = ref target.StyleController;
			int localStatesCount = styleController.LocalStatesCount;
			for (int i = 0; i < localStatesCount; i++)
			{
				ref UIRuntimeStyleItem.LocalState localState = ref styleController.GetLocalState(i);
				if (localState.IsAnimationInProgress)
				{
					itemsSets.Data[localState.ItemsSetIndex].Items.Data[localState.RuntimeItemIndex].UpdateAnimation(target, ref localState, deltaTime);
					flag |= localState.IsAnimationInProgress;
				}
			}
			return flag;
		}

		private bool TryFindRuntimeItemIndices(IUIStyleTarget target, int initItemsSetIndex, UIStyleItem dataItem, out int resultItemSetIndex, out int resultRuntimeItemIndex)
		{
			resultItemSetIndex = -1;
			resultRuntimeItemIndex = -1;
			StaticString identifier = dataItem.GetIdentifier();
			int num = initItemsSetIndex;
			while (num >= 0)
			{
				ref ItemsSet reference = ref itemsSets.Data[num];
				int num2 = reference.IndexOf(identifier);
				if (num2 >= 0)
				{
					if (reference.Items.Data[num2].IsDataValid(dataItem))
					{
						resultItemSetIndex = num;
						resultRuntimeItemIndex = num2;
						return true;
					}
				}
				else
				{
					num = reference.ParentIndex;
				}
			}
			return false;
		}

		internal static void TriggerStyleCreatedEvent(UIStyle style)
		{
			UIStylesEventArg args = new UIStyleCreatedEventArgs(style);
			Instance?.TriggerEvent(args);
		}

		internal static void TriggerStyleDeletedEvent(UIStyle style)
		{
			UIStylesEventArg args = new UIStyleDeletedEventArgs(style);
			Instance?.TriggerEvent(args);
		}

		internal static void TriggerItemCreatedEvent(UIStyle style, UIStyleItem newItem)
		{
			UIStylesEventArg args = new UIStylesItemCreatedEventArgs(style, newItem);
			Instance?.TriggerEvent(args);
		}

		internal static void TriggerItemDeletedEvent(UIStyle style, StaticString itemIdentifier)
		{
			UIStylesEventArg args = new UIStylesItemDeletedEventArgs(style, itemIdentifier);
			Instance?.TriggerEvent(args);
		}

		internal static void TriggerItemChangedEvent(UIStyleItem item)
		{
			UIStylesEventArg args = new UIStylesItemChangedEventArgs(item);
			Instance?.TriggerEvent(args);
		}

		internal void TriggerEvent(UIStylesEventArg args)
		{
			if (args.EventType == UIStylesEventArg.Type.StyleCreated || args.EventType == UIStylesEventArg.Type.StyleDeleted)
			{
				ReloadStylesheets();
			}
			for (int i = 0; i < UITransform.Roots.Count; i++)
			{
				UITransform uITransform = UITransform.Roots.Data[i];
				TriggerEventRecursive(args, uITransform);
			}
		}

		private void TriggerEventRecursive(UIStylesEventArg arg, UITransform transform)
		{
			IUIStyleTarget[] components = transform.GetComponents<IUIStyleTarget>();
			int num = components.Length;
			for (int i = 0; i < num; i++)
			{
				components[i].StyleController.OnStyleEvent(arg);
			}
			int count = transform.Children.Count;
			for (int j = 0; j < count; j++)
			{
				TriggerEventRecursive(arg, transform.Children.Data[j]);
			}
		}
	}
}
