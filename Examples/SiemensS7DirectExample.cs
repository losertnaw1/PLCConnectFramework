using PLCConnectFramework.Core;
using System;
using System.Threading.Tasks;

namespace PLCConnectFramework.Examples
{
    /// <summary>
    /// Example demonstrating how to use the Siemens S7 Direct implementation
    /// </summary>
    public class SiemensS7DirectExample
    {
        public static async Task RunExampleAsync()
        {
            Console.WriteLine("Siemens S7 Direct Connection Example");
            Console.WriteLine("====================================");
            
            // Register built-in PLC connection types
            PLCConnectionRegistry.RegisterBuiltInConnectionTypes();
            
            // Get connection parameters from user
            Console.Write("Enter PLC IP address (default: 192.168.1.100): ");
            string ipAddress = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ipAddress))
                ipAddress = "192.168.1.100";
                
            Console.Write("Enter PLC port (default: 102): ");
            string portStr = Console.ReadLine();
            int port = string.IsNullOrWhiteSpace(portStr) ? 102 : int.Parse(portStr);
            
            Console.Write("Enter PLC rack number (default: 0): ");
            string rackStr = Console.ReadLine();
            int rack = string.IsNullOrWhiteSpace(rackStr) ? 0 : int.Parse(rackStr);
            
            Console.Write("Enter PLC slot number (default: 2): ");
            string slotStr = Console.ReadLine();
            int slot = string.IsNullOrWhiteSpace(slotStr) ? 2 : int.Parse(slotStr);
            
            // Create a direct S7 connection
            var plcConnection = PLCConnectionFactory.CreateConnection("Siemens S7 Direct", "MyPLC", ipAddress, port);
            
            try
            {
                // Connect to the PLC
                Console.WriteLine($"\nConnecting to PLC at {ipAddress}:{port}...");
                bool connected = await plcConnection.ConnectAsync();
                
                if (connected)
                {
                    Console.WriteLine($"Connected to {plcConnection.Name} ({plcConnection.PLCType})");
                    
                    // Menu for operations
                    bool exit = false;
                    while (!exit && plcConnection.IsConnected)
                    {
                        Console.WriteLine("\nOperations:");
                        Console.WriteLine("1. Read a value");
                        Console.WriteLine("2. Write a value");
                        Console.WriteLine("3. Disconnect and exit");
                        Console.Write("\nSelect an operation (1-3): ");
                        
                        string choice = Console.ReadLine();
                        
                        switch (choice)
                        {
                            case "1":
                                await ReadValueAsync(plcConnection);
                                break;
                                
                            case "2":
                                await WriteValueAsync(plcConnection);
                                break;
                                
                            case "3":
                                exit = true;
                                break;
                                
                            default:
                                Console.WriteLine("Invalid choice. Please try again.");
                                break;
                        }
                    }
                    
                    // Disconnect from the PLC
                    Console.WriteLine("\nDisconnecting from PLC...");
                    await plcConnection.DisconnectAsync();
                    Console.WriteLine("Disconnected from PLC");
                }
                else
                {
                    Console.WriteLine("Failed to connect to PLC");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        
        private static async Task ReadValueAsync(IPLCConnection plcConnection)
        {
            try
            {
                Console.Write("\nEnter address to read (e.g., DB1.DBX0.0, DB1.DBW2, DB1.DBD4): ");
                string address = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(address))
                {
                    Console.WriteLine("Invalid address");
                    return;
                }
                
                Console.Write("Select data type (1=Bool, 2=Byte, 3=Word, 4=DWord): ");
                string typeChoice = Console.ReadLine();
                
                switch (typeChoice)
                {
                    case "1": // Bool
                        bool boolValue = await plcConnection.ReadAsync<bool>(address);
                        Console.WriteLine($"Read value: {boolValue}");
                        break;
                        
                    case "2": // Byte
                        byte byteValue = await plcConnection.ReadAsync<byte>(address);
                        Console.WriteLine($"Read value: {byteValue}");
                        break;
                        
                    case "3": // Word
                        short wordValue = await plcConnection.ReadAsync<short>(address);
                        Console.WriteLine($"Read value: {wordValue}");
                        break;
                        
                    case "4": // DWord
                        int dwordValue = await plcConnection.ReadAsync<int>(address);
                        Console.WriteLine($"Read value: {dwordValue}");
                        break;
                        
                    default:
                        Console.WriteLine("Invalid data type");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading value: {ex.Message}");
            }
        }
        
        private static async Task WriteValueAsync(IPLCConnection plcConnection)
        {
            try
            {
                Console.Write("\nEnter address to write (e.g., DB1.DBX0.0, DB1.DBW2, DB1.DBD4): ");
                string address = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(address))
                {
                    Console.WriteLine("Invalid address");
                    return;
                }
                
                Console.Write("Select data type (1=Bool, 2=Byte, 3=Word, 4=DWord): ");
                string typeChoice = Console.ReadLine();
                
                Console.Write("Enter value to write: ");
                string valueStr = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(valueStr))
                {
                    Console.WriteLine("Invalid value");
                    return;
                }
                
                bool success = false;
                
                switch (typeChoice)
                {
                    case "1": // Bool
                        bool boolValue = bool.Parse(valueStr);
                        success = await plcConnection.WriteAsync(address, boolValue);
                        break;
                        
                    case "2": // Byte
                        byte byteValue = byte.Parse(valueStr);
                        success = await plcConnection.WriteAsync(address, byteValue);
                        break;
                        
                    case "3": // Word
                        short wordValue = short.Parse(valueStr);
                        success = await plcConnection.WriteAsync(address, wordValue);
                        break;
                        
                    case "4": // DWord
                        int dwordValue = int.Parse(valueStr);
                        success = await plcConnection.WriteAsync(address, dwordValue);
                        break;
                        
                    default:
                        Console.WriteLine("Invalid data type");
                        return;
                }
                
                if (success)
                {
                    Console.WriteLine("Value written successfully");
                }
                else
                {
                    Console.WriteLine("Failed to write value");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing value: {ex.Message}");
            }
        }
    }
}
