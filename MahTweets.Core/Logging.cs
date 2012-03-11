using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace MahTweets.Core
{
    public static class Logging
    {
        private static readonly object ConsoleLockObject = new object();
        public static bool EnableConsoleOutput;

        public static void Info(string logMessage, params object[] args)
        {
            lock (ConsoleLockObject)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Output(DateTime.Now.Ticks + " INFO " + CallerName());

                Console.ForegroundColor = ConsoleColor.White;
                Output("\t" + string.Format(logMessage, args));

                Console.ForegroundColor = ConsoleColor.Blue;
            }
        }

        public static void Info(string Log)
        {
            lock (ConsoleLockObject)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Output(DateTime.Now.Ticks + " INFO " + CallerName());

                Console.ForegroundColor = ConsoleColor.White;
                Output("\t" + Log);

                Console.ForegroundColor = ConsoleColor.Blue;
            }
        }


        public static void Important(string LogMessage, params object[] args)
        {
            lock (ConsoleLockObject)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Output(DateTime.Now.Ticks + " IMPORTANT " + CallerName());

                Console.ForegroundColor = ConsoleColor.Yellow;
                Output("\t" + string.Format(LogMessage, args));

                Console.ForegroundColor = ConsoleColor.Blue;
            }
        }

        public static void Important(string Log)
        {
            lock (ConsoleLockObject)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Output(DateTime.Now.Ticks + " IMPORTANT " + CallerName());

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Output("\t" + Log);

                Console.ForegroundColor = ConsoleColor.Blue;
            }
        }

        private static void Output(string output)
        {
            Debug.WriteLine(output);
            if (EnableConsoleOutput)
            {
                Console.WriteLine(output);
            }
        }


        public static void Fail(string LogMessage, params object[] args)
        {
            lock (ConsoleLockObject)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Output(DateTime.Now.Ticks + " ERROR " + CallerName());

                Console.ForegroundColor = ConsoleColor.Red;
                Output("\t" + string.Format(LogMessage, args));
                Console.ForegroundColor = ConsoleColor.Blue;
            }
        }


        public static void Fail(string Log)
        {
            lock (ConsoleLockObject)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Output(DateTime.Now.Ticks + " ERROR " + CallerName());

                Console.ForegroundColor = ConsoleColor.Red;
                Output("\t" + Log);
                Console.ForegroundColor = ConsoleColor.Blue;
            }
        }

        public static void Fail(string LogMessage, Exception ex, params object[] args)
        {
            lock (ConsoleLockObject)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Output(DateTime.Now.Ticks + " ERROR " + CallerName());

                Console.ForegroundColor = ConsoleColor.Red;
                Output("\t" + string.Format(LogMessage, args));

                Console.ForegroundColor = ConsoleColor.Yellow;
                Output("\t" + ex);


                Console.ForegroundColor = ConsoleColor.Blue;
            }
        }

        public static void Fail(string Log, Exception ex)
        {
            lock (ConsoleLockObject)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Output(DateTime.Now.Ticks + " ERROR " + CallerName());

                Console.ForegroundColor = ConsoleColor.Red;
                Output("\t" + Log);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Output("\t" + ex);

                Console.ForegroundColor = ConsoleColor.Blue;
            }
        }

        /// <summary>
        /// Return the Namespace, Class, and Method of the calling method. 
        /// </summary>
        /// <returns></returns>
        public static CallerInfo CallerName()
        {
            return CallerName(3);
        }

        /// <summary>
        /// Return the Namespace, Class, and Method of the calling method. 
        /// </summary>
        /// <param name="skipFrames">The number of frames to skip up the stack.</param>
        /// <returns></returns>
        public static CallerInfo CallerName(int skipFrames)
        {
            string output = string.Empty;
#if DEBUG 
            StackFrame callerFrame = new StackFrame(skipFrames, true);
            MethodBase executingMethod = callerFrame.GetMethod();
            string filename = callerFrame.GetFileName();
            if (string.IsNullOrEmpty(filename))
            {
                filename = "(unknown)";
            }
            else
            {
                filename = filename.Substring(filename.LastIndexOf(@"\") + 1);
            }
            output = string.Format("{0}.{1}.{2}[{3}, Line: {4} Column {5}]", executingMethod.ReflectedType.Namespace, executingMethod.ReflectedType.Name, executingMethod.Name, filename, callerFrame.GetFileLineNumber(), callerFrame.GetFileColumnNumber());
#else

            var callerFrame = new StackFrame(skipFrames, false);
            MethodBase executingMethod = callerFrame.GetMethod();
            output = string.Format("{0}.{1}.{2}[+{3}]", executingMethod.ReflectedType.Namespace,
                                   executingMethod.ReflectedType.Name, executingMethod.Name,
                                   callerFrame.GetNativeOffset());
#endif

            output += string.Format(" Thread {0}", Thread.CurrentThread.ManagedThreadId);

            var caller = new CallerInfo
                             {
                                 FullMethodNamespace =
                                     executingMethod.ReflectedType.Namespace + "." +
                                     executingMethod.ReflectedType.Name + "." + executingMethod.Name,
                                 FullDetail = output
                             };
            return caller;
        }

        #region Nested type: CallerInfo

        [DataContract]
        public class CallerInfo
        {
            [DataMember]
            public string FullMethodNamespace { get; set; }

            [DataMember]
            public string FullDetail { get; set; }

            public override string ToString()
            {
                return FullDetail;
            }
        }

        #endregion
    }
}