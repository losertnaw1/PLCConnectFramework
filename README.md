# PLC Connect Framework

A C# library-like framework for connecting to various PLC (Programmable Logic Controller) devices. This project provides an extensible architecture that allows for easy implementation of different PLC communication protocols.

## Features

- **Extensible Architecture**: Easily add support for new PLC types
- **Async Communication**: All PLC operations are asynchronous
- **Factory Pattern**: Create PLC connections using a factory
- **Sample Implementations**: Includes sample implementations for Siemens S7 and Allen Bradley PLCs
- **Direct S7 Protocol Implementation**: Low-level implementation of the Siemens S7 protocol without external libraries
- **WPF Demo Application**: Simple WPF application to demonstrate the framework

## Project Structure

- **PLCConnectFramework.Core**: The core library containing the PLC communication framework
- **PLCConnectFramework.App**: A WPF application that demonstrates the framework
- **PLCConnectFramework.Examples**: Console application examples showing how to use the framework

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- Visual Studio 2022 or later (recommended)

### Building the Project

1. Clone the repository
2. Open the solution file `PLCConnectFramework.sln` in Visual Studio
3. Build the solution

### Running the Demo Application

1. Set `PLCConnectFramework.App` as the startup project
2. Run the application
3. Select a PLC type from the list
4. Enter the PLC name, IP address, and port
5. Click "Connect" to establish a connection
6. Use the Read/Write operations to interact with the PLC

### Running the Console Examples

1. Set `PLCConnectFramework.Examples` as the startup project
2. Run the application
3. Select an example from the menu
4. Follow the on-screen instructions to interact with the PLC

## Extending the Framework

### Adding a New PLC Type

1. Create a new class that inherits from `PLCConnectionBase`
2. Implement the abstract methods:
   - `ConnectAsync()`
   - `DisconnectAsync()`
   - `ReadAsync<T>(string address)`
   - `WriteAsync<T>(string address, T value)`
3. Register the new PLC type in `PLCConnectionRegistry.cs`

Example:

```csharp
public class MyCustomPLC : PLCConnectionBase
{
    public override string PLCType => "My Custom PLC";

    public MyCustomPLC(string name, string ipAddress, int port)
        : base(name, ipAddress, port)
    {
    }

    public override async Task<bool> ConnectAsync()
    {
        // Implement connection logic
    }

    public override async Task<bool> DisconnectAsync()
    {
        // Implement disconnection logic
    }

    public override async Task<T> ReadAsync<T>(string address)
    {
        // Implement read logic
    }

    public override async Task<bool> WriteAsync<T>(string address, T value)
    {
        // Implement write logic
    }
}

// Register the new PLC type
PLCConnectionFactory.RegisterConnectionType("My Custom PLC", (name, ipAddress, port) =>
    new MyCustomPLC(name, ipAddress, port));
```

## Real-World Implementation

For real-world usage, you would need to:

1. Replace the simulated implementations with actual PLC communication
2. Add proper error handling and logging
3. Implement configuration options for PLC connections
4. Add security features for industrial environments

## Using the Direct Siemens S7 Implementation

The framework includes a direct implementation of the Siemens S7 protocol without using external libraries. This implementation communicates directly with the PLC using the S7 protocol over TCP/IP.

### Connecting to a Siemens S7 PLC

```csharp
// Create a direct S7 connection
var plcConnection = PLCConnectionFactory.CreateConnection("Siemens S7 Direct", "MyPLC", "192.168.1.100", 102);

// Connect to the PLC
await plcConnection.ConnectAsync();
```

### Reading Data from a Siemens S7 PLC

```csharp
// Read a boolean value (bit)
bool bitValue = await plcConnection.ReadAsync<bool>("DB1.DBX0.0");

// Read a byte
byte byteValue = await plcConnection.ReadAsync<byte>("DB1.DBB1");

// Read a word (2 bytes)
short wordValue = await plcConnection.ReadAsync<short>("DB1.DBW2");

// Read a double word (4 bytes)
int dwordValue = await plcConnection.ReadAsync<int>("DB1.DBD4");
```

### Writing Data to a Siemens S7 PLC

```csharp
// Write a boolean value (bit)
await plcConnection.WriteAsync("DB1.DBX0.0", true);

// Write a byte
await plcConnection.WriteAsync("DB1.DBB1", (byte)42);

// Write a word (2 bytes)
await plcConnection.WriteAsync("DB1.DBW2", (short)12345);

// Write a double word (4 bytes)
await plcConnection.WriteAsync("DB1.DBD4", 987654321);
```

### Address Format for Siemens S7 PLCs

The address format for Siemens S7 PLCs follows this pattern:

- `DB<number>.<type><byte>[.<bit>]`

Where:
- `<number>` is the data block number
- `<type>` is one of:
  - `DBX` for bits
  - `DBB` for bytes
  - `DBW` for words (2 bytes)
  - `DBD` for double words (4 bytes)
- `<byte>` is the byte offset within the data block
- `<bit>` is the bit offset within the byte (only for DBX)

Examples:
- `DB1.DBX0.0` - Bit 0 of byte 0 in data block 1
- `DB1.DBB1` - Byte 1 in data block 1
- `DB1.DBW2` - Word starting at byte 2 in data block 1
- `DB1.DBD4` - Double word starting at byte 4 in data block 1

### Complete Example

```csharp
using PLCConnectFramework.Core;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Register built-in PLC connection types
        PLCConnectionRegistry.RegisterBuiltInConnectionTypes();

        // Create a direct S7 connection
        var plcConnection = PLCConnectionFactory.CreateConnection("Siemens S7 Direct", "MyPLC", "192.168.1.100", 102);

        try
        {
            // Connect to the PLC
            bool connected = await plcConnection.ConnectAsync();
            if (connected)
            {
                Console.WriteLine($"Connected to {plcConnection.Name}");

                // Read a value
                int value = await plcConnection.ReadAsync<int>("DB1.DBD0");
                Console.WriteLine($"Read value: {value}");

                // Write a value
                await plcConnection.WriteAsync("DB1.DBD0", 12345);
                Console.WriteLine("Value written successfully");

                // Read the value back to verify
                value = await plcConnection.ReadAsync<int>("DB1.DBD0");
                Console.WriteLine($"Read value after write: {value}");

                // Disconnect from the PLC
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
    }
}
```

## Libraries for Real PLC Communication

Alternatively, you can use established libraries for PLC communication:

- **Siemens S7**: [S7.Net](https://github.com/S7NetPlus/s7netplus)
- **Allen Bradley**: [libplctag](https://github.com/libplctag/libplctag)
- **Modbus**: [NModbus](https://github.com/NModbus/NModbus)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- This project is intended as a starting point for PLC communication in C#
- The architecture is designed to be extensible and maintainable
- The direct S7 implementation is based on the S7 protocol specification and reverse engineering
