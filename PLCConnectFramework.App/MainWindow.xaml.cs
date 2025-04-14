using System;
using System.Linq;
using System.Windows;
using PLCConnectFramework.Core;

namespace PLCConnectFramework.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IPLCConnection? _currentConnection;

        public MainWindow()
        {
            InitializeComponent();
            
            // Register built-in PLC connection types
            PLCConnectionRegistry.RegisterBuiltInConnectionTypes();
            
            // Load available PLC types
            LoadPLCTypes();
            
            // Set default values
            IpAddressTextBox.Text = "192.168.1.100";
            PortTextBox.Text = "102";
        }

        private void LoadPLCTypes()
        {
            var plcTypes = PLCConnectionFactory.GetRegisteredPLCTypes().ToList();
            PlcTypesList.ItemsSource = plcTypes;
            
            if (plcTypes.Any())
            {
                PlcTypesList.SelectedIndex = 0;
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? selectedPlcType = PlcTypesList.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedPlcType))
                {
                    MessageBox.Show("Please select a PLC type.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string plcName = PlcNameTextBox.Text;
                if (string.IsNullOrEmpty(plcName))
                {
                    MessageBox.Show("Please enter a PLC name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string ipAddress = IpAddressTextBox.Text;
                if (string.IsNullOrEmpty(ipAddress))
                {
                    MessageBox.Show("Please enter an IP address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(PortTextBox.Text, out int port))
                {
                    MessageBox.Show("Please enter a valid port number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create a new PLC connection
                _currentConnection = PLCConnectionFactory.CreateConnection(selectedPlcType, plcName, ipAddress, port);

                // Connect to the PLC
                bool connected = await _currentConnection.ConnectAsync();
                if (connected)
                {
                    ConnectionStatusTextBlock.Text = $"Connected to {_currentConnection.Name} ({_currentConnection.PLCType})";
                    
                    // Update UI
                    ConnectButton.IsEnabled = false;
                    DisconnectButton.IsEnabled = true;
                    ReadButton.IsEnabled = true;
                    WriteButton.IsEnabled = true;
                    
                    AppendToResult($"Connected to {_currentConnection.Name} ({_currentConnection.PLCType}) at {ipAddress}:{port}");
                }
                else
                {
                    ConnectionStatusTextBlock.Text = "Connection failed";
                    AppendToResult("Connection failed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to PLC: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AppendToResult($"Error: {ex.Message}");
            }
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentConnection != null)
                {
                    bool disconnected = await _currentConnection.DisconnectAsync();
                    if (disconnected)
                    {
                        ConnectionStatusTextBlock.Text = "Not connected";
                        
                        // Update UI
                        ConnectButton.IsEnabled = true;
                        DisconnectButton.IsEnabled = false;
                        ReadButton.IsEnabled = false;
                        WriteButton.IsEnabled = false;
                        
                        AppendToResult($"Disconnected from {_currentConnection.Name}");
                    }
                    else
                    {
                        AppendToResult("Disconnection failed");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error disconnecting from PLC: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AppendToResult($"Error: {ex.Message}");
            }
        }

        private async void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentConnection == null || !_currentConnection.IsConnected)
                {
                    MessageBox.Show("Not connected to PLC.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string address = AddressTextBox.Text;
                if (string.IsNullOrEmpty(address))
                {
                    MessageBox.Show("Please enter an address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Read from the PLC
                var result = await _currentConnection.ReadAsync<object>(address);
                
                // Display the result
                ValueTextBox.Text = result?.ToString() ?? "null";
                AppendToResult($"Read from {address}: {result}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading from PLC: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AppendToResult($"Error: {ex.Message}");
            }
        }

        private async void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentConnection == null || !_currentConnection.IsConnected)
                {
                    MessageBox.Show("Not connected to PLC.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string address = AddressTextBox.Text;
                if (string.IsNullOrEmpty(address))
                {
                    MessageBox.Show("Please enter an address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string value = ValueTextBox.Text;
                if (string.IsNullOrEmpty(value))
                {
                    MessageBox.Show("Please enter a value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Write to the PLC
                bool success = await _currentConnection.WriteAsync(address, value);
                
                // Display the result
                if (success)
                {
                    AppendToResult($"Write to {address}: {value} - Success");
                }
                else
                {
                    AppendToResult($"Write to {address}: {value} - Failed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to PLC: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AppendToResult($"Error: {ex.Message}");
            }
        }

        private void AppendToResult(string message)
        {
            ResultTextBox.Text += $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
            ResultTextBox.ScrollToEnd();
        }
    }
}
