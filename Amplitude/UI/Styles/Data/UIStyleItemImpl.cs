using System;
using UnityEngine;

namespace Amplitude.UI.Styles.Data
{
	public abstract class UIStyleItemImpl<TValueType> : UIStyleItem
	{
		[SerializeField]
		private TValueType value;

		[SerializeField]
		private string[] reactivityKeys;

		[NonSerialized]
		private UIStyleReactivityKey[] processedReactivityKeys;

		[SerializeField]
		private TValueType[] reactivityValues;

		[SerializeField]
		private AnimationCurve transitionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		private float duration = 0.1f;

		internal override Type ValueType => typeof(TValueType);

		internal TValueType Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}

		internal override AnimationCurve TransitionCurve => transitionCurve;

		internal override float Duration => duration;

		internal override UIStyleReactivityKey[] ReactivityKeys => processedReactivityKeys;

		internal override int ReactivityKeysCount
		{
			get
			{
				string[] array = reactivityKeys;
				if (array == null)
				{
					return 0;
				}
				return array.Length;
			}
		}

		internal override void Load()
		{
			base.Load();
			string[] array = reactivityKeys;
			int num = ((array != null) ? array.Length : 0);
			processedReactivityKeys = new UIStyleReactivityKey[num];
			for (int i = 0; i < num; i++)
			{
				processedReactivityKeys[i] = new UIStyleReactivityKey(reactivityKeys[i]);
			}
		}

		internal override string GetReactivityKeyAsString(int index)
		{
			if (reactivityKeys != null && index >= 0 && index < reactivityKeys.Length)
			{
				return reactivityKeys[index];
			}
			return string.Empty;
		}

		internal TValueType GetValue(int reactivityIndex = -1)
		{
			TValueType val = default(TValueType);
			val = ((reactivityValues == null || reactivityIndex < 0 || reactivityIndex >= reactivityValues.Length) ? value : reactivityValues[reactivityIndex]);
			if (typeof(TValueType).IsClass)
			{
				ICloneable cloneable = val as ICloneable;
				if (cloneable != null)
				{
					val = (TValueType)cloneable.Clone();
				}
			}
			return val;
		}
	}
}
