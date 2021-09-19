using System;
using Amplitude.Framework;
using Amplitude.Framework.Asset;
using UnityEngine;

namespace Amplitude.UI
{
	[Serializable]
	public class PrefabReference : AssetReference<GameObject>
	{
		[NonSerialized]
		private Transform rootTransform;

		public new GameObject Value
		{
			get
			{
				if (cachedAsset == null && !base.Guid.IsNull)
				{
					Load(64u);
				}
				return cachedAsset;
			}
		}

		public Transform Transform
		{
			get
			{
				if (rootTransform == null)
				{
					GameObject value = Value;
					rootTransform = ((value != null) ? value.transform : null);
				}
				return rootTransform;
			}
		}

		public PrefabReference()
		{
		}

		public PrefabReference(Amplitude.Framework.Guid guid)
			: base(guid)
		{
		}

		public new void Load()
		{
			Load(64u);
		}

		public override void Flush()
		{
			base.Flush();
			rootTransform = null;
		}
	}
}
