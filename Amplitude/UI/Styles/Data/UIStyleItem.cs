using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Amplitude.UI.Styles.Data
{
	public abstract class UIStyleItem : ScriptableObject
	{
		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("Identifier")]
		private string identifier = string.Empty;

		[NonSerialized]
		private StaticString cachedIdentifier = StaticString.Empty;

		internal abstract Type ValueType { get; }

		internal abstract UIStyleReactivityKey[] ReactivityKeys { get; }

		internal abstract int ReactivityKeysCount { get; }

		internal abstract AnimationCurve TransitionCurve { get; }

		internal abstract float Duration { get; }

		public override string ToString()
		{
			return base.name;
		}

		internal virtual void Load()
		{
			if (!string.IsNullOrEmpty(base.name))
			{
				cachedIdentifier = new StaticString(base.name);
			}
			else
			{
				SetName(identifier);
			}
		}

		internal abstract string GetReactivityKeyAsString(int index);

		internal StaticString GetIdentifier()
		{
			return cachedIdentifier;
		}

		internal void SetName(string newName)
		{
			base.name = newName.ToString();
			cachedIdentifier = new StaticString(newName);
		}

		private void OnEnable()
		{
			Load();
		}
	}
}
