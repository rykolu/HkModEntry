using System;
using UnityEngine;

namespace Amplitude.UI.Text
{
	[Serializable]
	public class SymbolToTextMapper : SymbolMapper
	{
		[SerializeField]
		private string text;

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
			}
		}
	}
}
