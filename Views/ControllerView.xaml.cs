using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GBxUWP.Converters;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
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

    public class ROMSaveRequestedArgs
    {
        public readonly StorageFolder SaveLocation;

        public ROMSaveRequestedArgs(StorageFolder saveLocation)
        {
            SaveLocation = saveLocation;
        }
    }

    public delegate void ROMSaveRequestedEventHandler(object sender, ROMSaveRequestedArgs e);

    public sealed partial class ControllerView : UserControl
    {
        public event RoutedEventHandler OpenConnection;
        public event RoutedEventHandler ReadHeader;
        public event VoltageChangedEventHandler VoltageChanged;
        public event ROMSaveRequestedEventHandler ROMSaveRequested;

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

        private async void ReadROM_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderPicker = new FolderPicker()
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                };

                folderPicker.FileTypeFilter.Add("*");

                var targetFolder = await folderPicker.PickSingleFolderAsync().AsTask();

                ROMSaveRequested?.Invoke(this, new ROMSaveRequestedArgs(targetFolder));
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to pick file!");
            }
        }
    }
}
