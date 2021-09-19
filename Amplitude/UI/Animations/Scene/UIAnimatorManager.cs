using System.Collections.Generic;
using UnityEngine;

namespace Amplitude.UI.Animations.Scene
{
	[ExecuteInEditMode]
	public class UIAnimatorManager : UIBehaviour
	{
		private static UIAnimatorManager instance;

		private List<UIAnimatorComponent> animationsInProgress = new List<UIAnimatorComponent>();

		private List<UIAnimatorComponent> animationsWithPendingEvents = new List<UIAnimatorComponent>();

		private bool freezeActivations;

		internal static UIAnimatorManager Instance => instance;

		protected UIAnimatorManager()
		{
			instance = this;
		}

		public void StartAnimation(UIAnimatorComponent animationComponent)
		{
			animationsInProgress.Add(animationComponent);
		}

		public void StopAnimation(UIAnimatorComponent animationComponent)
		{
			animationsInProgress.Remove(animationComponent);
		}

		internal void SpecificUpdate()
		{
			animationsWithPendingEvents.AddRange(animationsInProgress);
			freezeActivations = true;
			for (int num = animationsInProgress.Count - 1; num >= 0; num--)
			{
				if (!animationsInProgress[num].UpdateAnimation())
				{
					animationsInProgress.RemoveAt(num);
				}
			}
			freezeActivations = false;
			int count = animationsWithPendingEvents.Count;
			for (int i = 0; i < count; i++)
			{
				animationsWithPendingEvents[i].TriggerPendingEvents();
			}
			animationsWithPendingEvents.Clear();
		}

		protected override void Destruct()
		{
			instance = null;
			base.Destruct();
		}

		internal void OnAssetChanged(UIAnimationEditionEventArg arg)
		{
			Diagnostics.LogWarning(54uL, "Why are we receiving Edition messages if !UNITY_EDITOR ?");
		}

		private void PropagateAnimationEventEvent(UITransform root, UIAnimationEditionEventArg arg)
		{
			UIAnimatorComponent[] components = root.GetComponents<UIAnimatorComponent>();
			int num = components.Length;
			for (int i = 0; i < num; i++)
			{
				components[i].OnAnimationEvent(arg);
			}
			int count = root.Children.Count;
			for (int j = 0; j < count; j++)
			{
				PropagateAnimationEventEvent(root.Children.Data[j], arg);
			}
		}
	}
}
