using System.Collections;

namespace Amplitude.UI
{
	public class UIBehaviourAsynchronousLoader
	{
		public interface ITask
		{
			IEnumerator Run();
		}

		private class Scope
		{
			private PerformanceList<UIBehaviour> behaviours;

			public void Push(UIBehaviour behaviour)
			{
				behaviours.Add(behaviour);
			}

			public IEnumerator Flush()
			{
				Stopwatch stopWatch = default(Stopwatch);
				stopWatch.Start();
				int behaviourCount = behaviours.Count;
				int i = 0;
				while (i < behaviourCount)
				{
					UIBehaviour uIBehaviour = behaviours.Data[i];
					if (uIBehaviour != null)
					{
						string text = uIBehaviour.ToString();
						double elapsedMilliseconds = stopWatch.ElapsedMilliseconds;
						if (uIBehaviour.enabled && uIBehaviour.gameObject.activeInHierarchy)
						{
							uIBehaviour.LoadIfNecessary();
						}
						if (stopWatch.ElapsedMilliseconds >= (double)currentBudget)
						{
							if (LongYieldWarning && stopWatch.ElapsedMilliseconds > (double)(currentBudget + lowPriorityBudget))
							{
								Diagnostics.LogWarning("Yield async loading for {0} ms - last behaviour is {1} with {2} ms", stopWatch.ElapsedMilliseconds, text, stopWatch.ElapsedMilliseconds - elapsedMilliseconds);
							}
							yield return null;
							stopWatch.Start();
						}
					}
					int num = i + 1;
					i = num;
				}
				behaviours.Clear();
			}
		}

		private static Coroutine currentCoroutine;

		private static bool useAsyncLoading;

		private static float lowPriorityBudget;

		private static float highPriorityBudget;

		private static float currentBudget;

		private static bool longYieldWarning;

		private static PerformanceList<Scope> stack;

		public static bool UseAsyncLoading
		{
			get
			{
				return useAsyncLoading;
			}
			set
			{
				if (useAsyncLoading != value)
				{
					useAsyncLoading = value;
				}
			}
		}

		public static float LowPriorityBudget
		{
			get
			{
				return lowPriorityBudget;
			}
			internal set
			{
				if (lowPriorityBudget != value)
				{
					lowPriorityBudget = value;
				}
			}
		}

		public static float HighPriorityBudget
		{
			get
			{
				return highPriorityBudget;
			}
			internal set
			{
				if (highPriorityBudget != value)
				{
					highPriorityBudget = value;
				}
			}
		}

		public static float CurrentBudget => currentBudget;

		public static bool LongYieldWarning
		{
			get
			{
				return longYieldWarning;
			}
			internal set
			{
				if (longYieldWarning != value)
				{
					longYieldWarning = value;
				}
			}
		}

		internal static bool Active
		{
			get
			{
				if (stack.Count != 0)
				{
					return currentCoroutine != null;
				}
				return false;
			}
		}

		static UIBehaviourAsynchronousLoader()
		{
			currentCoroutine = null;
			useAsyncLoading = true;
			lowPriorityBudget = 5f;
			highPriorityBudget = 20f;
			longYieldWarning = false;
			stack = default(PerformanceList<Scope>);
			currentBudget = LowPriorityBudget;
		}

		public static IEnumerator StartAsyncLoading<T>(T task) where T : ITask
		{
			if (!UseAsyncLoading)
			{
				yield return task.Run();
				yield break;
			}
			if (currentCoroutine != null)
			{
				yield return Open();
				yield return task.Run();
				yield return WaitForCompletion();
				yield break;
			}
			Coroutine asyncLoadingCoroutine = Coroutine.StartCoroutine(((ITask)task).Run);
			yield return Open();
			while (!asyncLoadingCoroutine.IsFinished)
			{
				currentCoroutine = asyncLoadingCoroutine;
				asyncLoadingCoroutine.Run();
				currentCoroutine = null;
				yield return null;
			}
			yield return WaitForCompletion();
		}

		public static void SetHighPriority(bool highPriority)
		{
			currentBudget = (highPriority ? HighPriorityBudget : LowPriorityBudget);
		}

		internal static void Push(UIBehaviour behaviour)
		{
			stack.Data[stack.Count - 1].Push(behaviour);
		}

		private static IEnumerator Open()
		{
			if (stack.Count != 0)
			{
				yield return stack.Data[stack.Count - 1].Flush();
			}
			stack.Add(new Scope());
		}

		private static IEnumerator WaitForCompletion()
		{
			Scope scope = stack.Data[stack.Count - 1];
			stack.RemoveBack();
			yield return scope.Flush();
		}

		private static void CoroutineExceptionHandler(object sender, CoroutineExceptionEventArgs e)
		{
			Diagnostics.LogException(e.Exception);
		}
	}
}
