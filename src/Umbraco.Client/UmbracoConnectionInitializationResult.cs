using System;

namespace Umbraco.Client
{
    /// <summary>
    /// A connection initialisation result, used to pull a part the results of a connection initialisation attempt.
    /// Rather than throwing exceptions.
    /// </summary>
    public class UmbracoConnectionInitializationResult
    {
        internal UmbracoConnectionInitializationResult(bool success)
        {
            Success = success;
        }

        /// <summary>
        /// Was the initialisation attempt a success?
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// If the initialisation attempt was not successful, this message will represent why not.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// If the initialisation attempt was not successful, this represents any inner exception.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
