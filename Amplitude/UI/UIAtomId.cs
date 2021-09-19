namespace Amplitude.UI
{
	public struct UIAtomId
	{
		public const int InvalidAllocator = 0;

		public const int LinearAllocatorIndex = 1;

		public const int PoolAllocatorIndex = 2;

		public const int BlockAllocatorIndex = 3;

		public static readonly UIAtomId Invalid;

		public int Index;

		public int Allocator;

		public bool IsValid => Allocator != 0;
	}
}
