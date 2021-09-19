using System.Collections.Generic;
using UnityEngine;

namespace Amplitude.UI.Animations
{
	[ExecuteInEditMode]
	public class UIAnimationManager : UIBehaviour
	{
		private struct PendingActivation
		{
			public readonly IUIAnimationManagedController Controller;

			public readonly bool Activate;

			public PendingActivation(IUIAnimationManagedController controller, bool active)
			{
				Controller = controller;
				Activate = active;
			}
		}

		private static UIAnimationManager instance;

		private List<IUIAnimationManagedController> animatedControllers = new List<IUIAnimationManagedController>();

		private Queue<PendingActivation> pendingActivations = new Queue<PendingActivation>();

		private bool freezeActivations;

		internal static UIAnimationManager Instance => instance;

		protected UIAnimationManager()
		{
			instance = this;
		}

		public void StartAnimation(IUIAnimationManagedController controller)
		{
			if (freezeActivations)
			{
				pendingActivations.Enqueue(new PendingActivation(controller, active: true));
			}
			else if (!controller.Active)
			{
				controller.SetActive(value: true);
				animatedControllers.Add(controller);
			}
		}

		public void StopAnimation(IUIAnimationManagedController controller)
		{
			if (freezeActivations)
			{
				pendingActivations.Enqueue(new PendingActivation(controller, active: false));
			}
			else if (controller.Active)
			{
				animatedControllers.Remove(controller);
				controller.SetActive(value: false);
			}
		}

		public void NotifyTemplateUpdate(UIAnimationTemplate template)
		{
			if (UIHierarchyManager.Instance != null)
			{
				for (int i = 0; i < UITransform.Roots.Count; i++)
				{
					OnTemplateUpdatedRecursive(UITransform.Roots.Data[i], template);
				}
			}
		}

		internal void SpecificUpdate()
		{
			freezeActivations = true;
			for (int num = animatedControllers.Count - 1; num >= 0; num--)
			{
				IUIAnimationManagedController iUIAnimationManagedController = animatedControllers[num];
				if (!iUIAnimationManagedController.UpdateAnimation())
				{
					animatedControllers.RemoveAt(num);
					iUIAnimationManagedController.SetActive(value: false);
				}
			}
			freezeActivations = false;
			while (pendingActivations.Count > 0)
			{
				PendingActivation pendingActivation = pendingActivations.Dequeue();
				if (pendingActivation.Activate)
				{
					StartAnimation(pendingActivation.Controller);
				}
				else
				{
					StopAnimation(pendingActivation.Controller);
				}
			}
		}

		protected override void Destruct()
		{
			instance = null;
			base.Destruct();
		}

		private void OnTemplateUpdatedRecursive(UITransform root, UIAnimationTemplate template)
		{
			OnTemplateUpdated(root, template);
			int count = root.Children.Count;
			for (int i = 0; i < count; i++)
			{
				OnTemplateUpdatedRecursive(root.Children.Data[i], template);
			}
		}

		private void OnTemplateUpdated(UITransform transform, UIAnimationTemplate template)
		{
			IUIAnimationControllerOwner[] components = transform.GetComponents<IUIAnimationControllerOwner>();
			int num = ((components != null) ? components.Length : 0);
			for (int i = 0; i < num; i++)
			{
				(components[i].AnimationController as IUIAnimationManagedController)?.UpdateTemplate(template);
			}
		}
	}
}
