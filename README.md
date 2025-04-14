# PLC Connect Framework

A C# library-like framework for connecting to various PLC (Programmable Logic Controller) devices. This project provides an extensible architecture that allows for easy implementation of different PLC communication protocols.

## Features

- **Extensible Architecture**: Easily add support for new PLC types
- **Async Communication**: All PLC operations are asynchronous
- **Factory Pattern**: Create PLC connections using a factory
- **Sample Implementations**: Includes sample implementations for Siemens S7 and Allen Bradley PLCs
- **WPF Demo Application**: Simple WPF application to demonstrate the framework

## Project Structure

- **PLCConnectFramework.Core**: The core library containing the PLC communication framework
- **PLCConnectFramework.App**: A WPF application that demonstrates the framework

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

## Libraries for Real PLC Communication

- **Siemens S7**: [S7.Net](https://github.com/S7NetPlus/s7netplus)
- **Allen Bradley**: [libplctag](https://github.com/libplctag/libplctag)
- **Modbus**: [NModbus](https://github.com/NModbus/NModbus)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- This project is intended as a starting point for PLC communication in C#
- The architecture is designed to be extensible and maintainable
