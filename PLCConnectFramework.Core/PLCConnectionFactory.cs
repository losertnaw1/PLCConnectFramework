using System;
using System.Collections.Generic;

namespace PLCConnectFramework.Core
{
    /// <summary>
    /// Factory for creating PLC connections
    /// </summary>
    public class PLCConnectionFactory
    {
        private static readonly Dictionary<string, Func<string, string, int, IPLCConnection>> _connectionCreators = 
            new Dictionary<string, Func<string, string, int, IPLCConnection>>();

        /// <summary>
        /// Register a PLC connection type
        /// </summary>
        /// <param name="plcType">Type of the PLC</param>
        /// <param name="creator">Function to create a connection of this type</param>
        public static void RegisterConnectionType(string plcType, Func<string, string, int, IPLCConnection> creator)
        {
            if (string.IsNullOrEmpty(plcType))
            {
                throw new ArgumentNullException(nameof(plcType));
            }

            if (creator == null)
            {
                throw new ArgumentNullException(nameof(creator));
            }

            _connectionCreators[plcType] = creator;
        }

        /// <summary>
        /// Create a PLC connection
        /// </summary>
        /// <param name="plcType">Type of the PLC</param>
        /// <param name="name">Name of the PLC</param>
        /// <param name="ipAddress">IP address of the PLC</param>
        /// <param name="port">Port number</param>
        /// <returns>A PLC connection</returns>
        public static IPLCConnection CreateConnection(string plcType, string name, string ipAddress, int port)
        {
            if (string.IsNullOrEmpty(plcType))
            {
                throw new ArgumentNullException(nameof(plcType));
            }

            if (!_connectionCreators.TryGetValue(plcType, out var creator))
            {
                throw new ArgumentException($"PLC type '{plcType}' is not registered.");
            }

            return creator(name, ipAddress, port);
        }

        /// <summary>
        /// Get all registered PLC types
        /// </summary>
        /// <returns>List of registered PLC types</returns>
        public static IEnumerable<string> GetRegisteredPLCTypes()
        {
            return _connectionCreators.Keys;
        }
    }
}
