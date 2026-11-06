using System;

namespace XForma
{
    /// <summary>
    /// How much a message from the framework matters.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Worth knowing when looking into something.
        /// </summary>
        Information,

        /// <summary>
        /// Something failed, and the app carries on without it, such as an
        /// image that didn't load.
        /// </summary>
        Warning,

        /// <summary>
        /// An exception reached the framework.
        /// </summary>
        Error,
    }

    /// <summary>
    /// A message from the framework.
    /// </summary>
    public sealed class LogEventArgs : EventArgs
    {
        internal LogEventArgs(
            LogLevel level,
            string message,
            Exception exception)
        {
            this.Level = level;
            this.Message = message;
            this.Exception = exception;
        }

        public LogLevel Level
        {
            get;
        }

        public string Message
        {
            get;
        }

        /// <summary>
        /// The exception behind the message, or null.
        /// </summary>
        public Exception Exception
        {
            get;
        }
    }

    /// <summary>
    /// What the framework has to say about a running app, for the app's own
    /// logging. Without a listener, warnings and errors go to the console.
    /// </summary>
    public static class FrameworkLog
    {
        /// <summary>
        /// Raised for each message, on the thread that wrote it.
        /// </summary>
        public static event EventHandler<LogEventArgs> MessageLogged;

        /// <summary>
        /// Writes a message to the framework's log. Control libraries and
        /// platforms use it; it never throws.
        /// </summary>
        public static void Write(
            LogLevel level,
            string message,
            Exception exception = null)
        {
            var handler = MessageLogged;
            if (null != handler)
            {
                var args = new LogEventArgs(level, message, exception);
                try
                {
                    handler(null, args);
                    return;
                }
                catch (Exception listenerException)
                {
                    // A broken listener mustn't lose the message, or add a failure
                    WriteToConsole(LogLevel.Error, "A FrameworkLog listener failed.", listenerException);
                }
            }

            if (LogLevel.Information != level)
            {
                WriteToConsole(level, message, exception);
            }
        }

        private static void WriteToConsole(
            LogLevel level,
            string message,
            Exception exception)
        {
            try
            {
                Console.Error.WriteLine((null != exception) ? $"XForma {level}: {message} {exception}" : $"XForma {level}: {message}");
            }
            catch (Exception)
            {
                // Nowhere left to report it
            }
        }
    }

    /// <summary>
    /// An exception that nothing in the app caught.
    /// </summary>
    public sealed class ApplicationExceptionEventArgs : EventArgs
    {
        internal ApplicationExceptionEventArgs(
            Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception
        {
            get;
        }

        /// <summary>
        /// Keeps the app running where the platform allows it (see
        /// <see cref="Application.UnhandledException"/>).
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }
    }
}
