using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OneTon.Logging
{
    public enum LogLevel
    {
        Info = 0,
        Debug = 1,
        Warn = 2,
        Error = 3,
        Silent = 4
    }

    public class LogService
    {
        private static LogLevel globalMinimumLogLevel = LogLevel.Info;
        private static readonly Dictionary<Type, LogLevel> perClassLogLevels = new();

        private readonly Type contextType;

        private LogService(Type type, LogLevel? initialLevel = null)
        {
            contextType = type;
            if (initialLevel.HasValue)
            {
                perClassLogLevels[type] = initialLevel.Value;
            }
        }

        public static LogService Get<T>(LogLevel? initialLevel = null) => new(typeof(T));
        public static LogService GetStatic(Type type, LogLevel? initialLevel = null) => new(type);

        public static void SetLogLevelFor<T>(LogLevel level)
        {
            perClassLogLevels[typeof(T)] = level;
        }

        public static void SetGlobalLogLevel(LogLevel level)
        {
            globalMinimumLogLevel = level;
        }

        private bool ShouldLog(LogLevel level)
        {
            if (perClassLogLevels.TryGetValue(contextType, out var specificLevel))
                return level >= specificLevel;

            return level >= globalMinimumLogLevel;
        }

        private void Log(LogLevel level, string message, string methodName)
        {
            if (!ShouldLog(level)) return;

            string prefix = $"[{level}] {contextType.Name}.{methodName}(): ";
            string fullMessage = prefix + message;

            switch (level)
            {
                case LogLevel.Info:
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(fullMessage);
                    break;
                case LogLevel.Warn:
                    UnityEngine.Debug.LogWarning(fullMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(fullMessage);
                    break;
            }
        }

        public void Info(string message, [CallerMemberName] string caller = "")
            => Log(LogLevel.Info, message, caller);

        public void Debug(string message, [CallerMemberName] string caller = "")
            => Log(LogLevel.Debug, message, caller);

        public void Warn(string message, [CallerMemberName] string caller = "")
            => Log(LogLevel.Warn, message, caller);

        public void Error(string message, [CallerMemberName] string caller = "")
            => Log(LogLevel.Error, message, caller);

        public void Trace([CallerMemberName] string caller = "")
            => Info("Trace", caller);
    }
}