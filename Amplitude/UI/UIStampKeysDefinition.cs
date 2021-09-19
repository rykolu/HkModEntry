using System;
using UnityEngine;

namespace Amplitude.UI
{
	[CreateAssetMenu(menuName = "Amplitude/UI/Stamps Keys Definition")]
	public class UIStampKeysDefinition : ScriptableObject
	{
		[Serializable]
		public struct UIStampKey
		{
			public int ShortGuid;

			public string Key;
		}

		public static int NullKeyGuid;

		[SerializeField]
		public UIStampKey[] StampKeys;

		internal bool Contains(int shortGuid)
		{
			int num = ((StampKeys != null) ? StampKeys.Length : 0);
			for (int i = 0; i < num; i++)
			{
				if (StampKeys[i].ShortGuid == shortGuid)
				{
					return true;
				}
			}
			return false;
		}
	}
}
