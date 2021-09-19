using System;
using UnityEngine;

namespace Amplitude.UI.Windows
{
	[CreateAssetMenu(fileName = "NewWindowsGroupDefinition.asset", menuName = "Amplitude/UI/Windows Group")]
	public class UIWindowsGroupDefinition : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField]
		[HideInInspector]
		private PrefabReference[] windowPrefabs;

		[NonSerialized]
		private Type groupType;

		[SerializeField]
		[HideInInspector]
		private string serializableType = string.Empty;

		[SerializeField]
		private int criticity;

		[SerializeField]
		[UILayerIdentifier]
		private int rootLayerIndex = -1;

		public Type GroupType => groupType;

		public int Criticity => criticity;

		public int Length => windowPrefabs.Length;

		public int RootLayerIndex => rootLayerIndex;

		public Transform GetPrefab(int index)
		{
			if (windowPrefabs[index] == null)
			{
				return null;
			}
			return windowPrefabs[index].Transform;
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (!string.IsNullOrEmpty(serializableType))
			{
				groupType = Type.GetType(serializableType);
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (groupType != null)
			{
				serializableType = groupType.AssemblyQualifiedName;
			}
		}

		internal void SetGroupType(Type type)
		{
			groupType = type;
		}
	}
}
