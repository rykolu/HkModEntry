using System;
using UnityEngine;

namespace Amplitude.UI
{
	[Serializable]
	public struct UIStamp
	{
		[SerializeField]
		[UIStampKey]
		private int keyGuid;

		[NonSerialized]
		private int registrationId;

		public int KeyGuid => keyGuid;

		public int RegistrationId => registrationId;

		public bool IsLoaded => registrationId != 0;

		public bool HasTags()
		{
			if (UIStampsManager.Instance == null)
			{
				return false;
			}
			return UIStampsManager.Instance.HasTags(registrationId);
		}

		public void AddTag(StaticString tag)
		{
			UIStampsManager.Instance?.AddTag(registrationId, tag);
		}

		public void RemoveTag(StaticString tag)
		{
			UIStampsManager.Instance?.RemoveTag(registrationId, tag);
		}

		public void ClearTags()
		{
			UIStampsManager.Instance?.ClearTags(registrationId);
		}

		internal void LoadIfNecessary(UIComponent owner)
		{
			if (!IsLoaded && keyGuid != UIStampKeysDefinition.NullKeyGuid && UIStampsManager.Instance != null)
			{
				registrationId = UIStampsManager.Instance.Register(keyGuid, owner);
			}
		}

		internal void Unload()
		{
			if (IsLoaded)
			{
				UIStampsManager.Instance?.Unregister(registrationId);
				registrationId = UIStampsManager.InvalidRegistrationId;
			}
		}

		internal void Reload(UIComponent owner)
		{
			if (IsLoaded)
			{
				Unload();
			}
			LoadIfNecessary(owner);
		}
	}
}
