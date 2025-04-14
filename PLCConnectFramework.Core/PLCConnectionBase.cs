using System;
using System.Threading.Tasks;

namespace PLCConnectFramework.Core
{
    /// <summary>
    /// Base class for PLC connections
    /// </summary>
    public abstract class PLCConnectionBase : IPLCConnection
    {
        protected string _name;
        protected string _ipAddress;
        protected int _port;
        protected bool _isConnected;

        /// <summary>
        /// Gets the connection status
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// Gets the name of the PLC
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets the type of the PLC
        /// </summary>
        public abstract string PLCType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the PLC</param>
        /// <param name="ipAddress">IP address of the PLC</param>
        /// <param name="port">Port number</param>
        protected PLCConnectionBase(string name, string ipAddress, int port)
        {
            _name = name;
            _ipAddress = ipAddress;
            _port = port;
            _isConnected = false;
        }

        /// <summary>
        /// Connect to the PLC
        /// </summary>
        /// <returns>True if connection was successful, false otherwise</returns>
        public abstract Task<bool> ConnectAsync();

        /// <summary>
        /// Disconnect from the PLC
        /// </summary>
        /// <returns>True if disconnection was successful, false otherwise</returns>
        public abstract Task<bool> DisconnectAsync();

        /// <summary>
        /// Read a value from the PLC
        /// </summary>
        /// <typeparam name="T">Type of the value to read</typeparam>
        /// <param name="address">Address to read from</param>
        /// <returns>The value read from the PLC</returns>
        public abstract Task<T> ReadAsync<T>(string address);

        /// <summary>
        /// Write a value to the PLC
        /// </summary>
        /// <typeparam name="T">Type of the value to write</typeparam>
        /// <param name="address">Address to write to</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if write was successful, false otherwise</returns>
        public abstract Task<bool> WriteAsync<T>(string address, T value);

        /// <summary>
        /// Validates if the connection is established before performing operations
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when not connected to PLC</exception>
        protected void ValidateConnection()
        {
            if (!_isConnected)
            {
                throw new InvalidOperationException("Not connected to PLC. Please connect first.");
            }
        }
    }
}
