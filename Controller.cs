using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace GBxUWP
{
    static class Constants
    {
        public const byte VOLTAGE_5V = (byte)'5';
        public const byte VOLTAGE_3V = (byte)'3';

        public const byte COMMAND_RESET = (byte)'0';
        public const byte COMMAND_CONTINUE = (byte)'1';
        public const byte COMMAND_READ_PCB_VERSION = (byte)'h';
        public const byte COMMAND_READ_FIRMWARE_VERSION = (byte)'V';
        public const byte COMMAND_READ_ROM_RAM = (byte)'R';

        public const byte COMMAND_SET_START_ADDRESS = (byte)'A';

        public const byte COMMAND_MODE_GAMEBOY = 1;
        public const byte COMMAND_MODE_GAMEBOY_ADVANCE = 2;
        public const byte COMMAND_MODE_CART = (byte)'C';

        public const byte PCB_1_0 = 1;
        public const byte PCB_1_1 = 2;
        public const byte PCB_1_3 = 4;
        public const byte PCB_XMAS = 90;
    }

    enum PCBVersion
    {
        Unknown,
        v1_0,
        v1_1,
        v1_3,
        XMAS
    }

    enum Voltage
    {
        Unknown,
        Gameboy,
        GameboyAdvance
    }

    class ControllerState : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        private byte _pcbVersion;
        public PCBVersion BoardVersion
        {
            get
            {
                switch (_pcbVersion)
                {
                    case Constants.PCB_1_0:
                        return PCBVersion.v1_0;
                    case Constants.PCB_1_1:
                        return PCBVersion.v1_1;
                    case Constants.PCB_1_3:
                        return PCBVersion.v1_3;
                    case Constants.PCB_XMAS:
                        return PCBVersion.XMAS;
                    default:
                        return PCBVersion.Unknown;
                }
            }
        }

        private byte _pcbVoltage;
        public Voltage BoardVoltage
        {
            get
            {
                switch (_pcbVoltage)
                {
                    case Constants.VOLTAGE_5V:
                    case Constants.COMMAND_MODE_GAMEBOY:
                        return Voltage.Gameboy;
                    case Constants.VOLTAGE_3V:
                    case Constants.COMMAND_MODE_GAMEBOY_ADVANCE:
                        return Voltage.GameboyAdvance;
                    default:
                        return Voltage.Unknown;
                }
            }
        }

        public bool IsOpen { get; private set; }

        public bool CanSetVoltage => IsOpen && (_pcbVersion == Constants.PCB_1_3 || _pcbVersion == Constants.PCB_XMAS);

        public void SignalChange()
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(IsOpen)));
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(CanSetVoltage)));
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(BoardVersion)));
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(BoardVoltage)));
        }

        public void SignalChangeCompleted()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOpen)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanSetVoltage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BoardVersion)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BoardVoltage)));
        }

        public void UpdateState(bool isOpen, byte pcbVersion, byte pcbVoltage)
        {
            IsOpen = isOpen;

            _pcbVersion = pcbVersion;
            _pcbVoltage = pcbVoltage;
        }
    }

    class Controller : IDisposable
    {
        private SerialPort _serial;
        private ControllerState _state;

        public Controller(ControllerState state)
        {
            _state = state;

            _serial = new SerialPort("COM3", baudRate: 1_000_000);
            _serial.DiscardNull = false;
            _serial.ReadTimeout = 1000;
            _serial.ReadBufferSize = 64;
            _serial.WriteBufferSize = 64;
            _serial.WriteTimeout = 1000;
            _serial.Encoding = Encoding.Default;
        }

        #region IDisposable

        public void Dispose()
        {
            Close();
            _serial.Dispose();
        }

        #endregion

        public void Open()
        {
            _state.SignalChange();
            _serial.Open();

            var pcbVersion = ReadPCBVerion();
            var pcbVoltage = ReadVoltage();

            _state.UpdateState(_serial.IsOpen, pcbVersion, pcbVoltage);
            _state.SignalChangeCompleted();
        }

        public void Close()
        {
            _state.SignalChange();
            _serial.Close();
            _state.UpdateState(_serial.IsOpen, 0, 0);
            _state.SignalChangeCompleted();
        }

        public void SetVoltage(Voltage voltage)
        {
            _state.SignalChange();
            switch (voltage)
            {
                case Voltage.Gameboy:
                    SetMode(Constants.VOLTAGE_5V);
                    break;
                case Voltage.GameboyAdvance:
                    SetMode(Constants.VOLTAGE_3V);
                    break;
                default:
                    return;
            }

            var pcbVersion = ReadPCBVerion();
            var pcbVoltage = ReadVoltage();

            _state.UpdateState(_serial.IsOpen, pcbVersion, pcbVoltage);
            _state.SignalChangeCompleted();
        }

        public CartridgeHeader ReadGameboyHeader()
        {
            var headerBuffer = new byte[385];
            var currentAddress = 0x000;
            var targetAddress = 0x180;

            _serial.DiscardInBuffer();
            _serial.DiscardOutBuffer();

            SetMode(Constants.COMMAND_RESET);
            SetMode(Constants.COMMAND_SET_START_ADDRESS, currentAddress);
            SetMode(Constants.COMMAND_READ_ROM_RAM);

            while (currentAddress < targetAddress)
            {
                try
                {
                    var numberRead = _serial.Read(headerBuffer, currentAddress, 64);
                    Debug.WriteLine($"Read {numberRead} bytes!");

                    currentAddress += numberRead;

                    if (currentAddress < targetAddress)
                    {
                        SetMode(Constants.COMMAND_CONTINUE);
                    }
                }
                catch (TimeoutException)
                {
                    Debug.WriteLine("Bytes not ready!");
                    Thread.Sleep(500);

                    SetMode(Constants.COMMAND_SET_START_ADDRESS, (byte)currentAddress);
                    SetMode(Constants.COMMAND_READ_ROM_RAM);

                    continue;
                }
            }

            SetMode(Constants.COMMAND_RESET);

            CartridgeType cartType;
            switch (headerBuffer[0x143])
            {
                case 0x80:
                    cartType = CartridgeType.GameboyEnhanced;
                    break;
                case 0xC0:
                    cartType = CartridgeType.GameboyColor;
                    break;
                default:
                    cartType = CartridgeType.Gameboy;
                    break;
            }

            var title = Encoding.ASCII.GetString(headerBuffer, 0x134, cartType == CartridgeType.Gameboy ? 16 : 15);


            return new CartridgeHeader
            {
                Title = title,
                Type = cartType,
                SGBEnhanced = headerBuffer[0x146] == 0x03,
                Mapper = (CartridgeMapperType)headerBuffer[0x147]
            };
        }

        private byte ReadPCBVerion()
        {
            return RequestValue(Constants.COMMAND_READ_PCB_VERSION);
        }

        private void ReadFirmwareVersion()
        {
            // TODO
        }

        private byte ReadVoltage()
        {
            return RequestValue(Constants.COMMAND_MODE_CART);
        }

        private byte RequestValue(byte command)
        {
            SetMode(command);
            var value = (byte)_serial.ReadByte();
            _serial.DiscardInBuffer();

            return value;
        }

        private void SetMode(byte command)
        {
            var commandText = $"{(char)command}";
            _serial.Write(commandText);
        }

        private void SetMode(byte command, int number)
        {
            var commandText = string.Format("{0}{1:x}", (char)command, number);
            _serial.Write(commandText);
            _serial.Write(new byte[] { 0 }, 0, 1);
        }
    }
}
