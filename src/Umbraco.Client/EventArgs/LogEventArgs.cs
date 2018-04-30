using System;

namespace Umbraco.Client.EventArgs
{
    /// <summary>
    /// A base log event arguments which have data relating to a basic log. 
    /// Such as a message and any misc data.
    /// </summary>
    public class LogEventArgs : System.EventArgs
    {
        public string Message { get; set; }
        public object Data { get; set; }
    }

    /// <summary>
    /// Represents a success log event arguments
    /// </summary>
    public class SuccessLogEventArgs : LogEventArgs
    {
    }

    /// <summary>
    /// Represents an information log event arguments
    /// </summary>
    public class InfoLogEventArgs : LogEventArgs
    {
    }

    /// <summary>
    /// Represents an warning log event arguments
    /// </summary>
    public class WarningLogEventArgs : LogEventArgs
    {
    }

    /// <summary>
    /// Represents a failure log event arguments
    /// </summary>
    public class FailureLogEventArgs : LogEventArgs
    {
        public Exception Exception { get; set; }
    }
}
