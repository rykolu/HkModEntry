using System;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[CreateAssetMenu(fileName = "NewAnimationTemplate.asset", menuName = "Amplitude/UI/AnimationTemplate")]
	public class UIAnimationTemplate : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField]
		private UIAnimationItemsCollection items = new UIAnimationItemsCollection();

		[NonSerialized]
		private bool deserialized;

		public IUIAnimationItemsReadOnlyCollection Items => items;

		public bool Deserialized => deserialized;

		public event Action OnAfterDeserialization;

		public void SaveItems(IUIAnimationController controller)
		{
			items.Copy(controller.Items);
			UIAnimationManager instance = UIAnimationManager.Instance;
			if (instance != null)
			{
				instance.NotifyTemplateUpdate(this);
			}
		}

		public void SaveAsset()
		{
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			deserialized = true;
			this.OnAfterDeserialization?.Invoke();
			this.OnAfterDeserialization = null;
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}
	}
}
