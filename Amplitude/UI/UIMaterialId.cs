using System;
using UnityEngine;

namespace Amplitude.UI
{
	[Serializable]
	public struct UIMaterialId
	{
		[SerializeField]
		private string materialName;

		private StaticString materialNameId;

		public StaticString Id
		{
			get
			{
				if (StaticString.IsNullOrEmpty(materialNameId) || Application.isEditor)
				{
					materialNameId = new StaticString(materialName);
				}
				return materialNameId;
			}
		}

		public UIMaterialId(string materialName)
		{
			this.materialName = materialName;
			materialNameId = new StaticString(this.materialName);
		}
	}
}
