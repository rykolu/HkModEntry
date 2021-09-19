using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amplitude.UI.Text
{
	[CreateAssetMenu(fileName = "UISymbolMappersCollection.asset", menuName = "Amplitude/UI/SymbolMappersCollection")]
	public class SymbolMappersCollection : ScriptableObject
	{
		[SerializeField]
		private List<SymbolMapper> symbolMappers = new List<SymbolMapper>();

		public List<SymbolMapper> SymbolMappers => symbolMappers;

		public virtual IEnumerable<Type> GetTypesOfMappers()
		{
			yield return typeof(SymbolToTextMapper);
			yield return typeof(SymbolToRectMapper);
			yield return typeof(SymbolToSquircleMapper);
		}

		public bool TryGetMapper(StaticString tag, out SymbolMapper mapper, out short identifier)
		{
			int count = symbolMappers.Count;
			for (int i = 0; i < count; i++)
			{
				if (symbolMappers[i].Tag == tag)
				{
					mapper = symbolMappers[i];
					identifier = (short)i;
					return true;
				}
			}
			mapper = null;
			identifier = -1;
			return false;
		}

		public SymbolMapper GetMapperById(int id)
		{
			if (id >= 0 && id < symbolMappers.Count)
			{
				return symbolMappers[id];
			}
			Diagnostics.LogError("Could not find any SymbolMapper with id '{0}'", id);
			return null;
		}

		public void Unload()
		{
			int num = ((symbolMappers != null) ? symbolMappers.Count : 0);
			for (int i = 0; i < num; i++)
			{
				symbolMappers[i].Unload();
			}
		}
	}
}
