namespace Amplitude.UI
{
	public struct UIRenderCommand
	{
		public delegate void RenderCallback(UIPrimitiveDrawer drawer);

		public UIBehaviour Owner;

		public RenderCallback Render;

		private readonly long sortingIndex;

		private readonly int layerIndex;

		public long SortingIndex => sortingIndex;

		public int LayerIndex => layerIndex;

		public UIRenderCommand(long sortingIndex, UIView view, int layerIdentifier, RenderCallback renderCallback, UIBehaviour owner)
		{
			this.sortingIndex = sortingIndex;
			layerIndex = 0;
			view.LayerOrderedIndex(layerIdentifier, ref layerIndex);
			Owner = owner;
			Render = renderCallback;
		}

		public void Clear()
		{
			Owner = null;
			Render = null;
		}

		public override string ToString()
		{
			return $"{Render.Target} {Render.Method.Name}()";
		}
	}
}
