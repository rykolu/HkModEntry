namespace Amplitude.UI
{
	public abstract class UIAbstractRenderRequest : IUIRenderRequest
	{
		private struct RenderCommandRegistation
		{
			public UIView View;

			public long SortingIndex;

			public int LayerIndex;

			public RenderCommandRegistation(UIView view, long sortingIndex, int layerIndex)
			{
				View = view;
				SortingIndex = sortingIndex;
				LayerIndex = layerIndex;
			}

			public void Remove(UIBehaviour owner)
			{
				if ((bool)View && View.Loaded)
				{
					View.RenderCommands.Remove(SortingIndex, LayerIndex, owner);
				}
				View = null;
				SortingIndex = 0L;
				LayerIndex = 0;
			}
		}

		protected UIBehaviour owner;

		private RenderCommandRegistation renderCommandRegistation0;

		private RenderCommandRegistation renderCommandRegistation1;

		private RenderCommandRegistation renderCommandRegistation2;

		private RenderCommandRegistation renderCommandRegistation3;

		private int renderCommandRegistrationCount;

		public UIAbstractRenderRequest(UIBehaviour owner)
		{
			this.owner = owner;
		}

		public abstract void BindRenderCommands(UIView view);

		public void UnbindRenderCommands()
		{
			if (renderCommandRegistrationCount > 0)
			{
				renderCommandRegistation0.Remove(owner);
			}
			if (renderCommandRegistrationCount > 1)
			{
				renderCommandRegistation1.Remove(owner);
			}
			if (renderCommandRegistrationCount > 2)
			{
				renderCommandRegistation2.Remove(owner);
			}
			if (renderCommandRegistrationCount > 3)
			{
				renderCommandRegistation3.Remove(owner);
			}
			renderCommandRegistrationCount = 0;
		}

		protected void AddRenderCommand(UIView view, ref UIRenderCommand renderCommand)
		{
			switch (renderCommandRegistrationCount)
			{
			default:
				return;
			case 0:
				renderCommandRegistation0 = new RenderCommandRegistation(view, renderCommand.SortingIndex, renderCommand.LayerIndex);
				break;
			case 1:
				renderCommandRegistation1 = new RenderCommandRegistation(view, renderCommand.SortingIndex, renderCommand.LayerIndex);
				break;
			case 2:
				renderCommandRegistation2 = new RenderCommandRegistation(view, renderCommand.SortingIndex, renderCommand.LayerIndex);
				break;
			case 3:
				renderCommandRegistation3 = new RenderCommandRegistation(view, renderCommand.SortingIndex, renderCommand.LayerIndex);
				break;
			}
			view.RenderCommands.Add(ref renderCommand);
			renderCommandRegistrationCount++;
		}
	}
}
