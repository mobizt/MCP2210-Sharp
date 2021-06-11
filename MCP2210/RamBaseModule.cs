
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

namespace MCP2210 {
    class RamBaseModule {
        private readonly bool _nonVolatileRam;
        private readonly IHidCommunicationHandler _hidHandler;

        public RamBaseModule(IHidCommunicationHandler handler, bool nonVolatileRam) {
            _hidHandler = handler;
            _nonVolatileRam = nonVolatileRam;
        }

        #region Private conversion routines

        private static SpiSetup FromPacketToSpiConfiguration(byte[] packet) {
            SpiSetup setup = new SpiSetup();

            // read the bitrate
            setup.BitRate = Convert.ToInt64(BitConverter.ToUInt32(packet, 4));

            // read the chip select values
            setup.ActiveChipSelectValues = new bool[Constants.NumberOfGeneralPurposeLines];
            setup.IdleChipSelectValues = new bool[Constants.NumberOfGeneralPurposeLines];

            ushort idle = BitConverter.ToUInt16(packet, 8);
            ushort active = BitConverter.ToUInt16(packet, 10);

            for (int i = 0; i < Constants.NumberOfGeneralPurposeLines; i++) {
                ushort activeMask = (ushort)Math.Pow(2, i);
                setup.ActiveChipSelectValues[i] = (active & activeMask) == activeMask;
                
                ushort idleMask = (ushort)Math.Pow(2, i);
                setup.IdleChipSelectValues[i] = (idle & idleMask) == idleMask;
            }

            // read the delays
            setup.ChipSelectToDataDelay = Convert.ToInt32(BitConverter.ToUInt16(packet, 12));
            setup.DataToChipSelectDelay = Convert.ToInt32(BitConverter.ToUInt16(packet, 14));
            setup.BetweenDataDelay = Convert.ToInt32(BitConverter.ToUInt16(packet, 16));

            // read the bytes to trasnfer
            setup.BytesToTransfer = Convert.ToInt32(BitConverter.ToUInt16(packet, 18));

            // read the spi mode
            switch (packet[20]) {
                case 0:
                    setup.Mode = SpiModes.Spi0;
                    break;
                case 1:
                    setup.Mode = SpiModes.Spi1;
                    break;
                case 2:
                    setup.Mode = SpiModes.Spi2;
                    break;
                case 3:
                    setup.Mode = SpiModes.Spi3;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return setup;
        }

        private static void FromSpiConfigurationToPacket(SpiSetup setup, ref byte[] packet) {
            // setup bitrate
            byte[] bitrateBytes = BitConverter.GetBytes(Convert.ToUInt32(setup.BitRate));
            packet[4] = bitrateBytes[0];
            packet[5] = bitrateBytes[1];
            packet[6] = bitrateBytes[2];
            packet[7] = bitrateBytes[3];
            
            // set chip select status
            ushort activeCS = 0;
            ushort idleCS = 0;
            for (int i = 0; i < Constants.NumberOfGeneralPurposeLines; i++) {
                if (setup.ActiveChipSelectValues[i]) {
                    activeCS += (ushort)(1 << i);
                }
                if (setup.IdleChipSelectValues[i]) {
                    idleCS += (ushort)(1 << i);
                }
            }

            byte[] idleCSBytes = BitConverter.GetBytes(Convert.ToUInt16(idleCS));
            packet[8] = idleCSBytes[0];
            packet[9] = idleCSBytes[1];

            byte[] activeCSBytes = BitConverter.GetBytes(Convert.ToUInt16(activeCS));
            packet[10] = activeCSBytes[0];
            packet[11] = activeCSBytes[1];

            // convert delay into 16bits data, considering 100us as the quanta
            byte[] csToDDelayBytes = BitConverter.GetBytes(Convert.ToUInt16(setup.ChipSelectToDataDelay));
            packet[12] = csToDDelayBytes[0];
            packet[13] = csToDDelayBytes[1];

            byte[] dToCsDelayBytes = BitConverter.GetBytes(Convert.ToUInt16(setup.DataToChipSelectDelay));
            packet[14] = dToCsDelayBytes[0];
            packet[15] = dToCsDelayBytes[1];

            byte[] betweenDataDelayBytes = BitConverter.GetBytes(Convert.ToUInt16(setup.BetweenDataDelay));
            packet[16] = betweenDataDelayBytes[0];
            packet[17] = betweenDataDelayBytes[1];

            // set bytes to transfer
            byte[] bytesToTransfBytes = BitConverter.GetBytes(Convert.ToUInt16(setup.BytesToTransfer));
            packet[18] = bytesToTransfBytes[0];
            packet[19] = bytesToTransfBytes[1];

            // set spi mode
            switch (setup.Mode) {
                case SpiModes.Spi0:
                    packet[20] = 0;
                    break;
                case SpiModes.Spi1:
                    packet[20] = 1;
                    break;
                case SpiModes.Spi2:
                    packet[20] = 2;
                    break;
                case SpiModes.Spi3:
                    packet[20] = 3;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static ChipSettings FromPacketToChipSettings(byte[] packet) {
            // create the current settings instance
            ChipSettings settings = new ChipSettings();
            settings.PinDirections = new PinDirection[Constants.NumberOfGeneralPurposeLines];
            settings.PinModes = new PinMode[Constants.NumberOfGeneralPurposeLines];
            settings.DefaultOutput = new bool[Constants.NumberOfGeneralPurposeLines];

            ushort gpioOutput = BitConverter.ToUInt16(packet, 13);
            ushort gpioDirection = BitConverter.ToUInt16(packet, 15);
            for (int i = 0; i < Constants.NumberOfGeneralPurposeLines; i++) {
                // set pin modes
                switch (packet[4 + i]) {
                    case 0:
                        settings.PinModes[i] = PinMode.GPIO;
                        break;
                    case 1:
                        settings.PinModes[i] = PinMode.ChipSelects;
                        break;
                    case 2:
                        settings.PinModes[i] = PinMode.DedicatedFunction;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                // compute the current bit mask
                int bitMask =(int)Math.Pow(2, i);
                int pinStatus = 0;

                // set pin directions
                pinStatus = gpioDirection & bitMask;
                settings.PinDirections[i] = pinStatus == bitMask ?
                    PinDirection.Output : // 0
                    PinDirection.Input; // 1

                // set the default outputs
                pinStatus = gpioOutput & bitMask;
                settings.DefaultOutput[i] = pinStatus == bitMask;
            }

            // set other chip functionalities
            byte otherChipSettingByte = packet[17];
            settings.SpiBusReleaseEnable = (otherChipSettingByte & 1) == 0;
            settings.RemoteWakeUpEnabled = (otherChipSettingByte & 8) == 8;

            int dedfunctIndex = (otherChipSettingByte >> 1) & 7; // 0000 0111 mask
            switch (dedfunctIndex) {
                case 0:
                    settings.InterruptBitMode = DedicatedFunction.NoInterruptCounting;
                    break;
                case 1:
                    settings.InterruptBitMode = DedicatedFunction.CountFallingEdges;
                    break;
                case 2:
                    settings.InterruptBitMode = DedicatedFunction.CountRisingEdges;
                    break;
                case 3:
                    settings.InterruptBitMode = DedicatedFunction.CountLowPulses;
                    break;
                case 4:
                    settings.InterruptBitMode = DedicatedFunction.CountHighPulses;
                    break;
                default:
                    throw new NotImplementedException();
            }

            // set nvram access control
            switch (packet[18]) {
                case 0x00:
                    settings.AccessControl = NramChipAccessControl.NotProtected;
                    break;
                case 0x40:
                    settings.AccessControl = NramChipAccessControl.PasswordProtected;
                    break;
                case 0x80:
                    settings.AccessControl = NramChipAccessControl.PermanentlyLocked;
                    break;
            }

            return settings;
        }
        
        private static void FromChipSettingstoPacket(ChipSettings settings, ref byte[] packet) {
            ushort gpioOut = 0;
            ushort gpioDir = 0;
            for (int i = 0; i < Constants.NumberOfGeneralPurposeLines; i++) {
                PinMode mode = settings.PinModes[i];
                PinDirection direction = settings.PinDirections[i];

                // setup pin mode
                byte mbyte = 0;
                switch (mode) {
                    case PinMode.GPIO:
                        mbyte = 0;
                        break;
                    case PinMode.ChipSelects:
                        mbyte = 1;
                        break;
                    case PinMode.DedicatedFunction:
                        mbyte = 2;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                packet[i + 4] = mbyte;

                // create the GPIO configuration
                byte dirbyte = (byte)(direction == PinDirection.Output ? 0 : 1);
                ushort bit = (ushort)(dirbyte << i);
                gpioDir |= bit;

                // set the GPIO configuration
                if (settings.DefaultOutput[i]) {
                    gpioOut |= (ushort)(1 << i);
                } else {
                    gpioOut &= (ushort)~(1 << i);
                }
            }
            
            byte[] gpioOutBytes = BitConverter.GetBytes(gpioOut);
            packet[13] = gpioOutBytes[0];
            packet[14] = gpioOutBytes[1];

            byte[] gpioDirBytes = BitConverter.GetBytes(gpioDir);
            packet[15] = gpioDirBytes[0];
            packet[16] = gpioDirBytes[1];

            // set the other chip functionalities
            byte otherChipSettingsByte = 0x00;
            otherChipSettingsByte |= (byte)(settings.SpiBusReleaseEnable ? 0 : 1); // 0000 0001
            otherChipSettingsByte |= (byte)(settings.RemoteWakeUpEnabled ? 16 : 0); // 0000 1000

            switch (settings.InterruptBitMode) {
                case DedicatedFunction.CountFallingEdges:
                    otherChipSettingsByte |= 2; // 001
                    break;
                case DedicatedFunction.CountRisingEdges:
                    otherChipSettingsByte |= 3; // 010
                    break;
                case DedicatedFunction.CountLowPulses:
                    otherChipSettingsByte |= 4; // 011
                    break;
                case DedicatedFunction.CountHighPulses:
                    otherChipSettingsByte |= 5; // 100
                    break;
                case DedicatedFunction.NoInterruptCounting:
                default:
                    otherChipSettingsByte |= 0x00; // 000
                    break;
            }

            packet[17] = otherChipSettingsByte;

            // set the Nvram chip parameters access control
            switch (settings.AccessControl) {
                case NramChipAccessControl.NotProtected:
                    packet[18] = 0x00;
                    break;
                case NramChipAccessControl.PasswordProtected:
                    packet[18] = 0x40;
                    break;
                case NramChipAccessControl.PermanentlyLocked:
                    packet[18] = 0x80;
                    break;
            }
        }

        #endregion

        /// <summary>
        /// This method configures the chip.
        /// </summary>
        /// <param name="settings">The chip settings.</param>
        /// <param name="command">The writing configuration command.</param>
        /// <param name="subcommand">The sub command configuration. If 0, it's not used.</param>
        protected void ConfigureChip(ChipSettings settings,byte command, byte subcommand = 0) {
            // check if the input argument has 9 length array configuration
            if (settings.PinDirections == null || settings.PinDirections.Length != Constants.NumberOfGeneralPurposeLines) {
                throw new ArgumentException("Expected non null and " + Constants.NumberOfGeneralPurposeLines + " length pins directions array.");
            }

            if (settings.PinModes == null || settings.PinModes.Length != Constants.NumberOfGeneralPurposeLines) {
                throw new ArgumentException("Expected non null and " + Constants.NumberOfGeneralPurposeLines + " length pins modes array.");
            }

            if (settings.DefaultOutput == null || settings.DefaultOutput.Length != Constants.NumberOfGeneralPurposeLines) {
                throw new ArgumentException("Expected non null and " + Constants.NumberOfGeneralPurposeLines + " length pins default output array.");
            }

            // create the packet
            byte[] packet = new byte[Constants.PacketsSize];
            packet[0] = command;
            packet[1] = subcommand;

            FromChipSettingstoPacket(settings, ref packet);

            // set the password according with the access control byte (the 18th byte)
            string password = settings.Password;
            if (_nonVolatileRam && settings.AccessControl != NramChipAccessControl.NotProtected) {
                if (string.IsNullOrEmpty(password)) {
                    // reset password
                    for (int i = 0; i < Constants.MaximumPasswordLength; i++) {
                        packet[i + 19] = 0;
                    }
                } else {
                    // setup password
                    string subPassword = "";
                    if (password.Length > Constants.MaximumPasswordLength) {
                        subPassword = password.Substring(0, Constants.MaximumPasswordLength);
                    } else {
                        subPassword = password;
                    }

                    for (int i = 0; i < Constants.MaximumPasswordLength; i++) {
                        if (password.Length > i) {
                            packet[i + 19] = BitConverter.GetBytes(password[i])[0];
                        } else {
                            packet[i + 19] = 0x00;
                        }
                    }
                }
            }

            // write the new setup and read the reply
            byte[] settingsReply = _hidHandler.WriteData(packet);

            // check if package format is faulty
            if (settingsReply[0] != command || (subcommand != 0 && settingsReply[2] != subcommand)) {
                throw new PacketReplyFormatException();
            }

            // check for errors
            switch (settingsReply[1]) {
                case ReplyStatusCodes.CompletedSuccessfully:
                    break;
                case ReplyStatusCodes.BlockedAccess:
                    throw new AccessBlockedException();
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// This method configures the SPI bus.
        /// </summary>
        /// <param name="setup">The SPI setup.</param>
        /// <param name="command">The configuration command.</param>
        /// <param name="subcommand">The sub command configuration. If 0, it's not used.</param>
        protected void ConfigureSpi(SpiSetup setup, byte command, byte subcommand = 0) {
            // check input argument
            if (setup.ActiveChipSelectValues == null ||
                setup.ActiveChipSelectValues.Length != Constants.NumberOfGeneralPurposeLines) {
                throw new ArgumentException("Expected " + Constants.NumberOfGeneralPurposeLines + " active CS lines.");
            }

            if (setup.IdleChipSelectValues == null ||
                setup.IdleChipSelectValues.Length != Constants.NumberOfGeneralPurposeLines) {
                throw new ArgumentException("Expected " + Constants.NumberOfGeneralPurposeLines + " idle CS lines.");
            }

            if (setup.BetweenDataDelay < 0) {
                setup.BetweenDataDelay = 0;
            }

            if (setup.DataToChipSelectDelay < 0) {
                setup.DataToChipSelectDelay = 0;
            }

            if (setup.ChipSelectToDataDelay < 0) {
                setup.ChipSelectToDataDelay = 0;
            }

            if (setup.BitRate < 0) {
                setup.BitRate = 0;
            }

            if (setup.BytesToTransfer < 1) {
                setup.BytesToTransfer = 12; // default value
            }

            // create the packet
            byte[] packet = new byte[Constants.PacketsSize];
            packet[0] = command;
            packet[1] = subcommand;
            FromSpiConfigurationToPacket(setup, ref packet);

            // write the packet
            byte[] reply = _hidHandler.WriteData(packet);

            // check if package format is faulty
            if (reply[0] != command || (subcommand != 0 && reply[2] != subcommand)) {
                throw new PacketReplyFormatException();
            }

            // check for errors
            switch (reply[1]) {
                case ReplyStatusCodes.CompletedSuccessfully:
                    break;
                case ReplyStatusCodes.BlockedAccess:
                    throw new AccessBlockedException();
                case ReplyStatusCodes.UsbTransferInProgress:
                    throw new UsbtransferInProgressException();
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// This method reads the chip configuration.
        /// </summary>
        /// <param name="command">The configuration command.</param>
        /// <param name="subcommand">The sub command configuration. If 0, it's not used.</param>
        /// <returns>The chip settings.</returns>
        protected ChipSettings ReadChipConfiguration(byte command, byte subcommand = 0) {
            // create the packet
            byte[] packet = new byte[Constants.PacketsSize];
            packet[0] = command;
            packet[1] = subcommand;

            // write the packet
            byte[] reply = _hidHandler.WriteData(packet);

            // check if package format is faulty
            if (reply[0] != command || (subcommand != 0 && reply[2] != subcommand)) {
                throw new PacketReplyFormatException();
            }

            // create the current settings instance
            ChipSettings settings = FromPacketToChipSettings(reply);
            return settings;
        }

        /// <summary>
        /// This method reads the SPI bus configuration.
        /// </summary>
        /// <param name="command">The configuration command.</param>
        /// <param name="subcommand">The sub command configuration. If 0, it's not used.</param>
        /// <returns>The SPI bus configuration.</returns>
        protected SpiSetup ReadSpiConfiguration(byte command, byte subcommand = 0) {
            // create the packet
            byte[] packet = new byte[Constants.PacketsSize];
            packet[0] = command;
            packet[1] = subcommand;

            // write the packet
            byte[] reply = _hidHandler.WriteData(packet);

            // check if package format is faulty
            if (reply[0] != command || (subcommand != 0 && reply[2] != subcommand)) {
                throw new PacketReplyFormatException();
            }

            // create the spi setup
            SpiSetup setup = FromPacketToSpiConfiguration(reply);
            return setup;
        }
    }
}
