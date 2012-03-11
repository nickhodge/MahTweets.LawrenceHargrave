using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core
{
    /// <summary>
    /// Reporting point for exceptions and other application data. 
    /// 
    /// 
    /// Stores exception data on disk for a session. 
    ///
    /// </summary>
    public static class BlackBoxRecorder
    {
        private static readonly Func<string, string> cleanPath =
            input => input
                         .Replace(":", "")
                         .Replace("-", "")
                         .Replace("+", "")
                         .Replace(" ", "")
                         .Replace("[", "")
                         .Replace("]", "")
                         .Replace("<", "")
                         .Replace(">", "");

        internal static readonly Queue<ExceptionEntry> exceptionQueue = new Queue<ExceptionEntry>();
        internal static DateTime? lastFlush;
        internal static int exceptionIndex;
        internal static readonly object queueLock = new object();
        internal static bool flushing;
        internal static string sessionPath = string.Empty;


        /// <summary>
        /// Send Exception to Logging mechanism
        /// </summary>
        /// <param name="ex">Generated exception</param>
        // [Obsolete("Use MahTweets.Core.Composition.CompositionManager.Get<MahTweets.Core.Interfaces.Application.IExceptionReporter>().ReportHandledException(ex) instead", false)]
        public static void LogHandledException(Exception ex)
        {
            CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
        }

        public static void LegacyLogHandledException(Exception ex)
        {
            if (ex is OutOfMemoryException)
            {
                // Try flushing queue to disk immediately. 
                FlushQueue();
            }

            Logging.CallerInfo caller = Logging.CallerName();
            if (ex is ReflectionTypeLoadException)
            {
                var loaderException = ex as ReflectionTypeLoadException;
                foreach (Exception err in loaderException.LoaderExceptions)
                {
                    exceptionQueue.Enqueue(new ExceptionEntry {Caller = caller, Exception = err});
                }
            }
            else
            {
                exceptionQueue.Enqueue(new ExceptionEntry {Caller = caller, Exception = ex});
            }

            // Logging.Info("Exception queued for output. Queue size: {0}", exceptionQueue.Count);
            if (exceptionQueue.Count > 5)
                FlushQueue();
            else if (exceptionQueue.Count > 1 && !lastFlush.HasValue)
                FlushQueue();
            else if (lastFlush != null && lastFlush.HasValue &&
                     DateTime.UtcNow.Subtract(lastFlush.Value).TotalSeconds > 90)
                FlushQueue();
        }

        /// <summary>
        /// Send Scripting Exception to Logging mechanism
        /// </summary>
        /// <param name="ex">Generated exception</param>
        /// <param name="scriptpath">script file path</param>
        public static void LogHandledException(Exception ex, string scriptpath)
        {
            if (ex is OutOfMemoryException)
            {
                // Try flushing queue to disk immediately. 
                FlushQueue();
            }

            Logging.CallerInfo caller = Logging.CallerName();
            if (ex is ReflectionTypeLoadException)
            {
                var loaderException = ex as ReflectionTypeLoadException;
                foreach (Exception err in loaderException.LoaderExceptions)
                {
                    exceptionQueue.Enqueue(new ExceptionEntry {Caller = caller, Exception = err});
                }
            }
            else
            {
                exceptionQueue.Enqueue(new ScriptExceptionEntry
                                           {Caller = caller, Exception = ex, ScriptName = scriptpath});
            }

            // Logging.Info("Exception queued for output. Queue size: {0}", exceptionQueue.Count);
            if (exceptionQueue.Count > 5)
                FlushQueue();
            else if (exceptionQueue.Count > 1 && !lastFlush.HasValue)
                FlushQueue();
            else if (lastFlush != null && lastFlush.HasValue &&
                     DateTime.UtcNow.Subtract(lastFlush.Value).TotalSeconds > 90)
                FlushQueue();
        }

        public static void FlushQueue()
        {
            if (flushing)
            {
                return;
            }
            ThreadPool.QueueUserWorkItem(o => FlushQueueImmediate());
        }

        private static void FlushQueueImmediate()
        {
            lock (queueLock)
            {
                // Logging.Info("Flushing Exception Queue");
                flushing = true;
                while (exceptionQueue.Count > 0)
                {
                    exceptionIndex++;
                    ExceptionEntry e = exceptionQueue.Dequeue();
                    if (e == null)
                        continue;
                    if (Directory.Exists(sessionPath) == false)
                        // moved here -- only create the blackboxrecording dir if required
                    {
                        Directory.CreateDirectory(sessionPath);
                    }
                    string exceptionFilename = string.Format("{0}-{1}-{2}.xml", exceptionIndex,
                                                             e.Caller.FullMethodNamespace,
                                                             (e.Exception != null)
                                                                 ? e.Exception.GetType().Name
                                                                 : "(null)");
                    // Logging.Info("Writing Exception Data as {0}", Path.Combine(sessionPath, cleanPath(exceptionFilename)));
                    var knownTypes = new List<Type>();
                    knownTypes.Add(typeof (ExceptionEntry));
                    if (e.Exception != null)
                    {
                        //knownTypes.Add(e.Exception.GetType());

                        ProcessExceptionForAdditionalData(e, ref knownTypes);
                    }


                    string content = Serialization.Serialize(e, Serialization.SerializerType.Xml, knownTypes);
                    File.WriteAllText(Path.Combine(sessionPath, cleanPath(exceptionFilename)), content);
                }
                lastFlush = DateTime.UtcNow;
                flushing = false;
                // Logging.Info("Exception Queue Emptied");
            }
        }

        private static void ProcessExceptionForAdditionalData(ExceptionEntry e, ref List<Type> knownTypes)
        {
            // TODO (currently empty, here for future use) 
        }


        public static void Init()
        {
            string sessionID = cleanPath(DateTime.UtcNow.ToString("o")) + "-" + Process.GetCurrentProcess().Id;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string AssemblyPath = Path.GetDirectoryName(assembly.Location);

            // Init the exception logging. 
            sessionPath = AssemblyPath + Path.DirectorySeparatorChar + "BlackBoxRecorder" + Path.DirectorySeparatorChar +
                          sessionID;
        }

        public static void ApplicationShuttingDown()
        {
            // exceptionQueue.Enqueue(new ExceptionEntry { Caller = new Logging.CallerInfo { FullMethodNamespace = "EOF", FullDetail = "EOF" } });
            FlushQueueImmediate();
        }
    }

    [DataContract]
    internal class ExceptionEntry
    {
        private Exception _exception;

        public Exception Exception
        {
            get { return _exception; }
            set
            {
                _exception = value;
                ExceptionString = value.ToString();
            }
        }

        [DataMember]
        public string ExceptionString { get; set; }

        [DataMember]
        public object AdditionalExceptionData { get; set; }

        public Logging.CallerInfo Caller { get; set; }
    }

    [DataContract]
    internal class ScriptExceptionEntry : ExceptionEntry
    {
        [DataMember]
        public string ScriptName { get; set; }
    }
}