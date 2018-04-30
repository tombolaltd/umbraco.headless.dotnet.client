using System;

namespace Umbraco.Caching
{
    public class CacheMissEventArgs : EventArgs
    {
        private string attemptedKey;
        private string reason;

        public string AttemptedKey => $"{attemptedKey}";
        public string Reason => $"{reason}";

        internal CacheMissEventArgs(string attemptedKey, string reason)
        {
            this.attemptedKey = attemptedKey;
            this.reason = reason;
        }
    }
}
