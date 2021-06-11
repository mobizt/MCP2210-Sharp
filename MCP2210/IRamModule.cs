
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
    /// The generic RAM module.
    /// </summary>
    public interface IRamModule {
        /// <summary>
        /// This method configures the chip settings, such as the
        /// pins modes and the pins direction (I/O).
        /// </summary>
        /// <param name="settings">The chip settings.</param>
        void ConfigureChip(ChipSettings settings);

        /// <summary>
        /// This method reads the current chip configuration.
        /// </summary>
        /// <returns>The current chip configuration.</returns>
        ChipSettings ReadChipConfiguration();

        /// <summary>
        /// This method configure the SPI transaction.
        /// </summary>
        /// <param name="setup">The SPI setup configuration.</param>
        void ConfigureSpi(SpiSetup setup);

        /// <summary>
        /// This method returns the current SPI configuration.
        /// </summary>
        SpiSetup ReadSpiConfiguration();
    }

    public enum SpiModes {
        /// <summary>
        ///  CPOL = 0
        ///  CPHA = 0
        /// </summary>
        Spi0 = 0,

        /// <summary>
        ///  CPOL = 1
        ///  CPHA = 0
        /// </summary>
        Spi1,

        /// <summary>
        ///  CPOL = 0
        ///  CPHA = 1
        /// </summary>
        Spi2,

        /// <summary>
        ///  CPOL = 1
        ///  CPHA = 1
        /// </summary>
        Spi3
    }

    public struct SpiSetup {
        /// <summary>
        /// The SPI mode.
        /// </summary>
        public SpiModes Mode;

        /// <summary>
        /// The SPI bitrate.
        /// </summary>
        public long BitRate;

        /// <summary>
        /// The number of bytes to transfer.
        /// </summary>
        public int BytesToTransfer;

        /// <summary>
        /// An array that represents the status of the chip select lines
        /// when they are idle.
        /// </summary>
        public bool[] IdleChipSelectValues;

        /// <summary>
        /// An array that represents the status of the chip select lines
        /// when they are active.
        /// </summary>
        public bool[] ActiveChipSelectValues;

        /// <summary>
        /// Delay in us between chip select and data byte. Quanta is 100us.
        /// i.e. set 5 for 500us.
        /// </summary>
        public int ChipSelectToDataDelay;

        /// <summary>
        /// Delay in us between data bytes. Quanta is 100us.
        /// i.e. set 5 for 500us.
        /// </summary>
        public int BetweenDataDelay;

        /// <summary>
        /// Delay in us between the last data byte and chip select. Quanta is 100us.
        /// i.e. set 5 for 500us.
        /// </summary>
        public int DataToChipSelectDelay;
    }

    /// <summary>
    /// The degital pin modes.
    /// </summary>
    public enum PinMode {
        /// <summary>
        /// General purpose I/O.
        /// </summary>
        GPIO = 0,

        /// <summary>
        /// SPI chip select.
        /// </summary>
        ChipSelects,

        /// <summary>
        /// Dedicated device function.
        /// </summary>
        DedicatedFunction
    }

    /// <summary>
    /// The digital pin direction.
    /// </summary>
    public enum PinDirection {
        Output = 0,
        Input
    }

    /// <summary>
    /// The dedicated functions of the general purpose lines.
    /// </summary>
    public enum DedicatedFunction {
        NoInterruptCounting = 0,
        CountFallingEdges,
        CountRisingEdges,
        CountLowPulses,
        CountHighPulses
    }
    
    public enum NramChipAccessControl {
        /// <summary>
        /// Chip settings not protected.
        /// </summary>
        NotProtected,

        /// <summary>
        /// Chip settings protected by password access.
        /// </summary>
        PasswordProtected,

        /// <summary>
        /// Chip settings permanently locked.
        /// </summary>
        PermanentlyLocked
    }

    /// <summary>
    /// The chip settings.
    /// </summary>
    public struct ChipSettings {
        /// <summary>
        /// The pin modes array.
        /// </summary>
        public PinMode[] PinModes;

        /// <summary>
        /// The pin directions array.
        /// </summary>
        public PinDirection[] PinDirections;

        /// <summary>
        /// The default output of the pins.
        /// </summary>
        public bool[] DefaultOutput;

        /// <summary>
        /// Enable/Disable the remote wake up.
        /// </summary>
        public bool RemoteWakeUpEnabled;

        /// <summary>
        /// The interrupt bit mode.
        /// </summary>
        public DedicatedFunction InterruptBitMode;

        /// <summary>
        /// If true, the SPI bus is released between transfer.
        /// </summary>
        public bool SpiBusReleaseEnable;

        /// <summary>
        /// The password to use for locking the device.
        /// This parameters is not used when RAM is volatile,
        /// as well when the owner structure is a reply.
        /// </summary>
        public string Password;

        /// <summary>
        /// This status is used to protected the device.
        /// If writing the chip settings, the access control
        /// will be used according with the Password setting.
        /// If reading the chip settings, the access control
        /// will be set with the current access control.
        /// </summary>
        public NramChipAccessControl AccessControl; 
    }
}
