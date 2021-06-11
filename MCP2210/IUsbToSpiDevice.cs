
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

namespace MCP2210 {
    /// <summary>
    /// The MCP2210 device.
    /// </summary>
    public interface IUsbToSpiDevice {
        /// <summary>
        /// This method connects the device.
        /// </summary>
        /// <param name="password">The password to access the device.</param>
        void Connect(string password = null);

        /// <summary>
        /// This method disconnects from the device.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// The number of general purpose lines.
        /// </summary>
        int NumberOfGeneralPurposeLines { get; }

        /// <summary>
        /// The maximum password length.
        /// </summary>
        int MaximumPasswordLength { get; }

        /// <summary>
        /// This method is used to access the device with a password.
        /// </summary>
        /// <param name="password">The access password.</param>
        void AccessWithPassword(string password);

        /// <summary>
        /// The non volatile RAM module.
        /// </summary>
        INonVolatileRam NonVolatileRam { get; }

        /// <summary>
        /// The  volatile RAM module.
        /// </summary>
        IVolatileRam VolatileRam { get; }

        /// <summary>
        /// The external interrupt pin module.
        /// </summary>
        IExternalInterruptPin ExternalInterruptPin { get; }

        /// <summary>
        /// The SPI data transfer module.
        /// </summary>
        ISpiDataTransfer SpiDataTransfer { get; }

        /// <summary>
        /// The EEPROM handler.
        /// </summary>
        IEepromMemory EEPROM { get; }
    }
}
