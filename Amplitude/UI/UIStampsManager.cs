using System;
using System.Collections.Generic;
using Amplitude.Framework;
using UnityEngine;

namespace Amplitude.UI
{
	public class UIStampsManager : UIBehaviour, IUIStampsService, IService
	{
		private struct StampData
		{
			public UIComponent Target;

			public int KeyGuid;

			public StaticString[] Tags;
		}

		public static readonly int InvalidRegistrationId = 0;

		private static readonly int StampTagsCapacity = 10;

		private static UIStampsManager instance = null;

		[SerializeField]
		private UIStampKeysDefinition stampKeysDefinition;

		private Dictionary<int, StampData> stampDataByTargetInstanceId = new Dictionary<int, StampData>();

		private Dictionary<int, List<int>> registrationIdsByKeyGuid = new Dictionary<int, List<int>>();

		private Dictionary<StaticString, int> guidByKeyDictionary = new Dictionary<StaticString, int>();

		public static UIStampsManager Instance => instance;

		internal UIStampKeysDefinition.UIStampKey[] AllStampKeys
		{
			get
			{
				if (!(stampKeysDefinition != null))
				{
					return null;
				}
				return stampKeysDefinition.StampKeys;
			}
		}

		public IEnumerable<UIComponent> AllStampedComponents
		{
			get
			{
				foreach (KeyValuePair<int, List<int>> kvp in registrationIdsByKeyGuid)
				{
					int i = 0;
					while (i < kvp.Value.Count)
					{
						yield return stampDataByTargetInstanceId[kvp.Value[i]].Target;
						int num = i + 1;
						i = num;
					}
				}
			}
		}

		public event Action<int, bool> StampRegistrationChanged;

		public event Action<int, StaticString, bool> StampTagsChanged;

		public event Action<int> StampTagsCleared;

		protected UIStampsManager()
		{
			instance = this;
		}

		public void FindStamps(StaticString key, ref List<UIComponent> result)
		{
			int value = UIStampKeysDefinition.NullKeyGuid;
			if (guidByKeyDictionary.TryGetValue(key, out value))
			{
				FindStamps(value, ref result);
			}
		}

		public void FindStamps(StaticString key, StaticString tag, ref List<UIComponent> result)
		{
			int value = UIStampKeysDefinition.NullKeyGuid;
			if (guidByKeyDictionary.TryGetValue(key, out value))
			{
				FindStamps(value, tag, ref result);
			}
		}

		public void FindStamps(StaticString key, StaticString[] tags, ref List<UIComponent> result)
		{
			int value = UIStampKeysDefinition.NullKeyGuid;
			if (guidByKeyDictionary.TryGetValue(key, out value))
			{
				FindStamps(value, tags, ref result);
			}
		}

		public void FindStamps(int keyGuid, ref List<UIComponent> result)
		{
			List<int> value = null;
			if (!registrationIdsByKeyGuid.TryGetValue(keyGuid, out value))
			{
				return;
			}
			int count = value.Count;
			for (int i = 0; i < count; i++)
			{
				if (stampDataByTargetInstanceId.TryGetValue(value[i], out var value2) && !result.Contains(value2.Target))
				{
					result.Add(value2.Target);
				}
			}
		}

		public void FindStamps(int keyGuid, StaticString tag, ref List<UIComponent> result)
		{
			if (StaticString.IsNullOrEmpty(tag))
			{
				FindStamps(keyGuid, ref result);
				return;
			}
			List<int> value = null;
			if (!registrationIdsByKeyGuid.TryGetValue(keyGuid, out value))
			{
				return;
			}
			int count = value.Count;
			for (int i = 0; i < count; i++)
			{
				if (!stampDataByTargetInstanceId.TryGetValue(value[i], out var value2))
				{
					continue;
				}
				StaticString[] tags = value2.Tags;
				int num = value2.Tags.Length;
				for (int j = 0; j < num; j++)
				{
					StaticString staticString = tags[j];
					if (StaticString.IsNullOrEmpty(staticString))
					{
						break;
					}
					if (staticString == tag && !result.Contains(value2.Target))
					{
						result.Add(value2.Target);
						break;
					}
				}
			}
		}

		public void FindStamps(int keyGuid, StaticString[] searchedTags, ref List<UIComponent> result)
		{
			if (searchedTags == null || searchedTags.Length == 0)
			{
				FindStamps(keyGuid, ref result);
				return;
			}
			List<int> value = null;
			if (!registrationIdsByKeyGuid.TryGetValue(keyGuid, out value))
			{
				return;
			}
			int num = searchedTags.Length;
			int count = value.Count;
			for (int i = 0; i < count; i++)
			{
				if (!stampDataByTargetInstanceId.TryGetValue(value[i], out var value2))
				{
					continue;
				}
				bool flag = true;
				StaticString[] tags = value2.Tags;
				int num2 = tags.Length;
				for (int j = 0; j < num; j++)
				{
					StaticString staticString = searchedTags[j];
					if (StaticString.IsNullOrEmpty(staticString))
					{
						continue;
					}
					bool flag2 = false;
					for (int k = 0; k < num2; k++)
					{
						StaticString staticString2 = tags[k];
						if (StaticString.IsNullOrEmpty(staticString2))
						{
							break;
						}
						if (staticString == staticString2)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						flag = false;
						break;
					}
				}
				if (flag && !result.Contains(value2.Target))
				{
					result.Add(value2.Target);
				}
			}
		}

		public bool ContainsAnyTags(int registrationId, StaticString[] searchedTags)
		{
			if (!stampDataByTargetInstanceId.TryGetValue(registrationId, out var value))
			{
				Diagnostics.LogError($"RegistrationId '{registrationId}' does not exist.");
				return false;
			}
			StaticString[] tags = value.Tags;
			int num = ((tags != null) ? tags.Length : 0);
			for (int i = 0; i < num; i++)
			{
				int num2 = ((searchedTags != null) ? searchedTags.Length : 0);
				for (int j = 0; j < num2; j++)
				{
					if (value.Tags[i] == searchedTags[j])
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool ContainsAllTags(int registrationId, StaticString[] searchedTags)
		{
			if (!stampDataByTargetInstanceId.TryGetValue(registrationId, out var value))
			{
				Diagnostics.LogError($"RegistrationId '{registrationId}' does not exist.");
				return false;
			}
			int num = ((searchedTags != null) ? searchedTags.Length : 0);
			for (int i = 0; i < num; i++)
			{
				bool flag = false;
				StaticString[] tags = value.Tags;
				int num2 = ((tags != null) ? tags.Length : 0);
				for (int j = 0; j < num2; j++)
				{
					if (value.Tags[j] == searchedTags[i])
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return true;
		}

		internal int Register(int keyGuid, UIComponent target)
		{
			int instanceID = target.GetInstanceID();
			stampDataByTargetInstanceId[target.GetInstanceID()] = new StampData
			{
				Target = target,
				KeyGuid = keyGuid,
				Tags = new StaticString[StampTagsCapacity]
			};
			List<int> value = null;
			if (!registrationIdsByKeyGuid.TryGetValue(keyGuid, out value))
			{
				value = new List<int>();
				registrationIdsByKeyGuid.Add(keyGuid, value);
			}
			value.Add(instanceID);
			this.StampRegistrationChanged?.Invoke(keyGuid, arg2: true);
			return instanceID;
		}

		internal void Unregister(int targetInstanceId)
		{
			if (!stampDataByTargetInstanceId.TryGetValue(targetInstanceId, out var value))
			{
				Diagnostics.LogError("Target was not registered.");
				return;
			}
			List<int> value2 = null;
			if (registrationIdsByKeyGuid.TryGetValue(value.KeyGuid, out value2))
			{
				value2.Remove(targetInstanceId);
			}
			stampDataByTargetInstanceId.Remove(targetInstanceId);
			this.StampRegistrationChanged?.Invoke(value.KeyGuid, arg2: false);
		}

		internal bool HasTags(int targetInstanceId)
		{
			if (!stampDataByTargetInstanceId.TryGetValue(targetInstanceId, out var value))
			{
				Diagnostics.LogError("Target is not registered.");
				return false;
			}
			int num = value.Tags.Length;
			for (int i = 0; i < num; i++)
			{
				if (!StaticString.IsNullOrEmpty(value.Tags[i]))
				{
					return true;
				}
			}
			return false;
		}

		internal void AddTag(int registrationId, StaticString addedTag)
		{
			if (StaticString.IsNullOrEmpty(addedTag))
			{
				Diagnostics.LogError(58uL, "Target is given an empty tag.");
				return;
			}
			if (!stampDataByTargetInstanceId.TryGetValue(registrationId, out var value))
			{
				Diagnostics.LogError("Target is not registered.");
				return;
			}
			int num = value.Tags.Length;
			for (int i = 0; i < num; i++)
			{
				StaticString staticString = value.Tags[i];
				if (StaticString.IsNullOrEmpty(staticString))
				{
					value.Tags[i] = addedTag;
					return;
				}
				if (staticString == addedTag)
				{
					Diagnostics.LogWarning("Target '{0}' was already tagged '{1}'", value.Target, addedTag);
					return;
				}
			}
			Diagnostics.LogWarning(58uL, "Target '{0}' has too many tags for standard capacity ([{1}]). Consider increasing the general capacity?", value.Target, StaticString.Join("; ", value.Tags));
			StaticString[] array = new StaticString[value.Tags.Length * 2];
			Array.Copy(value.Tags, array, value.Tags.Length);
			stampDataByTargetInstanceId[registrationId] = new StampData
			{
				Target = value.Target,
				KeyGuid = value.KeyGuid,
				Tags = array
			};
			this.StampTagsChanged?.Invoke(value.KeyGuid, addedTag, arg3: true);
		}

		internal void RemoveTag(int targetInstanceId, StaticString removedTag)
		{
			if (StaticString.IsNullOrEmpty(removedTag))
			{
				Diagnostics.LogError(58uL, "Target is given an empty tag.");
				return;
			}
			if (!stampDataByTargetInstanceId.TryGetValue(targetInstanceId, out var value))
			{
				Diagnostics.LogError("Target is not registered.");
				return;
			}
			bool flag = false;
			int num = value.Tags.Length;
			for (int i = 0; i < num; i++)
			{
				flag |= value.Tags[i] == removedTag;
				if (flag)
				{
					value.Tags[i] = ((i < num - 1) ? value.Tags[i + 1] : StaticString.Empty);
				}
				if (StaticString.IsNullOrEmpty(value.Tags[i]))
				{
					break;
				}
			}
			if (!flag)
			{
				Diagnostics.LogWarning("Tag '{0}' could not be removed for Target '{1}' ([{2}]", removedTag, value.Target, StaticString.Join("; ", value.Tags));
			}
			else
			{
				this.StampTagsChanged?.Invoke(value.KeyGuid, removedTag, arg3: false);
			}
		}

		internal void ClearTags(int targetInstanceId)
		{
			if (!stampDataByTargetInstanceId.TryGetValue(targetInstanceId, out var value))
			{
				Diagnostics.LogError("Target is not registered.");
				return;
			}
			bool flag = false;
			int num = value.Tags.Length;
			for (int i = 0; i < num && !StaticString.IsNullOrEmpty(value.Tags[i]); i++)
			{
				flag = true;
				value.Tags[i] = StaticString.Empty;
			}
			if (flag)
			{
				this.StampTagsCleared?.Invoke(value.KeyGuid);
			}
		}

		protected override void Load()
		{
			base.Load();
			ReloadStampKeys();
		}

		protected override void Unload()
		{
			guidByKeyDictionary.Clear();
			base.Unload();
		}

		protected override void Destruct()
		{
			instance = null;
			base.Destruct();
		}

		private void ReloadStampKeys()
		{
			guidByKeyDictionary.Clear();
			if (stampKeysDefinition != null)
			{
				int num = ((stampKeysDefinition.StampKeys != null) ? stampKeysDefinition.StampKeys.Length : 0);
				for (int i = 0; i < num; i++)
				{
					guidByKeyDictionary.Add(new StaticString(stampKeysDefinition.StampKeys[i].Key), stampKeysDefinition.StampKeys[i].ShortGuid);
				}
			}
		}

		internal bool IsStampKeyValid(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return false;
			}
			return !guidByKeyDictionary.ContainsKey(new StaticString(key));
		}

		internal int CreateStampKey(string key)
		{
			Diagnostics.LogError(58uL, "Not implemented (UNITY_EDITOR only)");
			return UIStampKeysDefinition.NullKeyGuid;
		}

		public bool TryFindStampData(int registrationId, out int keyGuid, out StaticString[] tagsCopy)
		{
			if (stampDataByTargetInstanceId.TryGetValue(registrationId, out var value))
			{
				keyGuid = value.KeyGuid;
				int num = -1;
				_ = value.Tags.Length;
				for (int i = 0; i < value.Tags.Length; i++)
				{
					if (StaticString.IsNullOrEmpty(value.Tags[i]))
					{
						num = i;
						break;
					}
				}
				tagsCopy = new StaticString[num];
				Array.Copy(value.Tags, tagsCopy, num);
				return true;
			}
			keyGuid = UIStampKeysDefinition.NullKeyGuid;
			tagsCopy = null;
			return false;
		}

		public StaticString FindKeyFromGuid(int keyGuid)
		{
			foreach (KeyValuePair<StaticString, int> item in guidByKeyDictionary)
			{
				if (item.Value == keyGuid)
				{
					return item.Key;
				}
			}
			return StaticString.Empty;
		}
	}
}
