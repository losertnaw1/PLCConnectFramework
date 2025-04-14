using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PLCConnectFramework.Core.Implementations
{
    /// <summary>
    /// Implementation of a direct Siemens S7 PLC connection using raw TCP/IP
    /// </summary>
    public class SiemensS7DirectConnection : PLCConnectionBase
    {
        private int _rack;
        private int _slot;
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private ushort _pduSize = 240;
        private ushort _pduReference = 0;

        /// <summary>
        /// Gets the type of the PLC
        /// </summary>
        public override string PLCType => "Siemens S7 Direct";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the PLC</param>
        /// <param name="ipAddress">IP address of the PLC</param>
        /// <param name="port">Port number (default: 102)</param>
        /// <param name="rack">Rack number (default: 0)</param>
        /// <param name="slot">Slot number (default: 2)</param>
        public SiemensS7DirectConnection(string name, string ipAddress, int port = 102, int rack = 0, int slot = 2)
            : base(name, ipAddress, port)
        {
            _rack = rack;
            _slot = slot;
        }

        /// <summary>
        /// Connect to the PLC
        /// </summary>
        /// <returns>True if connection was successful, false otherwise</returns>
        public override async Task<bool> ConnectAsync()
        {
            try
            {
                // Step 1: Establish TCP connection
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_ipAddress, _port);
                _stream = _tcpClient.GetStream();
                
                // Step 2: Set up COTP (Connection Oriented Transport Protocol) Connection
                if (!await SetupCOTPConnectionAsync())
                    return false;
                    
                // Step 3: Set up S7 Communication (negotiate PDU size, etc.)
                if (!await SetupS7CommunicationAsync())
                    return false;
                    
                _isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to Siemens S7 PLC: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        private async Task<bool> SetupCOTPConnectionAsync()
        {
            // TPKT (RFC1006 Header)
            // - 3-byte header + 1-byte length
            // COTP (ISO 8073 Header)
            // - Connection request
            byte[] cotpConnectionRequest = new byte[] {
                // TPKT Header
                0x03, 0x00,             // Version
                0x00, 0x16,             // Length (22 bytes)
                // COTP Header
                0x11,                   // PDU Size
                0xE0,                   // CR - Connection Request
                0x00, 0x00,             // Destination Reference
                0x00, 0x01,             // Source Reference
                0x00,                   // Flags
                // COTP Parameters
                0xC1, 0x02, 0x10, 0x00, // Parameter 1: TPDU Size (1024)
                0xC2, 0x02, 0x01, 0x02, // Parameter 2: Src TSAP (local TSAP)
                0xC0, 0x01, 0x0A        // Parameter 3: Dst TSAP (remote TSAP)
            };
            
            // Modify the destination TSAP (last byte) based on rack/slot
            cotpConnectionRequest[cotpConnectionRequest.Length - 1] = (byte)((_rack * 0x20) + _slot);
            
            await _stream.WriteAsync(cotpConnectionRequest, 0, cotpConnectionRequest.Length);
            
            // Read the response
            byte[] response = new byte[22];
            int bytesRead = await _stream.ReadAsync(response, 0, response.Length);
            
            // Check if the response is valid (should be Connection Confirm)
            if (bytesRead < 11 || response[5] != 0xD0) // 0xD0 = CC (Connection Confirm)
            {
                return false;
            }
            
            return true;
        }

        private async Task<bool> SetupS7CommunicationAsync()
        {
            // S7 Communication Setup
            byte[] s7CommSetup = new byte[] {
                // TPKT Header
                0x03, 0x00,             // Version
                0x00, 0x19,             // Length (25 bytes)
                // COTP Header
                0x02,                   // Header Length
                0xF0, 0x80,             // Data Transfer
                // S7 Protocol Header
                0x32,                   // Protocol ID (S7 Protocol)
                0x01,                   // Message Type (Job Request)
                0x00, 0x00,             // Reserved
                0x00, 0x01,             // PDU Reference
                0x00, 0x00, 0x00, 0x08, // Parameter Length (8 bytes)
                0x00, 0x00,             // Data Length (0 bytes)
                // S7 Parameter
                0xF0,                   // Function (Setup Communication)
                0x00,                   // Reserved
                0x00, 0x01,             // Max AmQ Calling (1)
                0x00, 0x01,             // Max AmQ Called (1)
                0x00, 0xF0              // PDU Size (240 bytes)
            };
            
            await _stream.WriteAsync(s7CommSetup, 0, s7CommSetup.Length);
            
            // Read the response
            byte[] response = new byte[27];
            int bytesRead = await _stream.ReadAsync(response, 0, response.Length);
            
            // Check if the response is valid
            if (bytesRead < 20 || response[8] != 0x03) // 0x03 = Ack Data
            {
                return false;
            }
            
            // Extract negotiated PDU size (bytes 23-24)
            _pduSize = (ushort)((response[23] << 8) | response[24]);
            
            return true;
        }
    }

    /// <summary>
    /// Disconnect from the PLC
    /// </summary>
    /// <returns>True if disconnection was successful, false otherwise</returns>
    public override async Task<bool> DisconnectAsync()
    {
        try
        {
            if (_tcpClient != null && _tcpClient.Connected)
            {
                _stream?.Close();
                _tcpClient.Close();
            }
            
            _isConnected = false;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disconnecting from Siemens S7 PLC: {ex.Message}");
            return false;
        }
    }
    /// <summary>
    /// Read a value from the PLC
    /// </summary>
    /// <typeparam name="T">Type of the value to read</typeparam>
    /// <param name="address">Address to read from (e.g., "DB1.DBX0.0", "DB1.DBW2", "DB1.DBD4")</param>
    /// <returns>The value read from the PLC</returns>
    public override async Task<T> ReadAsync<T>(string address)
    {
        ValidateConnection();
        
        try
        {
            // Parse the address
            (int dbNumber, string dataType, int startByte, int bitOffset) = ParseS7Address(address);
            
            // Create read request
            byte[] readRequest = CreateReadRequest(dbNumber, dataType, startByte, bitOffset);
            
            // Send the request
            await _stream.WriteAsync(readRequest, 0, readRequest.Length);
            
            // Read the response
            byte[] header = new byte[7]; // TPKT + COTP header
            await _stream.ReadAsync(header, 0, header.Length);
            
            // Get the S7 response length from TPKT header
            int s7ResponseLength = (header[2] << 8) | header[3];
            byte[] s7Response = new byte[s7ResponseLength - 7];
            await _stream.ReadAsync(s7Response, 0, s7Response.Length);
            
            // Process the response
            return ProcessReadResponse<T>(s7Response, dataType);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading from Siemens S7 PLC: {ex.Message}");
            throw;
        }
    }

    private (int dbNumber, string dataType, int startByte, int bitOffset) ParseS7Address(string address)
    {
        // Example: DB1.DBX0.0, DB1.DBW2, DB1.DBD4
        string[] parts = address.Split('.');
        
        if (parts.Length < 2 || !parts[0].StartsWith("DB"))
            throw new ArgumentException("Invalid address format. Expected format: DB<number>.<type><byte>[.<bit>]");
        
        int dbNumber = int.Parse(parts[0].Substring(2));
        string dataType = parts[1].Substring(0, 3); // DBX, DBW, DBD
        
        int startByte = 0;
        int bitOffset = 0;
        
        if (dataType == "DBX")
        {
            if (parts.Length < 3)
                throw new ArgumentException("Bit address requires bit offset. Expected format: DB<number>.DBX<byte>.<bit>");
            
            startByte = int.Parse(parts[1].Substring(3));
            bitOffset = int.Parse(parts[2]);
        }
        else
        {
            startByte = int.Parse(parts[1].Substring(3));
        }
        
        return (dbNumber, dataType, startByte, bitOffset);
    }

    private byte[] CreateReadRequest(int dbNumber, string dataType, int startByte, int bitOffset)
    {
        // Determine the data size based on the data type
        int dataSize = 0;
        byte dataCode = 0;
        
        switch (dataType)
        {
            case "DBX": // Bit
                dataSize = 1;
                dataCode = 0x01;
                break;
            case "DBB": // Byte
                dataSize = 1;
                dataCode = 0x02;
                break;
            case "DBW": // Word (2 bytes)
                dataSize = 2;
                dataCode = 0x03;
                break;
            case "DBD": // Double Word (4 bytes)
                dataSize = 4;
                dataCode = 0x04;
                break;
            default:
                throw new ArgumentException($"Unsupported data type: {dataType}");
        }
        
        // Increment PDU reference
        _pduReference++;
        
        // Create the S7 read request
        byte[] request = new byte[31];
        
        // TPKT Header
        request[0] = 0x03; // Version
        request[1] = 0x00;
        request[2] = 0x00; // Length (will be set later)
        request[3] = 0x1F; // 31 bytes total
        
        // COTP Header
        request[4] = 0x02; // Header Length
        request[5] = 0xF0; // Data Transfer
        request[6] = 0x80;
        
        // S7 Protocol Header
        request[7] = 0x32; // Protocol ID (S7 Protocol)
        request[8] = 0x01; // Message Type (Job Request)
        request[9] = 0x00; // Reserved
        request[10] = 0x00;
        request[11] = (byte)(_pduReference >> 8); // PDU Reference (high byte)
        request[12] = (byte)(_pduReference & 0xFF); // PDU Reference (low byte)
        request[13] = 0x00; // Parameter Length (high byte)
        request[14] = 0x0E; // Parameter Length (14 bytes)
        request[15] = 0x00; // Data Length (high byte)
        request[16] = 0x00; // Data Length (low byte)
        
        // S7 Parameter
        request[17] = 0x04; // Function (Read Variable)
        request[18] = 0x01; // Item count (1 item)
        
        // Item
        request[19] = 0x12; // Variable specification
        request[20] = 0x0A; // Length of following address specification
        request[21] = 0x10; // Syntax ID (S7 Any pointer)
        request[22] = dataCode; // Transport size
        request[23] = 0x00; // Length (high byte)
        request[24] = (byte)dataSize; // Length (low byte)
        request[25] = (byte)(dbNumber >> 8); // DB Number (high byte)
        request[26] = (byte)(dbNumber & 0xFF); // DB Number (low byte)
        request[27] = 0x84; // Area code (0x84 = DB)
        request[28] = (byte)(startByte * 8 + bitOffset >> 16); // Address: Byte address * 8 + bit offset (high byte)
        request[29] = (byte)(startByte * 8 + bitOffset >> 8); // Address: Byte address * 8 + bit offset (middle byte)
        request[30] = (byte)(startByte * 8 + bitOffset); // Address: Byte address * 8 + bit offset (low byte)
        
        return request;
    }

    private T ProcessReadResponse<T>(byte[] response, string dataType)
    {
        // Check if the response is valid
        if (response.Length < 14 || response[0] != 0x32 || response[1] != 0x03)
            throw new Exception("Invalid response from PLC");
        
        // Get the data length
        int dataLength = (response[12] << 8) | response[13];
        
        if (dataLength <= 0)
            throw new Exception("No data received from PLC");
        
        // Get the data (skip the transport size byte)
        byte[] data = new byte[dataLength - 1];
        Array.Copy(response, 15, data, 0, dataLength - 1);
        
        // Convert the data to the requested type
        return ConvertToType<T>(data, dataType);
    }

    private T ConvertToType<T>(byte[] data, string dataType)
    {
        object result = null;
        
        switch (dataType)
        {
            case "DBX": // Bit
                result = (data[0] & 0x01) == 0x01;
                break;
            case "DBB": // Byte
                result = data[0];
                break;
            case "DBW": // Word (2 bytes)
                result = (short)((data[0] << 8) | data[1]);
                break;
            case "DBD": // Double Word (4 bytes)
                result = (int)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);
                break;
            default:
                throw new ArgumentException($"Unsupported data type: {dataType}");
        }
        
        return (T)Convert.ChangeType(result, typeof(T));
    }
    /// <summary>
    /// Write a value to the PLC
    /// </summary>
    /// <typeparam name="T">Type of the value to write</typeparam>
    /// <param name="address">Address to write to (e.g., "DB1.DBX0.0", "DB1.DBW2", "DB1.DBD4")</param>
    /// <param name="value">Value to write</param>
    /// <returns>True if write was successful, false otherwise</returns>
    public override async Task<bool> WriteAsync<T>(string address, T value)
    {
        ValidateConnection();
        
        try
        {
            // Parse the address
            (int dbNumber, string dataType, int startByte, int bitOffset) = ParseS7Address(address);
            
            // Convert the value to bytes
            byte[] data = ConvertToBytesForWrite(value, dataType);
            
            // Create write request
            byte[] writeRequest = CreateWriteRequest(dbNumber, dataType, startByte, bitOffset, data);
            
            // Send the request
            await _stream.WriteAsync(writeRequest, 0, writeRequest.Length);
            
            // Read the response
            byte[] header = new byte[7]; // TPKT + COTP header
            await _stream.ReadAsync(header, 0, header.Length);
            
            // Get the S7 response length from TPKT header
            int s7ResponseLength = (header[2] << 8) | header[3];
            byte[] s7Response = new byte[s7ResponseLength - 7];
            await _stream.ReadAsync(s7Response, 0, s7Response.Length);
            
            // Check if the write was successful
            return s7Response.Length >= 2 && s7Response[1] == 0x01; // 0x01 = Positive response
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to Siemens S7 PLC: {ex.Message}");
            return false;
        }
    }

    private byte[] ConvertToBytesForWrite<T>(T value, string dataType)
    {
        byte[] result = null;
        
        switch (dataType)
        {
            case "DBX": // Bit
                bool boolValue = Convert.ToBoolean(value);
                result = new byte[] { (byte)(boolValue ? 0x01 : 0x00) };
                break;
            case "DBB": // Byte
                byte byteValue = Convert.ToByte(value);
                result = new byte[] { byteValue };
                break;
            case "DBW": // Word (2 bytes)
                short wordValue = Convert.ToInt16(value);
                result = new byte[] { 
                    (byte)(wordValue >> 8), 
                    (byte)(wordValue & 0xFF) 
                };
                break;
            case "DBD": // Double Word (4 bytes)
                int dwordValue = Convert.ToInt32(value);
                result = new byte[] { 
                    (byte)(dwordValue >> 24), 
                    (byte)(dwordValue >> 16), 
                    (byte)(dwordValue >> 8), 
                    (byte)(dwordValue & 0xFF) 
                };
                break;
            default:
                throw new ArgumentException($"Unsupported data type: {dataType}");
        }
        
        return result;
    }

    private byte[] CreateWriteRequest(int dbNumber, string dataType, int startByte, int bitOffset, byte[] data)
    {
        // Determine the data size based on the data type
        int dataSize = data.Length;
        byte dataCode = 0;
        
        switch (dataType)
        {
            case "DBX": // Bit
                dataCode = 0x01;
                break;
            case "DBB": // Byte
                dataCode = 0x02;
                break;
            case "DBW": // Word (2 bytes)
                dataCode = 0x03;
                break;
            case "DBD": // Double Word (4 bytes)
                dataCode = 0x04;
                break;
            default:
                throw new ArgumentException($"Unsupported data type: {dataType}");
        }
        
        // Increment PDU reference
        _pduReference++;
        
        // Calculate the total length
        int totalLength = 35 + data.Length;
        
        // Create the S7 write request
        byte[] request = new byte[totalLength];
        
        // TPKT Header
        request[0] = 0x03; // Version
        request[1] = 0x00;
        request[2] = (byte)(totalLength >> 8); // Length (high byte)
        request[3] = (byte)(totalLength & 0xFF); // Length (low byte)
        
        // COTP Header
        request[4] = 0x02; // Header Length
        request[5] = 0xF0; // Data Transfer
        request[6] = 0x80;
        
        // S7 Protocol Header
        request[7] = 0x32; // Protocol ID (S7 Protocol)
        request[8] = 0x01; // Message Type (Job Request)
        request[9] = 0x00; // Reserved
        request[10] = 0x00;
        request[11] = (byte)(_pduReference >> 8); // PDU Reference (high byte)
        request[12] = (byte)(_pduReference & 0xFF); // PDU Reference (low byte)
        request[13] = 0x00; // Parameter Length (high byte)
        request[14] = 0x0E; // Parameter Length (14 bytes)
        request[15] = (byte)((4 + data.Length) >> 8); // Data Length (high byte)
        request[16] = (byte)((4 + data.Length) & 0xFF); // Data Length (low byte)
        
        // S7 Parameter
        request[17] = 0x05; // Function (Write Variable)
        request[18] = 0x01; // Item count (1 item)
        
        // Item
        request[19] = 0x12; // Variable specification
        request[20] = 0x0A; // Length of following address specification
        request[21] = 0x10; // Syntax ID (S7 Any pointer)
        request[22] = dataCode; // Transport size
        request[23] = 0x00; // Length (high byte)
        request[24] = (byte)dataSize; // Length (low byte)
        request[25] = (byte)(dbNumber >> 8); // DB Number (high byte)
        request[26] = (byte)(dbNumber & 0xFF); // DB Number (low byte)
        request[27] = 0x84; // Area code (0x84 = DB)
        request[28] = (byte)(startByte * 8 + bitOffset >> 16); // Address: Byte address * 8 + bit offset (high byte)
        request[29] = (byte)(startByte * 8 + bitOffset >> 8); // Address: Byte address * 8 + bit offset (middle byte)
        request[30] = (byte)(startByte * 8 + bitOffset); // Address: Byte address * 8 + bit offset (low byte)
        
        // Data
        request[31] = 0x00; // Return code
        request[32] = 0x04; // Transport size
        request[33] = (byte)(data.Length >> 8); // Data length (high byte)
        request[34] = (byte)(data.Length & 0xFF); // Data length (low byte)
        
        // Copy the actual data
        Array.Copy(data, 0, request, 35, data.Length);
        
        return request;
    }
}