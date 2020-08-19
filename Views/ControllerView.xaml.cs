using GBxUWP.Converters;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GBxUWP.Views
{
    public class VoltageChangedEventArgs
    {
        public readonly Voltage Value;

        public VoltageChangedEventArgs(Voltage voltage)
        {
            Value = voltage;
        }
    }

    public delegate void VoltageChangedEventHandler(object sender, VoltageChangedEventArgs e);

    public sealed partial class ControllerView : UserControl
    {
        public event RoutedEventHandler OpenConnection;
        public event RoutedEventHandler ReadHeader;
        public event VoltageChangedEventHandler VoltageChanged;

        public ControllerState State
        {
            get { return (ControllerState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(ControllerState), typeof(ControllerView), new PropertyMetadata(null));

        public CartridgeHeader Header
        {
            get { return (CartridgeHeader)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(CartridgeHeader), typeof(ControllerView), new PropertyMetadata(null));

        private string[] _consoles;
        private string _connectLabel;
        private string _disconnectLabel;

        public ControllerView()
        {
            InitializeComponent();

            var resourceLoader = ResourceLoader.GetForCurrentView();

            _consoles = new[]
            {
                resourceLoader.GetString("Console/Gameboy"),
                resourceLoader.GetString("Console/GameboyAdvance")
            };

            _connectLabel = resourceLoader.GetString("Connect");
            _disconnectLabel = resourceLoader.GetString("Disconnect");
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            OpenConnection?.Invoke(this, new RoutedEventArgs());
        }

        private void VoltageButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VoltageButtons.SelectedItem != null && State.IsOpen)
            {
                var newVoltage = (Voltage)new VoltageToStringConverter().ConvertBack(VoltageButtons.SelectedItem, typeof(Voltage), null, null);
                VoltageChanged?.Invoke(this, new VoltageChangedEventArgs(newVoltage));
            }
        }

        private void ReadHeader_Click(object sender, RoutedEventArgs e)
        {
            ReadHeader?.Invoke(this, new RoutedEventArgs());
        }
    }
}
