using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amplitude.UI
{
	[Serializable]
	public class UILayerList
	{
		[SerializeField]
		public List<UILayer> Elements = new List<UILayer>();

		public int Count => Elements.Count;

		public UILayer this[int index] => Elements[index];
	}
}
