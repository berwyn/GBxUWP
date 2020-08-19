using System.ComponentModel;
using System.Diagnostics;
using GBxUWP.Converters;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GBxUWP
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Controller _controller;
        private ControllerState _state;
        private CartridgeHeader _header;

        private string _connectLabel => _state.IsOpen ? "Disconnect" : "Connect";

        public MainPage()
        {
            InitializeComponent();
            _state = new ControllerState();
            _controller = new Controller(_state);

            Application.Current.Suspending += (_, __) =>
            {
                _controller.Dispose();
            };

            Application.Current.UnhandledException += (_, __) =>
            {
                _controller.Dispose();
            };
        }

        private void VoltageButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VoltageButtons.SelectedItem != null && _state.IsOpen)
            {
                var newVoltage = (Voltage)new VoltageToStringConverter().ConvertBack(VoltageButtons.SelectedItem, typeof(Voltage), null, null);
                _controller.SetVoltage(newVoltage);
            }
        }

        private void ReadHeader_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (!_state.IsOpen)
                return;

            _header = _controller.ReadGameboyHeader();
            Debug.WriteLine($"Found header for {_header?.Title}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_header)));
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (_state.IsOpen)
            {
                _controller.Close();
            }
            else
            {
                _controller.Open();
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_connectLabel)));
        }
    }
}
