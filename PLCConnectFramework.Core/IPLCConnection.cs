using System;
using System.Threading.Tasks;

namespace PLCConnectFramework.Core
{
    /// <summary>
    /// Interface defining the basic operations for PLC connections
    /// </summary>
    public interface IPLCConnection
    {
        /// <summary>
        /// Gets the connection status
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the name of the PLC
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of the PLC
        /// </summary>
        string PLCType { get; }

        /// <summary>
        /// Connect to the PLC
        /// </summary>
        /// <returns>True if connection was successful, false otherwise</returns>
        Task<bool> ConnectAsync();

        /// <summary>
        /// Disconnect from the PLC
        /// </summary>
        /// <returns>True if disconnection was successful, false otherwise</returns>
        Task<bool> DisconnectAsync();

        /// <summary>
        /// Read a value from the PLC
        /// </summary>
        /// <typeparam name="T">Type of the value to read</typeparam>
        /// <param name="address">Address to read from</param>
        /// <returns>The value read from the PLC</returns>
        Task<T> ReadAsync<T>(string address);

        /// <summary>
        /// Write a value to the PLC
        /// </summary>
        /// <typeparam name="T">Type of the value to write</typeparam>
        /// <param name="address">Address to write to</param>
        /// <param name="value">Value to write</param>
        /// <returns>True if write was successful, false otherwise</returns>
        Task<bool> WriteAsync<T>(string address, T value);
    }
}
