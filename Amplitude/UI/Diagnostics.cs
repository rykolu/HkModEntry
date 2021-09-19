using System;
using System.Diagnostics;
using System.Text;

namespace Amplitude.UI
{
	public static class Diagnostics
	{
		public static class Debug
		{
			[Conditional("ENABLE_LOG")]
			public static void Log(string message)
			{
				Amplitude.Diagnostics.LogError("Incoherent ENABLE_LOG Define Usage. Framework doesn't define it, but the client do.");
			}

			[Conditional("ENABLE_LOG")]
			public static void Log(string format, params object[] args)
			{
				Amplitude.Diagnostics.LogError("Incoherent ENABLE_LOG Define Usage. Framework doesn't define it, but the client do.");
			}

			[Conditional("ENABLE_LOG")]
			public static void Log(ulong flags, string message)
			{
				Amplitude.Diagnostics.LogError("Incoherent ENABLE_LOG Define Usage. Framework doesn't define it, but the client do.");
			}

			[Conditional("ENABLE_LOG")]
			public static void Log(ulong flags, string format, params object[] args)
			{
				Amplitude.Diagnostics.LogError("Incoherent ENABLE_LOG Define Usage. Framework doesn't define it, but the client do.");
			}
		}

		internal static string LogPrefix = "[UI] ";

		private static StringBuilder dirtificationMessage = new StringBuilder();

		[Conditional("ASSERT")]
		public static void Assert(bool condition)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(ulong flags, bool condition)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(bool condition, string message)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(bool condition, string format, object arg0)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(bool condition, string format, params object[] args)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(ulong flags, bool condition, string message)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(ulong flags, bool condition, string format, params object[] args)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(string expression)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(ulong flags, string expression)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(string expression, string message)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(string expression, string format, params object[] args)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(ulong flags, string expression, string message)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(ulong flags, string expression, string format, params object[] args)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(object reference)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(ulong flags, object reference)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(object reference, string message)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(object reference, string format, params object[] args)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(ulong flags, object reference, string message)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		[Conditional("ASSERT")]
		public static void Assert(ulong flags, object reference, string format, params object[] args)
		{
			Amplitude.Diagnostics.LogError("Incoherent ASSERT Define Usage. Framework doesn't define it, but the client do.");
		}

		public static void Log(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentNullException("message", "The 'message' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Information, 50uL, LogPrefix + message, stackTrace.GetFrames()));
		}

		public static void Log(string format, params object[] args)
		{
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentNullException("format", "The 'format' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Information, 50uL, string.Format(LogPrefix + format, args), stackTrace.GetFrames()));
		}

		public static void Log(ulong flags, string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentNullException("message", "The 'message' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Information, flags, LogPrefix + message, stackTrace.GetFrames()));
		}

		public static void Log(ulong flags, string format, params object[] args)
		{
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentNullException("format", "The 'format' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Information, flags, string.Format(LogPrefix + format, args), stackTrace.GetFrames()));
		}

		public static void LogError(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentNullException("message", "The 'message' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Error, 50uL, LogPrefix + message, stackTrace.GetFrames()));
		}

		public static void LogError(string format, params object[] args)
		{
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentNullException("format", "The 'format' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Error, 50uL, string.Format(LogPrefix + format, args), stackTrace.GetFrames()));
		}

		public static void LogError(ulong flags, string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentNullException("message", "The 'message' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Error, flags, LogPrefix + message, stackTrace.GetFrames()));
		}

		public static void LogError(ulong flags, string format, params object[] args)
		{
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentNullException("format", "The 'format' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Error, flags, string.Format(LogPrefix + format, args), stackTrace.GetFrames()));
		}

		public static void LogException(Exception exception, int innerExceptionLevel = 0)
		{
			if (exception == null)
			{
				throw new ArgumentNullException("exception", "The 'exception' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(exception, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Critical, 50uL, exception.Message, stackTrace.GetFrames()));
			if (exception.InnerException != null)
			{
				Amplitude.Diagnostics.LogException(exception.InnerException, innerExceptionLevel + 1);
			}
		}

		public static void LogException(ulong flags, Exception exception, int innerExceptionLevel = 0)
		{
			if (exception == null)
			{
				throw new ArgumentNullException("exception", "The 'exception' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(exception, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Critical, flags, exception.Message, stackTrace.GetFrames()));
			if (exception.InnerException != null)
			{
				Amplitude.Diagnostics.LogException(exception.InnerException, innerExceptionLevel + 1);
			}
		}

		public static void LogWarning(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentNullException("message", "The 'message' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Warning, 50uL, LogPrefix + message, stackTrace.GetFrames()));
		}

		public static void LogWarning(string format, params object[] args)
		{
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentNullException("format", "The 'format' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Warning, 50uL, string.Format(LogPrefix + format, args), stackTrace.GetFrames()));
		}

		public static void LogWarning(ulong flags, string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentNullException("message", "The 'message' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Warning, flags, LogPrefix + message, stackTrace.GetFrames()));
		}

		public static void LogWarning(ulong flags, string format, params object[] args)
		{
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentNullException("format", "The 'format' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Warning, flags, string.Format(LogPrefix + format, args), stackTrace.GetFrames()));
		}

		[Conditional("ENABLE_TRACE")]
		public static void Trace(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentNullException("message", "The 'message' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Trace, 50uL, LogPrefix + message, stackTrace.GetFrames()));
		}

		[Conditional("ENABLE_TRACE")]
		public static void Trace(string format, params object[] args)
		{
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentNullException("format", "The 'format' argument cannot be null.");
			}
			StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
			Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Trace, 50uL, string.Format(LogPrefix + format, args), stackTrace.GetFrames()));
		}

		[Conditional("ENABLE_TRACE")]
		public static void Trace(ulong flags, string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentNullException("message", "The 'message' argument cannot be null.");
			}
			if (CanLogWithVerbosity(flags))
			{
				if (IsSubCategoryOfUI(flags))
				{
					flags = 50uL;
				}
				StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
				Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Trace, flags, LogPrefix + message, stackTrace.GetFrames()));
			}
		}

		[Conditional("ENABLE_TRACE")]
		public static void Trace(ulong flags, string format, params object[] args)
		{
			if (string.IsNullOrEmpty(format))
			{
				throw new ArgumentNullException("format", "The 'format' argument cannot be null.");
			}
			if (CanLogWithVerbosity(flags))
			{
				if (IsSubCategoryOfUI(flags))
				{
					flags = 50uL;
				}
				StackTrace stackTrace = new StackTrace(1, fNeedFileInfo: true);
				Amplitude.Diagnostics.OnMessageLogged(new LogMessage(LogLevel.Trace, flags, string.Format(LogPrefix + format, args), stackTrace.GetFrames()));
			}
		}

		private static bool CanLogWithVerbosity(ulong flags)
		{
			return false;
		}

		private static bool IsSubCategoryOfUI(ulong flags)
		{
			if (flags >= 50)
			{
				return flags < 59;
			}
			return false;
		}
	}
}
