using System;

namespace Umbraco.Client
{
    /// <summary>
    /// Provides static helpers to a umbraco connections.
    /// </summary>
    public static class UmbracoConnectionHolder
    {
        private static UmbracoConnection _instance;

        /// <summary>
        /// Returns the UmbracoConnection instance registered with the global holder
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public static UmbracoConnection Instance
        {
            get
            {
                if (_instance == null)
                    throw new NullReferenceException("An UmbracoConnection instance has not been registered");
                return _instance;
            }
        }

        /// <summary>
        /// Registers and initializes the connection so it can be accessed statically.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Register(UmbracoConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException();
            if (_instance != null)
                throw new InvalidOperationException("An UmbracoConnection instance has already been registered with the holder");

            _instance = connection;
        }
    }
}
