using System;
using System.Diagnostics;
using Amplitude.Graphics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Amplitude.UI
{
	public static class UIAtomRepository
	{
		private interface IUIAtomContainerAccessHelper
		{
			void Load(int pageSize, int startPageCount, SynchroniseUIAtomContainerOptionEnum synchroniseOption);

			void Unload();

			void FrameBarrier();

			void ProfilerLog();
		}

		private static class ProfilerNames
		{
			public static readonly string Prefix = "UI|Atom|Timings|";

			public static readonly string AllocatePersistant = Prefix + "AllocatePersistant";

			public static readonly string AllocateTemporary = Prefix + "AllocateTemporary";

			public static readonly string SetData = Prefix + "SetData";
		}

		private class UIAtomContainerAccessHelper<T> : IUIAtomContainerAccessHelper where T : struct
		{
			public void Load(int pageSize, int startPageCount, SynchroniseUIAtomContainerOptionEnum synchroniseOption)
			{
				UIAtomContainer<T>.Global.Load(pageSize, startPageCount, synchroniseOption);
			}

			public void FrameBarrier()
			{
				UIAtomContainer<T>.Global.FrameBarrier();
			}

			public void ProfilerLog()
			{
			}

			public void Unload()
			{
				UIAtomContainer<T>.Global.Unload();
			}
		}

		private static PerformanceList<IUIAtomContainerAccessHelper> declaredContainers;

		public static void Declare<T>(int pageSize, int startPageCount, SynchroniseUIAtomContainerOptionEnum synchroniseOption) where T : struct
		{
			if (!UIAtomContainer<T>.Global.Loaded)
			{
				UIAtomContainer<T>.Global.Load(pageSize, startPageCount, synchroniseOption);
				declaredContainers.Add(new UIAtomContainerAccessHelper<T>());
			}
		}

		[Obsolete("Use UIAtomContainer<T>.Allocate()")]
		public static UIAtomId Allocate<T>() where T : struct
		{
			return UIAtomContainer<T>.Allocate(UIAllocationType.Persistant);
		}

		[Obsolete("Use UIAtomContainer<T>.Allocate(int size)")]
		public static UIAtomId Allocate<T>(int size) where T : struct
		{
			return UIAtomContainer<T>.Allocate(size, UIAllocationType.Persistant);
		}

		[Obsolete("Use UIAtomContainer<T>.AllocateTemporary()")]
		public static UIAtomId AllocateTemporary<T>() where T : struct
		{
			return UIAtomContainer<T>.Allocate(UIAllocationType.Frame);
		}

		[Obsolete("Use UIAtomContainer<T>.AllocateTemporary(int size)")]
		public static UIAtomId AllocateTemporary<T>(int size) where T : struct
		{
			return UIAtomContainer<T>.Allocate(size, UIAllocationType.Frame);
		}

		[Obsolete("Use UIAtomContainer<T>.Deallocate()")]
		public static void Deallocate<T>(ref UIAtomId id) where T : struct
		{
			UIAtomContainer<T>.Deallocate(ref id);
		}

		[Obsolete("Use UIAtomContainer<T>.GetData(UIAtomId id)")]
		public static T GetData<T>(UIAtomId id) where T : struct
		{
			return UIAtomContainer<T>.GetData(id);
		}

		[Obsolete("Use UIAtomContainer<T>.GetData(UIAtomId id, int offset)")]
		public static T GetData<T>(UIAtomId id, int offset) where T : struct
		{
			return UIAtomContainer<T>.GetData(id, offset);
		}

		[Obsolete("Use UIAtomContainer<T>.SetData(UIAtomId id, ref T data)")]
		public static void SetData<T>(UIAtomId id, ref T data) where T : struct
		{
			UIAtomContainer<T>.SetData(id, ref data);
		}

		[Obsolete("Use UIAtomContainer<T>.SetData(UIAtomId id, int offset, ref T data)")]
		public static void SetData<T>(UIAtomId id, int offset, ref T data) where T : struct
		{
			UIAtomContainer<T>.SetData(id, offset, ref data);
		}

		[Obsolete("Use UIAtomContainer<T>.SetData(UIAtomId id, T[] data)")]
		public static void SetData<T>(UIAtomId id, T[] data) where T : struct
		{
			UIAtomContainer<T>.SetData(id, data);
		}

		[Obsolete("Use UIAtomContainer<T>.SynchronizeComputeBuffer()")]
		public static void SynchronizeComputeBuffer<T>() where T : struct
		{
			UIAtomContainer<T>.Global.SynchronizeComputeBuffer();
		}

		[Obsolete("Use UIAtomContainer<T>.AddToMaterialPropertyBlock(Graphics.ShaderString propertyId, UnityEngine.MaterialPropertyBlock materialPropertyBlock)")]
		public static void AddToMaterialPropertyBlock<T>(ShaderString propertyId, MaterialPropertyBlock materialPropertyBlock) where T : struct
		{
			UIAtomContainer<T>.Global.AddToMaterialPropertyBlock(propertyId, materialPropertyBlock);
		}

		[Obsolete("Use UIAtomContainer<T>.AddToShaderGlobal(Graphics.ShaderString propertyId)")]
		public static void AddToShaderGlobal<T>(ShaderString propertyId) where T : struct
		{
			UIAtomContainer<T>.Global.AddToShaderGlobal(propertyId);
		}

		internal static void Release()
		{
			int count = declaredContainers.Count;
			for (int i = 0; i < count; i++)
			{
				declaredContainers.Data[i].Unload();
			}
			declaredContainers.ClearReleaseMemory();
		}

		internal static void FrameBarrier()
		{
			int count = declaredContainers.Count;
			for (int i = 0; i < count; i++)
			{
				declaredContainers.Data[i].FrameBarrier();
			}
		}

		[Conditional("ENABLE_PROFILER")]
		internal static void ProfilerLog()
		{
			int count = declaredContainers.Count;
			for (int i = 0; i < count; i++)
			{
				declaredContainers.Data[i].ProfilerLog();
			}
		}

		[Conditional("ASSERT")]
		private static void AssertAtomContainerCreated<T>() where T : struct
		{
			if (!UIAtomContainer<T>.Global.Loaded)
			{
				Diagnostics.LogError("The UIAtomContainerAccessHelper for type {0} is lazy declared", typeof(T));
				int pageSize = 1024;
				SynchroniseUIAtomContainerOptionEnum synchroniseOption = ((SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan) ? SynchroniseUIAtomContainerOptionEnum.NoPartialUpdate : SynchroniseUIAtomContainerOptionEnum.Default);
				UIAtomContainer<T>.Global.Load(pageSize, 3, synchroniseOption);
				declaredContainers.Add(new UIAtomContainerAccessHelper<T>());
			}
		}
	}
}
