using System.ComponentModel;
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

        private void ControllerView_VoltageChanged(object sender, Views.VoltageChangedEventArgs e)
        {
            if (_state.IsOpen)
                _controller.SetVoltage(e.Value);
        }

        private void ControllerView_OpenConnection(object sender, RoutedEventArgs e)
        {
            if (_state.IsOpen)
            {
                _controller.Close();
            }
            else
            {
                _controller.Open();
            }
        }

        private void ControllerView_ReadHeader(object sender, RoutedEventArgs e)
        {
            if (!_state.IsOpen)
                return;

            _header = _controller.ReadGameboyHeader();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_header)));
        }
    }
}
