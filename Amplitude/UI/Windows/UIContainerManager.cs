using System.Collections.Generic;

namespace Amplitude.UI.Windows
{
	public class UIContainerManager : UIBehaviour
	{
		private enum DelayedOperationType
		{
			Register,
			Unregister
		}

		private struct DelayedOperation
		{
			public readonly UIContainer Container;

			public readonly DelayedOperationType Type;

			public DelayedOperation(UIContainer container, DelayedOperationType type)
			{
				Container = container;
				Type = type;
			}
		}

		private struct ContainerShell
		{
			public UIContainer Container;

			public string ProfileLabel;

			public ContainerShell(UIContainer instance)
			{
				Container = instance;
				ProfileLabel = $"{Container.GetType()}.UpdateAndCheckDirty";
			}
		}

		private static UIContainerManager instance;

		private List<ContainerShell> activeContainers = new List<ContainerShell>();

		private bool shouldDelayOperations;

		private List<DelayedOperation> delayedOperation = new List<DelayedOperation>();

		private int unregisterCount;

		private int registerCount;

		internal static UIContainerManager Instance => instance;

		protected UIContainerManager()
		{
			instance = this;
		}

		public void Clear()
		{
			activeContainers.Clear();
		}

		public void SpecificUpdate()
		{
			shouldDelayOperations = true;
			int count = activeContainers.Count;
			for (int i = 0; i < count; i++)
			{
				activeContainers[i].Container.UpdateAndCheckDirty();
			}
			shouldDelayOperations = false;
			int count2 = delayedOperation.Count;
			for (int j = 0; j < count2; j++)
			{
				switch (delayedOperation[j].Type)
				{
				case DelayedOperationType.Register:
					Register(delayedOperation[j].Container);
					delayedOperation[j].Container.UpdateAndCheckDirty();
					break;
				case DelayedOperationType.Unregister:
					Unregister(delayedOperation[j].Container);
					break;
				}
			}
			delayedOperation.Clear();
		}

		internal void Register(UIContainer container)
		{
			if (!shouldDelayOperations)
			{
				registerCount++;
				activeContainers.Add(new ContainerShell(container));
				return;
			}
			int count = this.delayedOperation.Count;
			for (int i = 0; i < count; i++)
			{
				DelayedOperation delayedOperation = this.delayedOperation[i];
				if (!(delayedOperation.Container == container))
				{
					continue;
				}
				if (delayedOperation.Type != 0)
				{
					if (delayedOperation.Type == DelayedOperationType.Unregister)
					{
						this.delayedOperation[i] = new DelayedOperation(container, DelayedOperationType.Register);
						return;
					}
					object[] array = new object[1];
					DelayedOperationType type = delayedOperation.Type;
					array[0] = type.ToString();
					Diagnostics.LogError("DelayedOperationType {0} should be handled !", array);
				}
				return;
			}
			this.delayedOperation.Add(new DelayedOperation(container, DelayedOperationType.Register));
		}

		internal void Unregister(UIContainer container)
		{
			if (!shouldDelayOperations)
			{
				unregisterCount++;
				int count = activeContainers.Count;
				for (int i = 0; i < count; i++)
				{
					if (activeContainers[i].Container == container)
					{
						activeContainers.RemoveAt(i);
						break;
					}
				}
				return;
			}
			int count2 = this.delayedOperation.Count;
			for (int j = 0; j < count2; j++)
			{
				DelayedOperation delayedOperation = this.delayedOperation[j];
				if (!(delayedOperation.Container == container))
				{
					continue;
				}
				if (delayedOperation.Type != DelayedOperationType.Unregister)
				{
					if (delayedOperation.Type == DelayedOperationType.Register)
					{
						this.delayedOperation[j] = new DelayedOperation(container, DelayedOperationType.Unregister);
						return;
					}
					object[] array = new object[1];
					DelayedOperationType type = delayedOperation.Type;
					array[0] = type.ToString();
					Diagnostics.LogError("DelayedOperationType {0} should be handled !", array);
				}
				return;
			}
			this.delayedOperation.Add(new DelayedOperation(container, DelayedOperationType.Unregister));
		}

		protected override void Destruct()
		{
			instance = null;
			base.Destruct();
		}
	}
}
