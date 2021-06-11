
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
    class VolatileRam : RamBaseModule, IVolatileRam {
        private readonly IHidCommunicationHandler _hidHandler;

        public VolatileRam(IHidCommunicationHandler handler) :
            base (handler, true) {
            _hidHandler = handler;
        }

        #region Public

        public PinDirection[] CurrentGpioPinsDirection {
            get {
                // create the command
                byte[] packet = new byte[Constants.PacketsSize];
                packet[0] = CommandCodes.GetGpioCurrentDirectionSettings;

                // send the packet
                byte[] reply = _hidHandler.WriteData(packet);

                // according with the manual, this command never faults
                if (reply[0] != CommandCodes.GetGpioCurrentDirectionSettings ||
                    reply[1] != ReplyStatusCodes.CompletedSuccessfully) {
                    throw new NotImplementedException();
                }

                // read the directions
                PinDirection[] direction = new PinDirection[Constants.NumberOfGeneralPurposeLines];
                ushort directionValue = BitConverter.ToUInt16(reply, 4);
                for (int i = 0; i < Constants.NumberOfGeneralPurposeLines; i++) {
                    int bitMask = (int)Math.Pow(2, i);
                    if ((directionValue & bitMask) == bitMask) {
                        direction[i] = PinDirection.Output;
                    } else {
                        direction[i] = PinDirection.Input;
                    }
                }

                return direction;
            }

            set {
                // check input argument
                if (value == null || value.Length != Constants.NumberOfGeneralPurposeLines) {
                    throw new ArgumentException("Expected non-null and " + Constants.NumberOfGeneralPurposeLines + " length digital direction array.");
                }

                // create the packet
                byte[] packet = new byte[Constants.PacketsSize];
                packet[0] = CommandCodes.SetGpioCurrentDirectionSettings;

                PinDirection[] direction = value;
                ushort directionValue = 0;
                for (int i = 0; i < Constants.NumberOfGeneralPurposeLines; i++) {
                    ushort bitValue = (ushort)Math.Pow(2, i);
                    if (direction[i] == PinDirection.Output) {
                        directionValue |= bitValue;
                    }
                }

                byte[] dirBytes = BitConverter.GetBytes(directionValue);
                packet[4] = dirBytes[0];
                packet[5] = dirBytes[1];

                // send the packet
                byte[] reply = _hidHandler.WriteData(packet);

                // according with the manual, this command never faults
                if (reply[0] != CommandCodes.SetGpioCurrentDirectionSettings ||
                    reply[1] != ReplyStatusCodes.CompletedSuccessfully) {
                    throw new NotImplementedException();
                }
            }
        }

        public bool[] GpioPinsValue {
            get {
                // create the packet
                byte[] packet = new byte[Constants.PacketsSize];
                packet[0] = CommandCodes.GetGpioCurrentDirectionSettings;

                // send the packet
                byte[] reply = _hidHandler.WriteData(packet);

                // according with the manual, this command never faults
                if (reply[0] != CommandCodes.GetGpioCurrentDirectionSettings ||
                    reply[1] != ReplyStatusCodes.CompletedSuccessfully) {
                    throw new NotImplementedException();
                }

                // read the directions
                bool[] outputValue = new bool[Constants.NumberOfGeneralPurposeLines];
                ushort directionValue = BitConverter.ToUInt16(reply, 4);
                for (int i = 0; i < Constants.NumberOfGeneralPurposeLines; i++) {
                    ushort bitMask = (ushort)Math.Pow(2, i);
                    outputValue[i] = (directionValue & bitMask) == bitMask;
                }

                return outputValue;
            }

            set {
                // check input argument
                if (value == null || value.Length != Constants.NumberOfGeneralPurposeLines) {
                    throw new ArgumentException("Expected non-null and " + Constants.NumberOfGeneralPurposeLines + " length output array.");
                }

                // create the packet
                byte[] packet = new byte[Constants.PacketsSize];
                packet[0] = CommandCodes.SetGpioCurrentDirectionSettings;

                bool[] outputValue = value;
                ushort directionValue = 0;
                for (int i = 0; i < Constants.NumberOfGeneralPurposeLines; i++) {
                    ushort bitValue = (ushort)Math.Pow(2, i);
                    if (outputValue[i]) {
                        directionValue |= bitValue;
                    }
                }

                byte[] dirBytes = BitConverter.GetBytes(directionValue);
                packet[4] = dirBytes[0];
                packet[5] = dirBytes[1];

                // send the packet
                byte[] reply = _hidHandler.WriteData(packet);

                // according with the manual, this command never faults
                if (reply[0] != CommandCodes.SetGpioCurrentDirectionSettings ||
                    reply[1] != ReplyStatusCodes.CompletedSuccessfully) {
                    throw new NotImplementedException();
                }
            }
        }

        public void ConfigureChip(ChipSettings settings) {
            ConfigureChip(settings, CommandCodes.SetGpioCurrentChipSettings);
        }

        public void ConfigureSpi(SpiSetup setup) {
            ConfigureSpi(setup, CommandCodes.SetSpiTransferSettings);
        }

        public ChipSettings ReadChipConfiguration() {
            ChipSettings settings = ReadChipConfiguration(CommandCodes.GetGpioCurrentChipSettings);
            return settings;
        }

        public SpiSetup ReadSpiConfiguration() {
            SpiSetup setup = ReadSpiConfiguration(CommandCodes.GetSpiTransferSettings);
            return setup;
        }

        #endregion
    }
}
