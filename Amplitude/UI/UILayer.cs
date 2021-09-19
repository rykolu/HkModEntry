using System;
using UnityEngine;

namespace Amplitude.UI
{
	[Serializable]
	public class UILayer
	{
		[SerializeField]
		public int Identifier;

		public UILayer(int identifier)
		{
			Identifier = identifier;
		}
	}
}
