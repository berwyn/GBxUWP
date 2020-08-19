namespace GBxUWP
{
    public enum CartridgeType
    {
        Gameboy,
        GameboyEnhanced,
        GameboyColor,
        GameboyAdvance
    }

    public enum CartridgeMapperType : byte
    {
        ROMOnly = 0x00,

        MBC1 = 0x01,
        MBC1_RAM = 0x02,
        MBC1_RAM_BATTERY = 0x03,

        MBC2 = 0x05,
        MBC2_BATTERY = 0x06,

        ROM_RAM = 0x08,
        ROM_RAM_BATTERY = 0x09,

        MMM01 = 0x0B,
        MMM01_RAM = 0x0C,
        MMM01_RAM_BATTERY = 0x0D,

        MBC3_TIMER_BATTERY = 0x0F,
        MBC3_TIMER_RAM_BATTERY = 0x10,
        MBC3 = 0x11,
        MBC3_RAM = 0x12,
        MBC3_RAM_BATTERY = 0x13,

        MBC5 = 0x19,
        MBC5_RAM = 0x1A,
        MBC5_RAM_BATTERY = 0x1B,
        MBC5_RUMBLE = 0x1C,
        MBC5_RUMBLE_RAM = 0x1D,
        MBC5_RUMBLE_RAM_BATTERY = 0x1E,

        MBC6 = 0x20,

        MBC7_SENSOR_RUMBLE_RAM_BATTERY = 0x22,

        POCKET_CAMERA = 0xFC,
        BANDAI_TAMA5 = 0xFD,
        HuC3 = 0xFE,
        HuC1_RAM_BATTERY = 0xFF
    }

    static class CartridgeMapperTypeExtensions
    {
        public static string DisplayName(this CartridgeMapperType type)
        {
            switch (type)
            {
                case CartridgeMapperType.ROMOnly:
                    return "ROM";

                case CartridgeMapperType.MBC1:
                    return "MBC1";
                case CartridgeMapperType.MBC1_RAM:
                    return "MBC1 + RAM";
                case CartridgeMapperType.MBC1_RAM_BATTERY:
                    return "MBC1 + RAM + Battery";

                case CartridgeMapperType.MBC2:
                    return "MBC2";
                case CartridgeMapperType.MBC2_BATTERY:
                    return "MBC2 + Battery";

                case CartridgeMapperType.ROM_RAM:
                    return "ROM + RAM";
                case CartridgeMapperType.ROM_RAM_BATTERY:
                    return "ROM + RAM + Battery";

                case CartridgeMapperType.MMM01:
                    return "MMM01";
                case CartridgeMapperType.MMM01_RAM:
                    return "MMM01 + RAM";
                case CartridgeMapperType.MMM01_RAM_BATTERY:
                    return "MMM01 + RAM + Battery";

                case CartridgeMapperType.MBC3_TIMER_BATTERY:
                    return "MBC3 + Timer + Battery";
                case CartridgeMapperType.MBC3_TIMER_RAM_BATTERY:
                    return "MBC3 + Timer + RAM + Battery";
                case CartridgeMapperType.MBC3:
                    return "MBC3";
                case CartridgeMapperType.MBC3_RAM:
                    return "MBC3 + RAM";
                case CartridgeMapperType.MBC3_RAM_BATTERY:
                    return "MBC3 + RAM + Battery";

                case CartridgeMapperType.MBC5:
                    return "MBC5";
                case CartridgeMapperType.MBC5_RAM:
                    return "MBC5 + RAM";
                case CartridgeMapperType.MBC5_RAM_BATTERY:
                    return "MBC5 + RAM + Battery";
                case CartridgeMapperType.MBC5_RUMBLE:
                    return "MBC5 + Rumble";
                case CartridgeMapperType.MBC5_RUMBLE_RAM:
                    return "MBC5 + Rumble + RAM";
                case CartridgeMapperType.MBC5_RUMBLE_RAM_BATTERY:
                    return "MBC5 + Rumble + RAM + Battery";

                case CartridgeMapperType.MBC6:
                    return "MBC6";

                case CartridgeMapperType.MBC7_SENSOR_RUMBLE_RAM_BATTERY:
                    return "MBC7 + Sensor + Rumble + RAM + Battery";

                case CartridgeMapperType.POCKET_CAMERA:
                    return "Pocket Camera";
                case CartridgeMapperType.BANDAI_TAMA5:
                    return "Bandai Tama 5";
                case CartridgeMapperType.HuC3:
                    return "HuC3";
                case CartridgeMapperType.HuC1_RAM_BATTERY:
                    return "HuC1 + RAM + Battery";

                default:
                    return "Unknown";
            }
        }
    }

    public class CartridgeHeader
    {
        public string Title { get; set; }
        public CartridgeType Type { get; set; }
        public bool SGBEnhanced { get; set; }
        public CartridgeMapperType Mapper { get; set; }
        public uint RomBanks { get; set; }
        public uint RamBanks { get; set; }
        public bool ChecksumValid { get; set; }

        public string RomText
        {
            get
            {
                switch (RomBanks)
                {
                    case 0:
                        return "32KB";
                    case 4:
                        return "64KB (4 banks)";
                    case 8:
                        return "128KB (8 banks)";
                    case 16:
                        return "256KB (16 banks)";
                    case 32:
                        return "512KB (32 banks)";
                    case 63:
                        return "1MB (63 banks)";
                    case 64:
                        return "1MB (64 banks)";
                    case 125:
                        return "2MB (125 banks)";
                    case 128:
                        return "2MB (128 banks)";
                    case 256:
                        return "4MB (256 banks)";

                    case 72:
                        return "1.1MB (72 banks)";
                    case 80:
                        return "1.2MB (80 banks)";
                    case 96:
                        return "1.5MB (96 banks)";

                    default:
                        return "Unknown";
                }
            }
        }

        public string RamText
        {
            get
            {
                switch (RamBanks)
                {
                    case 0:
                        return "0KB";
                    case 1:
                        return "8KB";
                    case 4:
                        return "32KB (4 banks of 8KB)";
                    case 16:
                        return "128KB (16 banks of 8KB)";
                    case 8:
                        return "64KB (8 banks of 8KB)";
                    default:
                        return "Unknown";
                }
            }
        }

        public string ChecksumText
        {
            get
            {
                return ChecksumValid ? "OK" : "Invalid";
            }
        }
    }

    public class Cartridge
    {
        public CartridgeHeader Header { get; set; }

        private byte[] _buffer;
    }
}
