using PLCConnectFramework.Core.Implementations;

namespace PLCConnectFramework.Core
{
    /// <summary>
    /// Registry for PLC connection types
    /// </summary>
    public static class PLCConnectionRegistry
    {
        /// <summary>
        /// Register all built-in PLC connection types
        /// </summary>
        public static void RegisterBuiltInConnectionTypes()
        {
            // Register Siemens S7 PLC connection
            PLCConnectionFactory.RegisterConnectionType("Siemens S7", (name, ipAddress, port) => 
                new SiemensS7Connection(name, ipAddress, port));

            // Register Allen Bradley PLC connection
            PLCConnectionFactory.RegisterConnectionType("Allen Bradley", (name, ipAddress, port) => 
                new AllenBradleyConnection(name, ipAddress, port));
        }
    }
}
