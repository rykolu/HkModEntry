using System.Collections.Generic;

namespace Amplitude.UI.Text
{
	public class SymbolMapperController
	{
		private Dictionary<StaticString, int> identifiersPerTag = new Dictionary<StaticString, int>();

		private List<SymbolMapper> allSymbolMappers = new List<SymbolMapper>();

		public bool TryFindSymbolMapper(StaticString tag, out SymbolMapper mapper, out int identifier)
		{
			mapper = null;
			identifier = -1;
			if (!identifiersPerTag.TryGetValue(tag, out identifier))
			{
				return false;
			}
			mapper = allSymbolMappers[identifier];
			return true;
		}

		public SymbolMapper GetSymbolMapper(int identifier)
		{
			if (identifier < 0 || identifier >= allSymbolMappers.Count)
			{
				Diagnostics.LogWarning(52uL, $"Identifier '{identifier}' is not valid for a SymbolMapper");
				return null;
			}
			return allSymbolMappers[identifier];
		}

		internal void Load(SymbolMappersCollection[] allCollections)
		{
			identifiersPerTag.Clear();
			allSymbolMappers.Clear();
			int num = ((allCollections != null) ? allCollections.Length : 0);
			for (int i = 0; i < num; i++)
			{
				SymbolMappersCollection symbolMappersCollection = allCollections[i];
				if (symbolMappersCollection == null)
				{
					continue;
				}
				int num2 = symbolMappersCollection.SymbolMappers?.Count ?? 0;
				for (int j = 0; j < num2; j++)
				{
					SymbolMapper symbolMapper = symbolMappersCollection.SymbolMappers[j];
					StaticString tag = symbolMapper.Tag;
					if (identifiersPerTag.ContainsKey(tag))
					{
						Diagnostics.LogWarning(52uL, "SymbolMapper name '" + symbolMapper.name + "' already exists.");
						continue;
					}
					identifiersPerTag.Add(tag, allSymbolMappers.Count);
					allSymbolMappers.Add(symbolMapper);
				}
			}
		}

		internal void Unload()
		{
			identifiersPerTag.Clear();
			allSymbolMappers.Clear();
		}
	}
}
