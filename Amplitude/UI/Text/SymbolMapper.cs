using System;
using UnityEngine;

namespace Amplitude.UI.Text
{
	[Serializable]
	public abstract class SymbolMapper : ScriptableObject
	{
		[SerializeField]
		private string tag = string.Empty;

		[NonSerialized]
		private StaticString tagAsStaticString;

		public StaticString Tag => tagAsStaticString;

		public virtual void Unload()
		{
		}

		private void OnEnable()
		{
			tagAsStaticString = new StaticString(tag.ToUpper());
		}

		private void OnValidate()
		{
			tagAsStaticString = new StaticString(tag.ToUpper());
		}
	}
}
