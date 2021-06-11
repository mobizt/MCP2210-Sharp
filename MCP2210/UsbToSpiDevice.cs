
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
    public class UsbToSpiDevice : IUsbToSpiDevice {
        private readonly IHidCommunicationHandler _hidComHandler;
        private readonly IExternalInterruptPin _extInterruptPin;
        private readonly ISpiDataTransfer _spiDataTransfer;
        private readonly INonVolatileRam _nvram;
        private readonly IVolatileRam _ram;
        private readonly IEepromMemory _eeprom;

        public UsbToSpiDevice() {
            _hidComHandler = new HidSharpCommunicationHandler();
            _nvram = new NonVolatileRam(_hidComHandler);
            _ram = new VolatileRam(_hidComHandler);
            _extInterruptPin = new ExternalInterruptPin(_hidComHandler);
            _spiDataTransfer = new SpiDataTransfer(_hidComHandler);
            _eeprom = new EepromMemory(_hidComHandler);
        }

        ~UsbToSpiDevice() {
            _hidComHandler.Close();
        }

        public IExternalInterruptPin ExternalInterruptPin {
            get { return _extInterruptPin; }
        }

        public INonVolatileRam NonVolatileRam {
            get { return _nvram; }
        }

        public IVolatileRam VolatileRam {
            get { return _ram; }
        }

        public ISpiDataTransfer SpiDataTransfer {
            get { return _spiDataTransfer; }
        }

        public void Connect(string password = null) {
            // open device
            _hidComHandler.Open();

            if (!string.IsNullOrEmpty(password)) {
                AccessWithPassword(password);
            }
        }

        public void Disconnect() {
            _hidComHandler.Close();
        }

        public int MaximumPasswordLength {
            get { return Constants.MaximumPasswordLength; }
        }

        public int NumberOfGeneralPurposeLines {
            get { return Constants.NumberOfGeneralPurposeLines; }
        }

        public IEepromMemory EEPROM {
            get { return _eeprom; }
        }

        public void AccessWithPassword(string password) {
            // create the packet
            byte[] packet = new byte[Constants.PacketsSize];
            packet[0] = CommandCodes.AccessPassword;

            string subPassword = "";
            if (password.Length > Constants.MaximumPasswordLength) {
                subPassword = password.Substring(0, Constants.MaximumPasswordLength);
            } else {
                subPassword = password;
            }

            for (int i = 0; i < Constants.MaximumPasswordLength; i++) {
                if (password.Length > i) {
                    packet[i + 4] = BitConverter.GetBytes(password[i])[0];
                } else {
                    packet[i + 4] = 0x00;
                }
            }

            // send password access
            byte[] reply = _hidComHandler.WriteData(packet);

            if (reply[0] != CommandCodes.AccessPassword) {
                throw new PacketReplyFormatException();
            }

            // check for errors
            switch (reply[1]) {
                case ReplyStatusCodes.CompletedSuccessfully:
                    break;
                case ReplyStatusCodes.AccessRejected:
                    throw new PasswordAccessRejectedException();
                case ReplyStatusCodes.PasswordDoesntMatchChipOnError:
                    throw new PasswordDoesntMatchChipOnException();
                case ReplyStatusCodes.PasswordDoesntMatchChipOnLockedError:
                    throw new PasswordMechanismIsLockedException();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
