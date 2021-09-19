using System;
using System.Collections.Generic;
using System.Diagnostics;
using Amplitude.Framework.Profiling;
using UnityEngine;

namespace Amplitude.UI
{
	internal static class UIStatistics
	{
		private struct InstanceInfo
		{
			public static AllocFreeSorter.CompareRef<InstanceInfo> Compare = CompareMethod;

			public Type Type;

			public string ProfilePath;

			public int Count;

			private static int CompareMethod(ref InstanceInfo lhs, ref InstanceInfo rhs)
			{
				if (lhs.Count == rhs.Count)
				{
					return 0;
				}
				if (rhs.Count >= lhs.Count)
				{
					return 1;
				}
				return -1;
			}
		}

		private static readonly Dictionary<Type, int> TypeToInstanceCounts;

		private static readonly Dictionary<Type, string> TypeToProfilerPath;

		private static readonly Type TypeOfUIBehaviour;

		private static readonly List<InstanceInfo> SortedInstanceInfo;

		[Conditional("ENABLE_PROFILER")]
		public static void OnInstanceCreated(UIBehaviour instance)
		{
			Type type = instance.GetType();
			while (type != TypeOfUIBehaviour)
			{
				if (!TypeToInstanceCounts.ContainsKey(type))
				{
					TypeToInstanceCounts.Add(type, 1);
					TypeToProfilerPath.Add(type, $"UI|Objects|{type.Name}");
				}
				else
				{
					TypeToInstanceCounts[type]++;
				}
				type = type.BaseType;
			}
			TypeToInstanceCounts[TypeOfUIBehaviour]++;
		}

		[Conditional("ENABLE_PROFILER")]
		public static void OnInstanceDestroyed(UIBehaviour instance)
		{
			Type type = instance.GetType();
			while (type != TypeOfUIBehaviour)
			{
				if (TypeToInstanceCounts.ContainsKey(type))
				{
					TypeToInstanceCounts[type]--;
				}
				type = type.BaseType;
			}
			TypeToInstanceCounts[TypeOfUIBehaviour]--;
		}

		[Conditional("ENABLE_PROFILER")]
		public static void ProfilerLog()
		{
			SortedInstanceInfo.Clear();
			foreach (KeyValuePair<Type, int> typeToInstanceCount in TypeToInstanceCounts)
			{
				string profilePath = TypeToProfilerPath[typeToInstanceCount.Key];
				SortedInstanceInfo.Add(new InstanceInfo
				{
					Count = typeToInstanceCount.Value,
					Type = typeToInstanceCount.Key,
					ProfilePath = profilePath
				});
			}
			AllocFreeSorter.Sort(SortedInstanceInfo, InstanceInfo.Compare);
			int num = Mathf.Min(10, SortedInstanceInfo.Count);
			for (int i = 0; i < num; i++)
			{
				_ = SortedInstanceInfo[i];
			}
			if (ProfilerDataRecorder.TryGetRecorder("UI|Rendering|Primitives|Total", out var recorder) && ProfilerDataRecorder.TryGetRecorder("UI|Update", out var recorder2))
			{
				float num2 = recorder2.FrameValue(recorder2.FrameCount - 1);
				_ = recorder.FrameValue(recorder.FrameCount - 1) / num2;
			}
		}
	}
}
