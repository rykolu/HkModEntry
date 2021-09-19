using System;
using System.Diagnostics;
using Amplitude.Graphics;
using UnityEngine;

namespace Amplitude.UI
{
	public struct UIAtomContainer<T> where T : struct
	{
		private static class ProfilerNames
		{
			public static readonly string Prefix = "UI|Atom|" + typeof(T).ToString() + "|";

			public static readonly string TotalAllocatedPageCount = Prefix + "TotalAllocatedPageCount";

			public static readonly string TotalAllocatedMemory = Prefix + "TotalAllocatedMemory (kB)";

			public static readonly string LinearAllocatedPageCount = Prefix + "Linear|AllocatedPageCount";

			public static readonly string LinearAllocatedMemory = Prefix + "Linear|AllocatedMemory (kB)";

			public static readonly string PoolAllocatedPageCount = Prefix + "Pool|AllocatedPageCount";

			public static readonly string PoolAllocatedMemory = Prefix + "Pool|AllocatedMemory (kB)";

			public static readonly string BlockAllocatedPageCount = Prefix + "Block|AllocatedPageCount";

			public static readonly string BlockAllocatedMemory = Prefix + "Block|AllocatedMemory (kB)";

			public static readonly string GpuUploadedMemory = Prefix + "GpuUploadedMemory (kB)";

			public static readonly string FrameAllocationCount = Prefix + "FrameAllocationCount";

			public static readonly string PersistantAllocationCount = Prefix + "PersistantAllocationCount";

			public static readonly string SetDataCount = Prefix + "SetData";
		}

		public static UIAtomContainer<T> Global;

		public UIStorage<T> Storage;

		private UILinearAllocator<T> linearAllocator;

		private UIPoolAllocator<T> poolAllocator;

		private UIBlockAllocator<T> blockAllocator;

		private ReadWriteBuffer1D<T> computeBuffer;

		private ShaderString globalShaderString;

		private bool loaded;

		private SynchroniseUIAtomContainerOptionEnum synchroniseOption;

		private int synchroniseDate;

		public bool Loaded => loaded;

		public static UIAtomId Allocate()
		{
			T value = new T();
			return Allocate(ref value);
		}

		public static UIAtomId Allocate(ref T value)
		{
			UIAtomId result = default(UIAtomId);
			result.Index = Global.poolAllocator.Allocate(ref Global.Storage, ref value);
			result.Allocator = 2;
			return result;
		}

		public static UIAtomId Allocate(int size)
		{
			UIAtomId invalid = UIAtomId.Invalid;
			if (size == 1)
			{
				T value = new T();
				invalid.Index = Global.poolAllocator.Allocate(ref Global.Storage, ref value);
				invalid.Allocator = 2;
			}
			else
			{
				invalid.Index = Global.blockAllocator.Allocate(ref Global.Storage, size);
				invalid.Allocator = 3;
			}
			return invalid;
		}

		public static UIAtomId Allocate(int size, T[] datas)
		{
			UIAtomId invalid = UIAtomId.Invalid;
			if (size == 1)
			{
				invalid.Index = Global.poolAllocator.Allocate(ref Global.Storage, ref datas[0]);
				invalid.Allocator = 2;
			}
			else
			{
				invalid.Index = Global.blockAllocator.Allocate(ref Global.Storage, size);
				invalid.Allocator = 3;
				Array.Copy(datas, 0, Global.Storage.Memory, invalid.Index, size);
			}
			return invalid;
		}

		[Obsolete("Use AllocateTemporary(ref T value)")]
		public static UIAtomId AllocateTemporary()
		{
			return Allocate(UIAllocationType.Frame);
		}

		public static UIAtomId AllocateTemporary(ref T value)
		{
			UIAtomId result = default(UIAtomId);
			result.Index = Global.linearAllocator.Allocate(ref Global.Storage, ref value);
			result.Allocator = 1;
			return result;
		}

		public static UIAtomId AllocateTemporary(int size)
		{
			UIAtomId invalid = UIAtomId.Invalid;
			invalid.Index = Global.linearAllocator.Allocate(ref Global.Storage, size);
			invalid.Allocator = 1;
			return invalid;
		}

		public static UIAtomId AllocateTemporary(int size, T[] datas)
		{
			UIAtomId invalid = UIAtomId.Invalid;
			invalid.Index = Global.linearAllocator.Allocate(ref Global.Storage, size);
			invalid.Allocator = 1;
			Array.Copy(datas, 0, Global.Storage.Memory, invalid.Index, size);
			return invalid;
		}

		[Obsolete("Use Allocate or AllocateTemporary")]
		public static UIAtomId Allocate(UIAllocationType allocationType)
		{
			UIAtomId invalid = UIAtomId.Invalid;
			switch (allocationType)
			{
			case UIAllocationType.Frame:
				invalid.Index = Global.linearAllocator.Allocate(ref Global.Storage);
				invalid.Allocator = 1;
				break;
			case UIAllocationType.Persistant:
				invalid.Index = Global.poolAllocator.Allocate(ref Global.Storage);
				invalid.Allocator = 2;
				break;
			}
			return invalid;
		}

		[Obsolete("Use Allocate or AllocateTemporary")]
		public static UIAtomId Allocate(int size, UIAllocationType allocationType)
		{
			UIAtomId invalid = UIAtomId.Invalid;
			if (size == 1)
			{
				T value = new T();
				switch (allocationType)
				{
				case UIAllocationType.Frame:
					invalid.Index = Global.linearAllocator.Allocate(ref Global.Storage, ref value);
					invalid.Allocator = 1;
					break;
				case UIAllocationType.Persistant:
					invalid.Index = Global.poolAllocator.Allocate(ref Global.Storage, ref value);
					invalid.Allocator = 2;
					break;
				}
			}
			else
			{
				switch (allocationType)
				{
				case UIAllocationType.Frame:
					invalid.Index = Global.linearAllocator.Allocate(ref Global.Storage, size);
					invalid.Allocator = 1;
					break;
				case UIAllocationType.Persistant:
					invalid.Index = Global.blockAllocator.Allocate(ref Global.Storage, size);
					invalid.Allocator = 3;
					break;
				}
			}
			return invalid;
		}

		public static void Deallocate(ref UIAtomId atom)
		{
			if (!Global.loaded)
			{
				atom = UIAtomId.Invalid;
				return;
			}
			switch (atom.Allocator)
			{
			case 1:
				Global.linearAllocator.Deallocate(ref Global.Storage, atom.Index);
				break;
			case 2:
				Global.poolAllocator.Deallocate(ref Global.Storage, atom.Index);
				break;
			case 3:
				Global.blockAllocator.Deallocate(ref Global.Storage, atom.Index);
				break;
			}
			atom = UIAtomId.Invalid;
		}

		[Obsolete("Use GetData(ref id)")]
		public static T GetData(UIAtomId id)
		{
			return GetData(ref id);
		}

		public static T GetData(ref UIAtomId id)
		{
			return Global.Storage.Memory[id.Index];
		}

		[Obsolete("Use GetData(ref id, offset)")]
		public static T GetData(UIAtomId id, int offset)
		{
			return GetData(ref id, offset);
		}

		public static T GetData(ref UIAtomId id, int offset)
		{
			return Global.Storage.Memory[id.Index + offset];
		}

		[Obsolete("Use SetData(ref id)")]
		public static void SetData(UIAtomId id, ref T data)
		{
			SetData(ref id, ref data);
		}

		public static void SetData(ref UIAtomId id, ref T data)
		{
			Global.Storage.Memory[id.Index] = data;
			int num = id.Index / Global.Storage.PageCapacity;
			Global.Storage.Pages.Data[num].ComputeBufferDirty = true;
		}

		[Obsolete("Use SetData(ref id, offet, ref data)")]
		public static void SetData(UIAtomId id, int offset, ref T data)
		{
			SetData(ref id, offset, ref data);
		}

		public static void SetData(ref UIAtomId id, int offset, ref T data)
		{
			int num = id.Index + offset;
			Global.Storage.Memory[num] = data;
			int num2 = num / Global.Storage.PageCapacity;
			Global.Storage.Pages.Data[num2].ComputeBufferDirty = true;
		}

		[Obsolete("Use SetData(ref id, ref data)")]
		public static void SetData(UIAtomId id, T[] data)
		{
			SetData(ref id, data);
		}

		public static void SetData(ref UIAtomId id, T[] data)
		{
			int index = id.Index;
			int num = data.Length;
			for (int i = 0; i < num; i++)
			{
				Global.Storage.Memory[index + i] = data[i];
			}
			int num2 = index / Global.Storage.PageCapacity;
			Global.Storage.Pages.Data[num2].ComputeBufferDirty = true;
		}

		public void Load(int pageSize, int startPageCount, SynchroniseUIAtomContainerOptionEnum synchroniseOption)
		{
			Storage = new UIStorage<T>(pageSize, startPageCount);
			this.synchroniseOption = synchroniseOption;
			linearAllocator = new UILinearAllocator<T>(UILinearAllocator<T>.Constructor.Default);
			poolAllocator = new UIPoolAllocator<T>(UIPoolAllocator<T>.Constructor.Default);
			blockAllocator = new UIBlockAllocator<T>(8);
			loaded = true;
		}

		public void Unload()
		{
			if (!loaded)
			{
				return;
			}
			Storage.Dispose();
			linearAllocator.Dispose();
			poolAllocator.Dispose();
			if (computeBuffer != null)
			{
				if (!string.IsNullOrEmpty(globalShaderString.Name))
				{
					computeBuffer.ResetGlobalShaderState(globalShaderString);
				}
				computeBuffer.Release();
				computeBuffer = null;
			}
			loaded = false;
		}

		public void FrameBarrier()
		{
			linearAllocator.Reset();
		}

		public void SynchronizeComputeBuffer()
		{
			synchroniseDate++;
			int num = Math.Max(1, Storage.BufferSize);
			bool flag = false;
			if (computeBuffer == null || computeBuffer.Size < num)
			{
				if (computeBuffer != null && !string.IsNullOrEmpty(globalShaderString.Name))
				{
					computeBuffer.ResetGlobalShaderState(globalShaderString);
				}
				computeBuffer?.Release();
				computeBuffer = new ReadWriteBuffer1D<T>($"UIAtomContainer<{typeof(T)}>.ComputeBuffer", num, default(T), ReadWriteBuffer1DOption.MaterialParameters);
				computeBuffer.Create(apply: false);
				flag = true;
				if (!string.IsNullOrEmpty(globalShaderString.Name))
				{
					computeBuffer.SetGlobalShaderState(globalShaderString);
				}
			}
			if (Storage.Capacity == 0)
			{
				return;
			}
			int pageCapacity = Storage.PageCapacity;
			int pageCount = Storage.PageCount;
			if (synchroniseOption != 0)
			{
				int num2 = 0;
				for (int i = 0; i < pageCount; i++)
				{
					if (flag || Storage.Pages.Data[i].ComputeBufferDirty)
					{
						Storage.Pages.Data[i].LastSynchronisationDate = synchroniseDate;
						num2++;
					}
				}
				if (num2 > 0)
				{
					computeBuffer.SetData(Storage.Memory);
				}
				return;
			}
			for (int j = 0; j < pageCount; j++)
			{
				if (flag || Storage.Pages.Data[j].ComputeBufferDirty)
				{
					computeBuffer.SetData(Storage.Memory, j * pageCapacity, j * pageCapacity, pageCapacity);
					Storage.Pages.Data[j].LastSynchronisationDate = synchroniseDate;
					Storage.Pages.Data[j].ComputeBufferDirty = false;
				}
			}
		}

		public void AddToMaterialPropertyBlock(ShaderString propertyName, MaterialPropertyBlock materialPropertyBlock)
		{
			computeBuffer?.AddToMaterialPropertyBlock(propertyName, materialPropertyBlock);
		}

		public void AddToShaderGlobal(ShaderString propertyName)
		{
			globalShaderString = propertyName;
			if (computeBuffer != null)
			{
				computeBuffer?.SetGlobalShaderState(propertyName);
			}
		}

		[Conditional("ENABLE_PROFILER")]
		internal void ProfilerLog()
		{
		}
	}
}
