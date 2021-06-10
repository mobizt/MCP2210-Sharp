
// Copyright(c) 2016 Andrea Cervesato <sawk.ita@gmail.com>

// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:

// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Text;

namespace MCP2210 {
    class NonVolatileRam : RamBaseModule, INonVolatileRam {
        private readonly IHidCommunicationHandler _hidHandler;

        public NonVolatileRam(IHidCommunicationHandler handler) :
            base (handler, true) {
            _hidHandler = handler;
        }

        #region Private

        private string ReadString(byte subcommand) {
            // create the packet
            byte[] packet = new byte[Constants.PacketsSize];
            packet[0] = CommandCodes.ReadChipParameters;
            packet[1] = subcommand;
            byte[] reply = _hidHandler.WriteData(packet);

            // check if package format is faulty
            if (reply[0] != CommandCodes.ReadChipParameters || reply[2] != subcommand) {
                throw new PacketReplyFormatException();
            }

            int length = reply[4];
            string desc = Encoding.Unicode.GetString(reply, 6, length - 2);
            return desc;
        }

        //SET USB MANUFACTURER NAME
        private void WriteString(byte subcommand, string name) {
            // create the packet
            // command byte length is 64
            byte[] packet = new byte[Constants.PacketsSize];

            //0x60 – Set Chip NVRAM Parameters – command code
            packet[0] = CommandCodes.WriteChipParameters;
            //0x50 – Set USB Manufacturer Name – sub-command code
            packet[1] = subcommand;

            //0x00 – Reserved
            //packet[2] = 0;
            //0x00 – Reserved
            //packet[3] = 0;

            // set the length of the string
            int length = name.Length * 2 + 2;

            //Total USB String Descriptor Length (this is the length of the Manufacturer string, multiplied by 2 + 2)
            packet[4] = (byte)(length);

            // from manual DS22288A-page 20: always fill the 6th byte with 0x03
            //USB String Descriptor ID – always fill with 0x03
            packet[5] = 0x03;

            // set the string
            //fill the unicode Character Low Byte followed by High Byte for each char
            byte[] nameBytes = Encoding.Unicode.GetBytes(name);
            for (int i = 0; i < nameBytes.Length; i++) {
                if (i > Constants.PacketsSize) {
                    break;
                }
                packet[6 + i] = nameBytes[i];
            }

            // write the data
            byte[] reply = _hidHandler.WriteData(packet);

            // check for errors
            //0x60 – Set Chip NVRAM Parameters – echos back the given command code
            if (reply[0] != CommandCodes.WriteChipParameters) {
                throw new PacketReplyFormatException();
            }

            switch (reply[1]) {
                //0x00 – Command Completed Successfully – settings written
                case ReplyStatusCodes.CompletedSuccessfully:
                    break;
                //0xFB – Blocked Access – The provided password is not matching the one stored in the chip or the
                //settings are permanently locked.
                case ReplyStatusCodes.BlockedAccess:
                    throw new AccessBlockedException();
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region Public

        public string ManufacterName {
            get {
                string str = ReadString(SubCommandCodes.UsbManufacturerName);
                return str;
            }

            set {
                if (string.IsNullOrEmpty(value)) {
                    return;
                }

                WriteString(SubCommandCodes.UsbManufacturerName, value);
            }
        }

        public string ProductName {
            get {
                string str = ReadString(SubCommandCodes.UsbProductName);
                return str;
            }

            set {
                if (string.IsNullOrEmpty(value)) {
                    return;
                }

                WriteString(SubCommandCodes.UsbProductName, value);
            }
        }

        public void ConfigureChip(ChipSettings settings) {
            ConfigureChip(
                settings,
                CommandCodes.WriteChipParameters,
                SubCommandCodes.ChipSettingsPowerUpDefault);
        }

        public ChipSettings ReadChipConfiguration() {
            ChipSettings settings = ReadChipConfiguration(
                CommandCodes.ReadChipParameters,
                SubCommandCodes.ChipSettingsPowerUpDefault);
            return settings;
        }

        public void ConfigureSpi(SpiSetup setup) {
            ConfigureSpi(
                setup, 
                CommandCodes.WriteChipParameters, 
                SubCommandCodes.SpiPowerUpTransferSettings);
        }

        public SpiSetup ReadSpiConfiguration() {
            SpiSetup setup = ReadSpiConfiguration(
                CommandCodes.ReadChipParameters, 
                SubCommandCodes.SpiPowerUpTransferSettings);
            return setup;
        }

        public UsbKeyPowerSettings ReadUsbSettings() {
            // create the packet
            byte[] packet = new byte[Constants.PacketsSize];
            packet[0] = CommandCodes.ReadChipParameters;
            packet[1] = SubCommandCodes.UsbPowerUpKeyParameters;
            byte[] reply = _hidHandler.WriteData(packet);

            // check if package format is faulty
            if (reply[0] != CommandCodes.ReadChipParameters ||
                reply[2] != SubCommandCodes.UsbPowerUpKeyParameters) {
                throw new PacketReplyFormatException();
            }

            // create the settings
            UsbKeyPowerSettings settings = new UsbKeyPowerSettings();
            settings.VID = Convert.ToInt32(BitConverter.ToUInt16(reply, 12));
            settings.PID = Convert.ToInt32(BitConverter.ToUInt16(reply, 14));
            settings.RequestedCurrent = Convert.ToInt32(reply[30]) * 2; // 2mA Quanta
            settings.RemoteWakeUpCapable = (reply[29] & 32) == 32;
            settings.SelfPowered = (reply[29] & 64) == 64;
            settings.HostPowered = (reply[29] & 128) == 128;

            return settings;
        }

        public void WriteUsbSettings(UsbKeyPowerSettings settings) {
            // get VID & pid bytes
            byte[] vidBytes = BitConverter.GetBytes(Convert.ToUInt16(settings.VID));
            byte[] pidBytes = BitConverter.GetBytes(Convert.ToUInt16(settings.PID));

            // get chip power options
            byte cpOptionsByte = 0x00;
            cpOptionsByte |= (byte)(settings.RemoteWakeUpCapable ? 32 : 0); // 0010 0000
            cpOptionsByte |= (byte)(settings.SelfPowered ? 64 : 0); // 0100 0000
            cpOptionsByte |= (byte)(settings.HostPowered ? 128 : 0); // 1000 0000

            // get current bytes
            byte currentByte = Convert.ToByte(settings.RequestedCurrent / 2);

            // create the package
            byte[] packet = new byte[Constants.PacketsSize];
            packet[0] = CommandCodes.WriteChipParameters;
            packet[1] = SubCommandCodes.UsbPowerUpKeyParameters;
            packet[4] = vidBytes[0];
            packet[5] = vidBytes[1];
            packet[6] = pidBytes[0];
            packet[7] = pidBytes[1];
            packet[8] = cpOptionsByte;
            packet[9] = currentByte;

            // write the packet
            byte[] reply = _hidHandler.WriteData(packet);

            // check if package format is faulty
            if (reply[0] != CommandCodes.WriteChipParameters ||
                reply[2] != SubCommandCodes.UsbPowerUpKeyParameters) {
                throw new PacketReplyFormatException();
            }

            switch (reply[1]) {
                case ReplyStatusCodes.CompletedSuccessfully:
                    break;
                case ReplyStatusCodes.BlockedAccess:
                    throw new AccessBlockedException();
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion
    }
}